namespace EventR
{
    using EventR.Abstractions;
    using EventR.Abstractions.Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using static EventREventSource;

    public sealed class EventStoreSession : IEventStoreSession
    {
        public EventStoreSession(
            IPersistence persistence,
            IProvideSerializers serializersProvider,
            bool suppressAmbientTransaction = false)
        {
            this.persistence = persistence;
            this.serializersProvider = serializersProvider;
            SuppressAmbientTransaction = suppressAmbientTransaction;
        }

        private readonly IPersistence persistence;
        private readonly IProvideSerializers serializersProvider;
        private IPersistenceSession persistenceSession;

        private bool isDisposed;

        public bool SuppressAmbientTransaction { get; }

        public int WarnOnStreamLength { get; set; }

        public int ErrorOnStreamLength { get; set; }

        public async Task<bool> HydrateAsync<T, TDataSnaphot>(T aggregate, string correlationId = null)
            where T : AggregateRoot<TDataSnaphot>
            where TDataSnaphot : class, new()
        {
            var load = await LoadEventsAsync<T, TDataSnaphot>(aggregate).ConfigureAwait(false);
            if (load.Events.Length == 0)
            {
                return false;
            }

            aggregate.Hydrate(load);
            return true;
        }

        public async Task<EventsLoad> LoadEventsAsync<T, TDataSnaphot>(T aggregate, string correlationId = null)
            where T : AggregateRoot<TDataSnaphot>
            where TDataSnaphot : class, new()
        {
            var sw = Stopwatch.StartNew();
            string streamId = null;
            try
            {
                Expect.NotDisposed(isDisposed);
                Expect.NotNull(aggregate, nameof(aggregate));

                EnsureOpenPersistenceSession();
                streamId = aggregate.StreamId;
                var load = await persistenceSession.LoadCommitsAsync(streamId).ConfigureAwait(false);

                EventsLoad result;
                var (hasCommits, streamByteLength) = AnalyzeCommitsLoad(load, correlationId, streamId);
                if (hasCommits)
                {
                    var events = CreateEvents(load.Commits, correlationId, streamId);
                    result = new EventsLoad(events, load.Version);
                }
                else
                {
                    result = EventsLoad.Empty;
                }

                Log.LoadEvents(streamId, result.Events.Length, streamByteLength, sw.ElapsedMilliseconds, correlationId);

                return result;
            }
            catch (Exception ex)
            {
                Log.LoadEventsError(ex, streamId, sw.ElapsedMilliseconds, correlationId);
                throw;
            }
        }

        private (bool hasCommits, int streamByteLength) AnalyzeCommitsLoad(CommitsLoad load, string correlationId, string streamId)
        {
            if (load.IsEmpty)
            {
                return (false, 0);
            }

            var streamByteLength = load.Commits.Sum(x => x.Payload.Length);
            if (ErrorOnStreamLength > 0 && streamByteLength >= ErrorOnStreamLength)
            {
                Log.EventStreamTooLongError(streamId, streamByteLength, ErrorOnStreamLength, correlationId);
            }
            else if (WarnOnStreamLength > 0 && streamByteLength >= WarnOnStreamLength)
            {
                Log.EventStreamTooLongWarn(streamId, streamByteLength, WarnOnStreamLength, correlationId);
            }

            return (true, streamByteLength);
        }

        public async Task<bool> SaveUncommitedEventsAsync<T, TDataSnaphot>(T aggregate, string correlationId = null)
            where T : AggregateRoot<TDataSnaphot>
            where TDataSnaphot : class, new()
        {
            var sw = Stopwatch.StartNew();
            string streamId = null;
            try
            {
                Expect.NotDisposed(isDisposed);
                Expect.NotNull(aggregate, nameof(aggregate));

                if (!aggregate.HasUncommitedEvents)
                {
                    return true;
                }

                streamId = aggregate.StreamId;
                var commit = CreateCommit(aggregate.UncommitedEvents, aggregate.CurrentVersion, correlationId, streamId);

                EnsureOpenPersistenceSession();
                var isSaved = await persistenceSession.SaveAsync(commit).ConfigureAwait(false);
                if (isSaved)
                {
                    aggregate.MarkAsCommited();
                }

                Log.SaveUncommitedEvents(streamId, sw.ElapsedMilliseconds, correlationId);

                return isSaved;
            }
            catch (VersionConflictException vex)
            {
                Log.VersionConflict(streamId, vex.Version, correlationId);
                return false;
            }
            catch (Exception ex)
            {
                Log.SaveUncommitedEventsError(ex, streamId, sw.ElapsedMilliseconds, correlationId);
                throw;
            }
        }

        public async Task<bool> DeleteStreamAsync<T, TDataSnaphot>(T aggregate, string correlationId = null)
            where T : AggregateRoot<TDataSnaphot>
            where TDataSnaphot : class, new()
        {
            var sw = Stopwatch.StartNew();
            string streamId = null;
            try
            {
                Expect.NotDisposed(isDisposed);
                Expect.NotNull(aggregate, nameof(aggregate));

                EnsureOpenPersistenceSession();
                streamId = aggregate.StreamId;
                var isDeleted = await persistenceSession.DeleteAsync(streamId).ConfigureAwait(false);
                if (isDeleted)
                {
                    aggregate.MarkAsDeleted();
                }

                Log.DeleteStream(streamId, sw.ElapsedMilliseconds, correlationId);

                return isDeleted;
            }
            catch (Exception ex)
            {
                Log.DeleteStreamError(ex, streamId, sw.ElapsedMilliseconds, correlationId);
                throw;
            }
        }

        public async Task<bool> DeleteStreamAsync(string streamId, string correlationId = null)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                Expect.NotDisposed(isDisposed);
                Expect.NotEmpty(streamId, nameof(streamId));

                EnsureOpenPersistenceSession();
                var isDeleted = await persistenceSession.DeleteAsync(streamId).ConfigureAwait(false);

                Log.DeleteStream(streamId, sw.ElapsedMilliseconds, correlationId);

                return isDeleted;
            }
            catch (Exception ex)
            {
                Log.DeleteStreamError(ex, streamId, sw.ElapsedMilliseconds, correlationId);
                throw;
            }
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;

            if (persistenceSession != null)
            {
                persistenceSession.Dispose();
                persistenceSession = null;
            }
        }

        private void EnsureOpenPersistenceSession()
        {
            if (persistenceSession == null)
            {
                persistenceSession = persistence.OpenSession(SuppressAmbientTransaction);
            }
        }

        private Commit CreateCommit(object[] uncommitedEvents, int version, string correlationId, string streamId)
        {
            var sw = Stopwatch.StartNew();

            var commit = new Commit
            {
                Id = TimeGuid.NewId(),
                StreamId = streamId,
                Version = version,
            };
            serializersProvider.DefaultSerializer.Serialize(commit, uncommitedEvents);

            Log.SerializeCommit(streamId, uncommitedEvents.Length, sw.ElapsedMilliseconds, correlationId);

            return commit;
        }

        private object[] CreateEvents(Commit[] commits, string correlationId, string streamId)
        {
            var sw = Stopwatch.StartNew();

            var all = new List<object>(commits.Length * 2);
            foreach (var commit in commits)
            {
                var events = serializersProvider.Get(commit.SerializerId).Deserialize(commit);
                all.AddRange(events);
            }

            Log.DeserializeCommits(streamId, all.Count, sw.ElapsedMilliseconds, correlationId);

            return all.ToArray();
        }
    }
}
