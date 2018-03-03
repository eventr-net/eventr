namespace EventR.Spec
{
    using System;
    using EventR.Abstractions;

    public interface ISerializerSpecFixture : IDisposable
    {
        ISerializeEvents Serializer { get; }

        string Description { get; }
    }
}
