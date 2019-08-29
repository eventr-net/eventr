namespace EventR.Tests
{
    using EventR.Spec.Serialization;

    public class BinaryFormatterTests : SerializerSpec<BinaryFormatterFixture>
    {
        public BinaryFormatterTests(BinaryFormatterFixture fixture)
            : base(fixture)
        {
            this.fixture = fixture;
        }

        private readonly BinaryFormatterFixture fixture;
    }
}
