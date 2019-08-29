namespace EventR.Tests
{
    using EventR.Abstractions;
    using Moq;
    using System;
    using System.Collections.Generic;
    using Xunit;

    public class SerializersTests
    {
        [Fact]
        public void CtorThrowsOnInvalidArg()
        {
            var serializers = new Mock<ICollection<ISerializeEvents>>().Object;

            Assert.ThrowsAny<ArgumentException>(() => new Serializers(serializers, null));
            Assert.ThrowsAny<ArgumentException>(() => new Serializers(serializers, string.Empty));
            Assert.ThrowsAny<ArgumentException>(() => new Serializers(null, "some"));
        }
    }
}
