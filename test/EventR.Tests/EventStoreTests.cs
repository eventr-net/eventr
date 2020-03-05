namespace EventR.Tests
{
    using EventR.Abstractions;
    using Moq;
    using System;
    using Xunit;

    public class EventStoreTests
    {
        [Fact]
        public void CtorThrowsOnInvalidArg()
        {
            var p = new Mock<IPersistence>().Object;
            var s = new Mock<IProvideSerializers>().Object;

            Assert.ThrowsAny<ArgumentException>(() => new EventStore(null, s));
            Assert.ThrowsAny<ArgumentException>(() => new EventStore(p, null));
        }
    }
}
