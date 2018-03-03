namespace EventR.Spec
{
    using EventR.Abstractions;
    using Xunit;

    public abstract class PersistenceSpec<T> : IClassFixture<T>
        where T : class, IPersistenceSpecFixture, new()
    {
    }
}
