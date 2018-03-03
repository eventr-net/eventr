namespace EventR.Spec
{
    using Xunit;

    public abstract class SerializerSpec<T> : IClassFixture<T>
        where T : class, ISerializerSpecFixture, new()
    {
    }
}
