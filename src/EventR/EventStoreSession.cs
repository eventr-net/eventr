namespace EventR
{
    using App.Metrics;
    using EventR.Abstractions;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public sealed class EventStoreSession : IEventStoreSession
    {
        public EventStoreSession(
            IPersistence persistence,
            IProvideSerializers serializersProvider,
            ILogger logger,
            IMetrics metrics,
            bool suppressAmbientTransaction = false)
        {
            Expect.NotNull(persistence, nameof(persistence));
            Expect.NotNull(serializersProvider, nameof(serializersProvider));
            Expect.NotNull(logger, nameof(logger));
            // metrics can be null; void implementation is too complex and it is taken care of in Metric class through which metrics are always used

            this.persistence = persistence;
            this.serializersProvider = serializersProvider;
            this.logger = logger;
            this.metrics = metrics;
            SuppressAmbientTransaction = suppressAmbientTransaction;
        }

        private readonly IPersistence persistence;

        private readonly IProvideSerializers serializersProvider;

        private readonly ILogger logger;

        private readonly IMetrics metrics;

        private IPersistenceSession persistenceSession;

        private bool isDisposed;

        public bool SuppressAmbientTransaction { get; }

        public int WarnOnStreamLength { get; set; }

        public int ErrorOnStreamLength { get; set; }

        public async Task<bool> Hydrate<T, TDataSnaphot>(T aggregate)
            where T : AggregateRoot<TDataSnaphot>
            where TDataSnaphot : class, new()
        {
            var load = await LoadEvents<T, TDataSnaphot>(aggregate).ConfigureAwait(false);
            if (load.Events.Length == 0)
            {
                return false;
            }

            aggregate.Hydrate(load);
            return true;
        }

        public async Task<EventsLoad> LoadEvents<T, TDataSnaphot>(T aggregate)
            where T : AggregateRoot<TDataSnaphot>
            where TDataSnaphot : class, new()
        {
            try
            {
                Expect.NotDisposed(isDisposed);
                Expect.NotNull(aggregate, nameof(aggregate));

                EnsureOpenPersistenceSession();

                CommitsLoad load;
                using (Metric.MeasureLoad(metrics))
                {
                    load = await persistenceSession.LoadCommits(aggregate.StreamId).ConfigureAwait(false);
                }

                Metric.CommitsPerLoad(metrics, load.Commits.Length);

                CheckStreamLength(aggregate.StreamId, load.Commits.Length);
                var events = CreateEvents(load.Commits);
                var result = new EventsLoad(events, load.Version);

                Metric.Success(metrics, Metric.Names.LoadOp);
                return result;
            }
            catch
            {
                Metric.Error(metrics, Metric.Names.LoadOp);
                throw;
            }
        }

        private void CheckStreamLength(string streamId, int streamLength)
        {
            if (ErrorOnStreamLength > 0 && streamLength >= ErrorOnStreamLength)
            {
                logger.LogError(
                    "event stream '{0}' is too long ({1} commits >= {2} limit)",
                    streamId,
                    streamLength,
                    ErrorOnStreamLength);
                Metric.StreamTooLong(metrics);
            }
            else if (WarnOnStreamLength > 0 && streamLength >= WarnOnStreamLength)
            {
                logger.LogWarning(
                    "event stream '{0}' is too long ({1} commits >= {2} limit)",
                    streamId,
                    streamLength,
                    WarnOnStreamLength);
                Metric.StreamTooLong(metrics);
            }
        }

        public async Task<bool> SaveUncommitedEvents<T, TDataSnaphot>(T aggregate)
            where T : AggregateRoot<TDataSnaphot>
            where TDataSnaphot : class, new()
        {
            try
            {
                Expect.NotDisposed(isDisposed);
                Expect.NotNull(aggregate, nameof(aggregate));

                if (!aggregate.HasUncommitedEvents)
                {
                    Metric.Success(metrics, Metric.Names.SaveOp);
                    return true;
                }

                var commit = CreateCommit(
                    aggregate.StreamId,
                    aggregate.CurrentVersion,
                    aggregate.UncommitedEvents);

                EnsureOpenPersistenceSession();

                bool isSaved;
                using (Metric.MeasureSave(metrics))
                {
                    isSaved = await persistenceSession.Save(commit).ConfigureAwait(false);
                }

                if (isSaved)
                {
                    aggregate.MarkAsCommited();
                }

                Metric.Success(metrics, Metric.Names.SaveOp);

                return isSaved;
            }
            catch (VersionConflictException)
            {
                Metric.VersionConflict(metrics);
                throw;
            }
            catch
            {
                Metric.Error(metrics, Metric.Names.SaveOp);
                throw;
            }
        }

        public async Task<bool> DeleteStream<T, TDataSnaphot>(T aggregate)
            where T : AggregateRoot<TDataSnaphot>
            where TDataSnaphot : class, new()
        {
            try
            {
                Expect.NotDisposed(isDisposed);
                Expect.NotNull(aggregate, nameof(aggregate));

                EnsureOpenPersistenceSession();

                bool isDeleted;
                using (Metric.MeasureDelete(metrics))
                {
                    isDeleted = await persistenceSession.Delete(aggregate.StreamId).ConfigureAwait(false);
                }

                if (isDeleted)
                {
                    aggregate.MarkAsDeleted();
                }

                Metric.Success(metrics, Metric.Names.DeleteOp);

                return isDeleted;
            }
            catch
            {
                Metric.Error(metrics, Metric.Names.DeleteOp);
                throw;
            }
        }

        public async Task<bool> DeleteStream(string streamId)
        {
            try
            {
                Expect.NotDisposed(isDisposed);
                Expect.NotEmpty(streamId, nameof(streamId));

                EnsureOpenPersistenceSession();

                bool isDeleted;
                using (Metric.MeasureDelete(metrics))
                {
                    isDeleted = await persistenceSession.Delete(streamId).ConfigureAwait(false);
                }

                Metric.Success(metrics, Metric.Names.DeleteOp);

                return isDeleted;
            }
            catch
            {
                Metric.Error(metrics, Metric.Names.DeleteOp);
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
                Metric.OpenPersistenceSesssion(metrics);
                persistenceSession = persistence.OpenSession(SuppressAmbientTransaction);
            }
        }

        private Commit CreateCommit(string streamId, int version, IEnumerable<object> uncommitedEvents)
        {
            var commit = new Commit
            {
                Id = TimeGuid.NewId(),
                StreamId = streamId,
                Version = version,
            };
            using (Metric.MeasureSerialize(metrics))
            {
                serializersProvider.DefaultSerializer.Serialize(commit, uncommitedEvents);
            }

            Metric.CommitSize(metrics, commit.Payload.Length);

            return commit;
        }

        private object[] CreateEvents(Commit[] commits)
        {
            var all = new List<object>(commits.Length * 2);
            using (Metric.MeasureDeserialize(metrics))
            {
                foreach (var commit in commits)
                {
                    var events = serializersProvider.Get(commit.SerializerId).Deserialize(commit);
                    all.AddRange(events);
                    Metric.Serializer(metrics, commit.SerializerId);
                }
            }

            return all.ToArray();
        }
    }
}
