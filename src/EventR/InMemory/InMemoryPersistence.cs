namespace EventR.InMemory
{
    using System.Diagnostics.CodeAnalysis;
    using EventR.Abstractions;
    using Storage = System.Collections.Generic.Dictionary<string, System.Collections.Generic.SortedList<int, Abstractions.Commit>>;

    public sealed class InMemoryPersistence : IPersistence
    {
        private static readonly Storage Data = new Storage();

        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "not to reveal implementation detail (static variable) is intentional")]
        public Storage Storage => Data;

        public IPersistenceSession OpenSession(bool suppressAmbientTransaction = false)
        {
            return new InMemoryPersistenceSession(Data, suppressAmbientTransaction);
        }

        public void Dispose()
        {
        }
    }
}
