namespace EventR.Tests
{
    using EventR.Abstractions;
    using System;
    using Xunit;

    public class AggregateRootTests
    {
        [Fact]
        public void Test1()
        {
            var a = new object() as IEventStore;
            Assert.True(1 == 1);
        }
    }
}
