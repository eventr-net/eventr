namespace EventR
{
    using EventR.Abstractions;
    using EventR.Abstractions.Exceptions;
    using EventR.Abstractions.Telemetry;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public sealed class EventStoreSession : IEventStoreSession
    {
        public EventStoreSession(
            IPersistence persistence,
            IProvideSerializers serializersProvider,
            ITelemetry telemetry,
            bool suppressAmbientTransaction = false)
        {
            this.persistence = persistence;
            this.serializersProvider = serializersProvider;
            this.telemetry = telemetry;
            SuppressAmbientTransaction = suppressAmbientTransaction;
        }

        private readonly IPersistence persistence;

        private readonly IProvideSerializers serializersProvider;
        private readonly ITelemetry telemetry;
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
                var load = await persistenceSession.LoadCommits(streamId).ConfigureAwait(false);

                EventsLoad result;
                var hasCommits = AnalyzeCommitsLoad(load, correlationId, streamId);
                if (hasCommits)
                {
                    var events = CreateEvents(load.Commits, correlationId, streamId);
                    result = new EventsLoad(events, load.Version);
                }
                else
                {
                    result = EventsLoad.Empty;
                }

                telemetry.TrackSuccess(Operation.LoadEvents, sw.Elapsed, correlationId, streamId);

                return result;
            }
            catch (Exception exception)
            {
                telemetry.TrackFailure(Operation.LoadEvents, sw.Elapsed, exception, correlationId, streamId);

                throw;
            }
        }

        private bool AnalyzeCommitsLoad(CommitsLoad load, string correlationId, string streamId)
        {
            if (load.IsEmpty)
            {
                telemetry.Track(Metric.EmptyStream, 1, correlationId, streamId);
                return false;
            }

            var streamLength = load.Commits.Length;
            telemetry.Track(Metric.CommitsPerLoad, streamLength, correlationId, streamId);

            var streamBytesCount = load.Commits.Sum(x => x.Payload.Length);
            telemetry.Track(Metric.BytesPerLoad, streamBytesCount, correlationId, streamId);

            if (ErrorOnStreamLength > 0 && streamLength >= ErrorOnStreamLength)
            {
                var message = $"event stream '{streamId}' is too long ({streamLength} commits >= {ErrorOnStreamLength} limit)";
                telemetry.Log(LogSeverity.Error, message, correlationId, streamId);
                telemetry.Track(Metric.StreamTooLong, 1, correlationId, streamId);
            }
            else if (WarnOnStreamLength > 0 && streamLength >= WarnOnStreamLength)
            {
                var message = $"event stream '{streamId}' is too long ({streamLength} commits >= {ErrorOnStreamLength} limit)";
                telemetry.Log(LogSeverity.Warning, message, correlationId, streamId);
                telemetry.Track(Metric.StreamTooLong, 1, correlationId, streamId);
            }

            return true;
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
                var isSaved = await persistenceSession.Save(commit).ConfigureAwait(false);
                if (isSaved)
                {
                    aggregate.MarkAsCommited();
                }

                telemetry.TrackSuccess(Operation.SaveUncommitedEvents, sw.Elapsed, correlationId, streamId);

                return isSaved;
            }
            catch (Exception exception)
            {
                if (exception is VersionConflictException)
                {
                    telemetry.Track(Metric.VersionConflict, 1, correlationId, streamId);
                }

                telemetry.TrackFailure(Operation.SaveUncommitedEvents, sw.Elapsed, exception, correlationId, streamId);

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
                var isDeleted = await persistenceSession.Delete(streamId).ConfigureAwait(false);
                if (isDeleted)
                {
                    aggregate.MarkAsDeleted();
                }

                telemetry.TrackSuccess(Operation.DeleteStream, sw.Elapsed, correlationId, streamId);

                return isDeleted;
            }
            catch (Exception exception)
            {
                telemetry.TrackFailure(Operation.DeleteStream, sw.Elapsed, exception, correlationId, streamId);

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
                var isDeleted = await persistenceSession.Delete(streamId).ConfigureAwait(false);

                telemetry.TrackSuccess(Operation.DeleteStream, sw.Elapsed, correlationId, streamId);

                return isDeleted;
            }
            catch (Exception exception)
            {
                telemetry.TrackFailure(Operation.DeleteStream, sw.Elapsed, exception, correlationId, streamId);
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

            telemetry.Track(Metric.SerializeTime, sw.ElapsedMilliseconds, correlationId, streamId);

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

            telemetry.Track(Metric.DeserializeTime, sw.ElapsedMilliseconds, correlationId, streamId);

            return all.ToArray();
        }
    }
}
