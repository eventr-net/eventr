namespace EventR.Abstractions
{
    using System;

    /// <summary>
    /// Taken from DataStax Cassandra .NET driver:
    /// https://github.com/datastax/csharp-driver/blob/d8c607563354e1b2eab65c825c00cae040b9bb96/src/Cassandra/TimeUuid.cs
    /// </summary>
    public struct TimeGuid : IEquatable<TimeGuid>, IComparable<TimeGuid>
    {
        private static readonly DateTimeOffset GregorianCalendarTime = new DateTimeOffset(1582, 10, 15, 0, 0, 0, TimeSpan.Zero);
        private static readonly Random RandomGenerator = new Random();
        private static readonly object RandomLock = new object();

        private readonly Guid value;

        private TimeGuid(Guid value)
        {
            this.value = value;
        }

        private TimeGuid(byte[] nodeId, byte[] clockId, DateTimeOffset time)
        {
            var timeBytes = BitConverter.GetBytes((time - GregorianCalendarTime).Ticks);
            var buffer = new byte[16];

            // positions 0-7 Timestamp
            Buffer.BlockCopy(timeBytes, 0, buffer, 0, 8);

            // position 8-9 Clock
            Buffer.BlockCopy(clockId, 0, buffer, 8, 2);

            // positions 10-15 Node
            Buffer.BlockCopy(nodeId, 0, buffer, 10, 6);

            // Version Byte: Time based
            // 0001xxxx
            // turn off first 4 bits
            buffer[7] &= 0x0f; // 00001111

            // turn on fifth bit
            buffer[7] |= 0x10; // 00010000

            // Variant Byte: 1.0.x
            // 10xxxxxx
            // turn off first 2 bits
            buffer[8] &= 0x3f; // 00111111

            // turn on first bit
            buffer[8] |= 0x80; // 10000000

            value = new Guid(buffer);
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a specified TimeUuid object represent the same value.
        /// </summary>
        public bool Equals(TimeGuid other)
        {
            return value.Equals(other.value);
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a specified TimeUuid object represent the same value.
        /// </summary>
        public override bool Equals(object obj)
        {
            var otherTimeUuid = obj as TimeGuid?;
            return otherTimeUuid != null && Equals(otherTimeUuid.Value);
        }

        /// <summary>
        /// Gets the DateTimeOffset representation of this instance
        /// </summary>
        public DateTimeOffset GetDate()
        {
            var bytes = value.ToByteArray();

            // remove version bit
            bytes[7] &= 0x0f; // 00001111

            // remove variant
            bytes[8] &= 0x3f; // 00111111

            var timestamp = BitConverter.ToInt64(bytes, 0);
            long ticks = timestamp + GregorianCalendarTime.Ticks;

            return new DateTimeOffset(ticks, TimeSpan.Zero);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        /// <summary>
        /// Returns a 16-element byte array that contains the value of this instance.
        /// </summary>
        public byte[] ToByteArray()
        {
            return value.ToByteArray();
        }

        /// <summary>
        /// Gets the Guid representation of the Id
        /// </summary>
        public Guid ToGuid()
        {
            return value;
        }

        /// <summary>
        /// Compares the current TimeUuid with another TimeUuid.
        /// </summary>
        public int CompareTo(TimeGuid other)
        {
            return value.CompareTo(other.value);
        }

        /// <summary>
        /// Returns a string representation of the value of this instance in registry format.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return value.ToString();
        }

        public string ToString(string format)
        {
            return value.ToString(format);
        }

        /// <summary>
        /// Returns a string representation
        /// </summary>
        public string ToString(string format, IFormatProvider provider)
        {
            return value.ToString(format, provider);
        }

        /// <summary>
        /// Initializes a new instance of the TimeUuid structure, using a random node id and clock sequence and the current date time
        /// </summary>
        public static TimeGuid NewId()
        {
            return NewId(DateTimeOffset.Now);
        }

        /// <summary>
        /// Initializes a new instance of the TimeUuid structure, using a random node id and clock sequence
        /// </summary>
        public static TimeGuid NewId(DateTimeOffset date)
        {
            byte[] nodeId;
            byte[] clockId;
            lock (RandomLock)
            {
                // oh yeah, thread safety
                nodeId = new byte[6];
                clockId = new byte[2];
                RandomGenerator.NextBytes(nodeId);
                RandomGenerator.NextBytes(clockId);
            }

            return new TimeGuid(nodeId, clockId, date);
        }

        /// <summary>
        /// From TimeUuid to Guid
        /// </summary>
        public static implicit operator Guid(TimeGuid value)
        {
            return value.ToGuid();
        }

        /// <summary>
        /// From Guid to TimeUuid
        /// </summary>
        public static implicit operator TimeGuid(Guid value)
        {
            return new TimeGuid(value);
        }

        public static bool operator ==(TimeGuid id1, TimeGuid id2)
        {
            return id1.ToGuid() == id2.ToGuid();
        }

        public static bool operator !=(TimeGuid id1, TimeGuid id2)
        {
            return id1.ToGuid() != id2.ToGuid();
        }

        public static bool operator >(TimeGuid id1, TimeGuid id2)
        {
            return id1.CompareTo(id2) > 0;
        }

        public static bool operator <(TimeGuid id1, TimeGuid id2)
        {
            return id1.CompareTo(id2) < 0;
        }

        public static bool operator >=(TimeGuid id1, TimeGuid id2)
        {
            return id1.CompareTo(id2) >= 0;
        }

        public static bool operator <=(TimeGuid id1, TimeGuid id2)
        {
            return id1.CompareTo(id2) <= 0;
        }
    }
}
