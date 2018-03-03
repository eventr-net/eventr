namespace EventR
{
    using App.Metrics;
    using EventR.Abstractions;
    using Microsoft.Extensions.Logging;

    public sealed class EventStore : IEventStore
    {
        public EventStore(
            IPersistence persistence,
            IProvideSerializers serializersProvider,
            IMetrics metrics,
            ILoggerFactory loggerFactory)
        {
            Expect.NotNull(persistence, "persistence");
            Expect.NotNull(serializersProvider, "serializersProvider");

            Persistence = persistence;
            this.serializersProvider = serializersProvider;
            this.metrics = metrics;
            this.loggerFactory = loggerFactory;
        }

        private readonly IProvideSerializers serializersProvider;
        private readonly IMetrics metrics;
        private readonly ILoggerFactory loggerFactory;
        private bool isDisposed;

        public IPersistence Persistence { get; }

        public int WarnOnStreamLength { get; set; }

        public int ErrorOnStreamLength { get; set; }

        public IEventStoreSession OpenSession(bool suppressAmbientTransaction = false)
        {
            var logger = loggerFactory != null ? loggerFactory.CreateLogger<EventStoreSession>() : VoidLogger.Current;
            return new EventStoreSession(Persistence, serializersProvider, logger, metrics, suppressAmbientTransaction)
            {
                WarnOnStreamLength = WarnOnStreamLength,
                ErrorOnStreamLength = ErrorOnStreamLength,
            };
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            Persistence.Dispose();
        }
    }
}
