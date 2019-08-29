namespace EventR
{
    using EventR.Abstractions;
    using EventR.Abstractions.Telemetry;

    public sealed class EventStore : IEventStore
    {
        public EventStore(
            IPersistence persistence,
            IProvideSerializers serializersProvider,
            ITelemetry telemetry)
        {
            Expect.NotNull(persistence, nameof(persistence));
            Expect.NotNull(serializersProvider, nameof(serializersProvider));
            Expect.NotNull(telemetry, nameof(telemetry));

            Persistence = persistence;
            this.serializersProvider = serializersProvider;
            this.telemetry = telemetry;
        }

        private readonly IProvideSerializers serializersProvider;
        private readonly ITelemetry telemetry;
        private bool isDisposed;

        public IPersistence Persistence { get; }

        public int WarnOnStreamLength { get; set; }

        public int ErrorOnStreamLength { get; set; }

        public IEventStoreSession OpenSession(bool suppressAmbientTransaction = false)
        {
            return new EventStoreSession(Persistence, serializersProvider, telemetry, suppressAmbientTransaction)
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
