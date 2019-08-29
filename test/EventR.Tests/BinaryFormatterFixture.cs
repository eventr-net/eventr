namespace EventR.Tests
{
    using EventR.Abstractions;
    using EventR.Binary;
    using EventR.Spec;
    using EventR.Spec.Serialization;

    public sealed class BinaryFormatterFixture : ISerializerSpecFixture
    {
        public BinaryFormatterFixture()
        {
            Serializer = new BinaryFormatterSerializer(Helper.EventFactory);

            var services = Helper.CreateAggregateRootServices(0);
            var rootAggregate = UseCases.Full().AsDirtyCustomerAggregate(services);
            EventsFromDomain = rootAggregate.UncommitedEvents;
        }

        public string Description { get; } = "BinaryFormatter serialization";

        public ISerializeEvents Serializer { get; }

        public object[] EventsFromDomain { get; }
    }
}
