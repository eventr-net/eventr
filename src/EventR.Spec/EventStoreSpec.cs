namespace EventR.Spec
{
    using EventR.Abstractions;
    using Xunit;

    public abstract class EventStoreSpec<T> : IClassFixture<T>
        where T : class, IEventStoreSpecFixture, new()
    {
        protected EventStoreSpec(IEventStoreSpecFixture fixture)
        {
            Expect.NotNull(fixture, nameof(fixture));
            Fixture = fixture;
        }

        protected IEventStoreSpecFixture Fixture { get; }
    }
}
