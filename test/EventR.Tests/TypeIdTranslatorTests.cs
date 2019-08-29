namespace EventR.Tests
{
    using System;
    using Xunit;

    public class TypeIdTranslatorTests
    {
        [Fact]
        public void CtorThrowsOnInvalidArg()
        {
            Assert.ThrowsAny<ArgumentException>(() => new TypeIdTranslator(null));
        }

        [Fact]
        public void GeneratesIdForKnownTypes()
        {
            var tt = new TypeIdTranslator(new[] { typeof(TestA), typeof(ITestB) });
            var actual = tt.Translate(typeof(TestA));
            Assert.Equal("TA.01993A36", actual);
        }

        [Fact]
        public void GeneratesIdForUnknownTypes()
        {
            var tt = new TypeIdTranslator(new[] { typeof(TestA)});
            var actual = tt.Translate(typeof(ITestB));
            Assert.Equal("ITB.2D74FBC0", actual);
        }

        public class TestA { }

        public interface ITestB { }
    }
}
