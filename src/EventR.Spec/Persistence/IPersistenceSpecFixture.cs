namespace EventR.Spec.Persistence
{
    using System;
    using EventR.Abstractions;

    public interface IPersistenceSpecFixture : IDisposable
    {
        IPersistence Persistence { get; }

        bool HasBeenSaved(Commit expected, out string errorDetail);

        string Description { get; }
    }
}
