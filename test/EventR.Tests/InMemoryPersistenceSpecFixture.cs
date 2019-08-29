namespace EventR.Tests
{
    using EventR.Abstractions;
    using EventR.InMemory;
    using EventR.Spec.Persistence;

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

        public bool HasBeenSaved(Commit expected, out string errorDetail)
        {
            if (!persistence.Storage.TryGetValue(expected.StreamId, out var commits))
            {
                errorDetail = $"no commit stream '{expected.StreamId}' has been found";
                return false;
            }

            if (!commits.TryGetValue(expected.Version, out var found))
            {
                errorDetail = $"commit stream '{expected.StreamId}' does not contain version {expected.Version}";
                return false;
            }

            errorDetail = null;
            return true;
        }
    }
}
