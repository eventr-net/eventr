namespace EventR.Spec.Persistence
{
    using EventR.Abstractions;
    using System;
    using System.Threading.Tasks;

    public interface IPersistenceSpecFixture : IDisposable
    {
        IPersistence Persistence { get; }

        Task<(bool ok, string errorDetail)> HasBeenSavedAsync(Commit expected);

        string Description { get; }
    }
}
