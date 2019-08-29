namespace EventR.Tests
{
    using EventR.Abstractions;
    using EventR.Abstractions.Telemetry;
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
            var t = new VoidTelemetry();

            Assert.ThrowsAny<ArgumentException>(() => new EventStore(null, s, t));
            Assert.ThrowsAny<ArgumentException>(() => new EventStore(p, null, t));
            Assert.ThrowsAny<ArgumentException>(() => new EventStore(p, s, null));
        }
    }
}
