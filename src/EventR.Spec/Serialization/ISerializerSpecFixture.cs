namespace EventR.Spec.Serialization
{
    using EventR.Abstractions;

    public interface ISerializerSpecFixture
    {
        ISerializeEvents Serializer { get; }

        object[] EventsFromDomain { get; }

        string Description { get; }
    }
}
