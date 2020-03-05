namespace EventR.Tests
{
    using EventR.Abstractions;
    using System;
    using Xunit;

    public class PayloadLayoutTests
    {
        [Fact]
        public void CtorThrowsOnInvalidArg()
        {
            Assert.ThrowsAny<ArgumentException>(() => new PayloadLayout(null));
            Assert.ThrowsAny<ArgumentException>(() => new PayloadLayout(Array.Empty<byte>()));
        }

        [Fact]
        public void SerializeAndDeserialize1()
        {
            var pl1 = new PayloadLayout();
            pl1.Add(0, 150, "My.Contracts.Type1, My.Contracts");
            pl1.Add(151, 678, "My.Contracts.Type2, My.Contracts");
            pl1.Add(679, 536, "My.Contracts.Type3, My.Contracts");
            var bytes = pl1.ToBytes();

            var pl2 = new PayloadLayout(bytes);

            Assert.True(IsSame(pl1, pl2));
        }

        [Fact]
        public void SerializeAndDeserialize2()
        {
            var pl1 = new PayloadLayout();
            pl1.Add(0, 150, "T1.EF48AE32");
            var bytes = pl1.ToBytes();

            var pl2 = new PayloadLayout(bytes);

            Assert.True(IsSame(pl1, pl2));
        }

        private bool IsSame(PayloadLayout pl1, PayloadLayout pl2)
        {
            var items1 = pl1.Items;
            var items2 = pl2.Items;
            if (items1.Length != items2.Length)
            {
                return false;
            }

            for (int i = 0; i < items1.Length; i++)
            {
                var (offset1, length1, typeId1) = items1[i];
                var (offset2, length2, typeId2) = items2[i];
                if (offset1 != offset2 || length1 != length2 || typeId1 != typeId2)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
