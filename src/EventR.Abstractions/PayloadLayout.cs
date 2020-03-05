namespace EventR.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public sealed class PayloadLayout
    {
        private const byte Delim = 0x10; // \n
        private const byte Sep = 0x3A; // :
        private const int IntSize = 4;
        private readonly List<(int offset, int length, string typeId)> data;

        public bool IsEmpty => data.Count == 0;

        public PayloadLayout(int capacity = 4)
        {
            data = new List<(int offset, int length, string typeId)>(capacity);
        }

        public PayloadLayout(byte[] serialized)
        {
            Expect.NotEmpty(serialized, nameof(serialized));

            data = new List<(int offset, int length, string typeId)>(4);
            int offset;
            int length;
            string typeId;
            var pos = 0;
            var start = 0;
            foreach (var b in serialized)
            {
                if (b == Delim)
                {
                    (offset, length, typeId) = ReadRow(serialized, start, pos);
                    Add(offset, length, typeId);
                    start = pos + 1;
                }

                ++pos;
            }

            (offset, length, typeId) = ReadRow(serialized, start, pos);
            Add(offset, length, typeId);
        }

        private static (int offset, int length, string typeId) ReadRow(byte[] bytes, int start, int end)
        {
            var idx = start;
            var offset = BitConverter.ToInt32(bytes, idx);
            idx += IntSize + 1;
            var length = BitConverter.ToInt32(bytes, idx);
            idx += IntSize + 1;
            var typeId = Encoding.ASCII.GetString(bytes, idx, end - idx);
            return (offset, length, typeId);
        }

        public void Add(int offset, int length, string typeId)
        {
            data.Add((offset, length, typeId));
        }

        public (int offset, int length, string typeId)[] Items
            => data.ToArray();

        public byte[] ToBytes()
        {
            if (data.Count == 0)
            {
                return Array.Empty<byte>();
            }

            using (var ms = new MemoryStream(data.Count * 100))
            {
                var first = true;
                foreach (var (offset, length, typeId) in data)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        ms.WriteByte(Delim);
                    }

                    ms.Write(BitConverter.GetBytes(offset), 0, IntSize);
                    ms.WriteByte(Sep);
                    ms.Write(BitConverter.GetBytes(length), 0, IntSize);
                    ms.WriteByte(Sep);
                    var typeIdBytes = Encoding.ASCII.GetBytes(typeId);
                    ms.Write(typeIdBytes, 0, typeIdBytes.Length);
                }

                return ms.ToArray();
            }
        }
    }
}
