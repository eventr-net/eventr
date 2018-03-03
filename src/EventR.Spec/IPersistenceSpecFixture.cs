namespace EventR.Spec
{
    using System;
    using EventR.Abstractions;

    public interface IPersistenceSpecFixture : IDisposable
    {
        IPersistence Persistence { get; }

        string Description { get; }
    }
}
