namespace EventR
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// MurMur2 32bit hash implementation.
    /// </summary>
    public sealed class MurmurHash2
    {
        private const uint M = 0x5bd1e995;
        private const int R = 24;

        [StructLayout(LayoutKind.Explicit)]
        private struct ByteToUintConverter
        {
            [FieldOffset(0)]
            public byte[] Bytes;

            [FieldOffset(0)]
            public readonly uint[] UInts;
        }

        public static uint Hash(byte[] data)
        {
            return Hash(data, 0xc58f1a7b);
        }

        public static uint Hash(byte[] data, uint seed)
        {
            if (data == null || data.Length == 0)
            {
                return 0;
            }

            int length = data.Length;
            uint h = seed ^ (uint)length;
            int index = 0;

            uint[] converter = new ByteToUintConverter { Bytes = data }.UInts;
            while (length >= 4)
            {
                uint k = converter[index++];
                k *= M;
                k ^= k >> R;
                k *= M;

                h *= M;
                h ^= k;
                length -= 4;
            }

            index *= 4;

            switch (length)
            {
                case 3:
                    h ^= (ushort)(data[index++] | data[index++] << 8);
                    h ^= (uint)data[index] << 16;
                    h *= M;
                    break;

                case 2:
                    h ^= (ushort)(data[index++] | data[index] << 8);
                    h *= M;
                    break;

                case 1:
                    h ^= data[index];
                    h *= M;
                    break;
            }

            h ^= h >> 13;
            h *= M;
            h ^= h >> 15;

            return h;
        }
    }
}
