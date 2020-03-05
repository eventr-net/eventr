namespace EventR.Tests
{
    using EventR.Abstractions;
    using EventR.InMemory;
    using EventR.Spec.Persistence;
    using System.Threading.Tasks;

    public sealed class InMemoryPersistenceSpecFixture : IPersistenceSpecFixture
    {
        private bool disposed;
        private readonly InMemoryPersistence persistence;

        public IPersistence Persistence => persistence;

        public string Description => "memory-only persistence";

        public InMemoryPersistenceSpecFixture()
        {
            persistence = new InMemoryPersistence();
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            Persistence.Dispose();
            disposed = true;
        }

        public Task<(bool ok, string errorDetail)> HasBeenSavedAsync(Commit expected)
        {
            if (!persistence.Storage.TryGetValue(expected.StreamId, out var commits))
            {
                return Task.FromResult((false, $"no commit stream '{expected.StreamId}' has been found"));
            }

            if (!commits.TryGetValue(expected.Version, out var _))
            {
                return Task.FromResult((false, $"commit stream '{expected.StreamId}' does not contain version {expected.Version}"));
            }

            return Task.FromResult<(bool, string)>((true, null));
        }
    }
}
