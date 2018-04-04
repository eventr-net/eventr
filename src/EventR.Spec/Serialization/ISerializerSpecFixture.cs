namespace EventR.Spec.Serialization
{
    using System;
    using EventR.Abstractions;

    public interface ISerializerSpecFixture : IDisposable
    {
        ISerializeEvents Serializer { get; }

        object[] EventsFromDomain();

        string Description { get; }
    }
}
