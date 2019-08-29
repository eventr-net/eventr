namespace EventR.Tests
{
    using EventR.Spec.Persistence;

    public class InMemoryPersistenceTests : PersistenceSpec<InMemoryPersistenceSpecFixture>
    {
        public InMemoryPersistenceTests(InMemoryPersistenceSpecFixture fixture)
            : base(fixture)
        {
        }
    }
}
