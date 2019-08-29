namespace EventR.Tests
{
    using EventR.Abstractions;
    using EventR.Spec.Domain;
    using Moq;
    using System;
    using Xunit;

    public class AggregateRootTests
    {
        [Fact]
        public void CtorThrowsOnInvalidArg()
        {
            var rootServices = new Mock<IAggregateRootServices>().Object;

            Assert.Throws<ArgumentNullException>(() => new CustomerAggregate(null, rootServices));
            Assert.ThrowsAny<ArgumentException>(() => new CustomerAggregate(string.Empty, rootServices));
            Assert.Throws<ArgumentNullException>(() => new CustomerAggregate(Guid.NewGuid().ToString(), null));
        }
    }
}
