namespace EventR
{
    using EventR.Abstractions;

    public sealed class EventStore : IEventStore
    {
        public EventStore(IPersistence persistence, IProvideSerializers serializersProvider)
        {
            Expect.NotNull(persistence, nameof(persistence));
            Expect.NotNull(serializersProvider, nameof(serializersProvider));

            Persistence = persistence;
            this.serializersProvider = serializersProvider;
        }

        private readonly IProvideSerializers serializersProvider;
        private bool isDisposed;

        public IPersistence Persistence { get; }

        public int WarnOnStreamLength { get; set; }

        public int ErrorOnStreamLength { get; set; }

        public IEventStoreSession OpenSession(bool suppressAmbientTransaction = false)
        {
            return new EventStoreSession(Persistence, serializersProvider, suppressAmbientTransaction)
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
