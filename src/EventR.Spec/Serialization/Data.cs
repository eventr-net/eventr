namespace EventR.Spec.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

#pragma warning disable SA1402 // File may only contain a single type
    public static class Data
    {
        public static IEnumerable<object[]> Tests()
        {
            var data = new object[]
            {
                new ByteCase(0xFF),
                new NullableByteCase(0x20),
                new IntCase(int.MinValue),
                new IntCase(1485),
                new IntCase(int.MaxValue),
                new NullableIntCase(-89567),
                new LongCase(long.MinValue),
                new LongCase(-95648596L),
                new LongCase(long.MaxValue),
                new NullableLongCase(4595L),
                new DoubleCase(double.MinValue),
                new DoubleCase(12.99d),
                new DoubleCase(double.MaxValue),
                new NullableDoubleCase(-199.43d),
                new DecimalCase(decimal.MinValue),
                new DecimalCase(39.99m),
                new DecimalCase(decimal.MaxValue),
                new NullableDecimalCase(0.77m),
                new BoolCase(true),
                new BoolCase(false),
                new NullableBoolCase(true),
                new DateTimeCase(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)),
                new DateTimeCase(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)),
                new DateTimeCase(new DateTime(2150, 12, 31, 23, 59, 59, 999, DateTimeKind.Utc)),
                new NullableDateTimeCase(new DateTime(2015, 7, 29, 8, 51, 17, 465, DateTimeKind.Utc)),
                new GuidCase(Guid.Empty),
                new GuidCase(Guid.Parse("f46f09e4-22fc-4f6b-9ced-1deb1a6e69bb")),
                new NullableGuidCase(Guid.Parse("ee1ae43b-09e1-44e5-9427-df9eb6555146")),
                new TimeSpanCase(TimeSpan.MinValue),
                new TimeSpanCase(new TimeSpan(30, 0, 0, 0)),
                new TimeSpanCase(TimeSpan.MaxValue),
                new NullableTimeSpanCase(new TimeSpan(7, 12, 30, 10)),
                new StringCase(string.Empty),
                new StringCase("lorem ipsum dolor sit"),
                new StringCase(">} {<"),
                new ArrayOfStringsCase(new[] { "alpha", "beta", "gama" }),
                new ArrayOfIntsCase(new[] { 1, 2, 3 }),
                new ArrayOfBytesCase(Encoding.UTF8.GetBytes("lorem ipsum")),
            };

            return data.Select(x => new[] { x });
        }

        public static TestCase TestCase()
        {
            return new TestCase
            {
                TheString = "Lorem ipsum",
                TheInt = 15,
                NullableInt = -50,
                TheUInt = uint.MaxValue,
                TheBoolean = true,
                TheGuid = Guid.NewGuid(),
                NullableGuid = Guid.NewGuid(),
                DateTime = DateTime.UtcNow,
                NullableDateTime = DateTime.UtcNow.AddDays(-7),
                ArrayOfStrings = new[] { "alpha", "beta", "gama" },
                ArrayOfInt = new[] { int.MinValue, 0, int.MaxValue },
                SubCase = new TestSubCase { TheInt = 96584, TheString = "Dolor sit" },
                ArrayOfSubCases = new[]
                {
                    new TestSubCase
                    {
                        TheInt = 156,
                        TheString = "green frog",
                    },
                    new TestSubCase
                    {
                        TheInt = 96854,
                        TheString = @"future<salary>",
                    },
                },
            };
        }
    }

    [Serializable]
    public abstract class SerializationCase<T>
    {
        protected SerializationCase(T value)
        {
            Value = value;
            TypeName = typeof(T).Name;
        }

        public T Value { get; set; }

        public string TypeName { get; set; }
    }

    [Serializable]
    public class ByteCase : SerializationCase<byte>
    {
        private ByteCase()
            : base(0)
        { }

        public ByteCase(byte value)
            : base(value)
        { }
    }

    [Serializable]
    public class NullableByteCase : SerializationCase<byte?>
    {
        private NullableByteCase()
            : base(null)
        { }

        public NullableByteCase(byte? value)
            : base(value)
        { }
    }

    [Serializable]
    public class IntCase : SerializationCase<int>
    {
        private IntCase()
            : base(0)
        { }

        public IntCase(int value)
            : base(value)
        { }
    }

    [Serializable]
    public class NullableIntCase : SerializationCase<int?>
    {
        private NullableIntCase()
            : base(null)
        { }

        public NullableIntCase(int? value)
            : base(value)
        { }
    }

    [Serializable]
    public class LongCase : SerializationCase<long>
    {
        private LongCase()
            : base(0L)
        { }

        public LongCase(long value)
            : base(value)
        { }
    }

    [Serializable]
    public class NullableLongCase : SerializationCase<long?>
    {
        private NullableLongCase()
            : base(null)
        { }

        public NullableLongCase(long? value)
            : base(value)
        { }
    }

    [Serializable]
    public class DoubleCase : SerializationCase<double>
    {
        private DoubleCase()
            : base(0d)
        { }

        public DoubleCase(double value)
            : base(value)
        { }
    }

    [Serializable]
    public class NullableDoubleCase : SerializationCase<double?>
    {
        private NullableDoubleCase()
            : base(null)
        { }

        public NullableDoubleCase(double? value)
            : base(value)
        { }
    }

    [Serializable]
    public class DecimalCase : SerializationCase<decimal>
    {
        private DecimalCase()
            : base(0m)
        { }

        public DecimalCase(decimal value)
            : base(value)
        { }
    }

    [Serializable]
    public class NullableDecimalCase : SerializationCase<decimal?>
    {
        private NullableDecimalCase()
            : base(null)
        { }

        public NullableDecimalCase(decimal? value)
            : base(value)
        { }
    }

    [Serializable]
    public class BoolCase : SerializationCase<bool>
    {
        private BoolCase()
            : base(false)
        { }

        public BoolCase(bool value)
            : base(value)
        { }
    }

    [Serializable]
    public class NullableBoolCase : SerializationCase<bool?>
    {
        private NullableBoolCase()
            : base(null)
        { }

        public NullableBoolCase(bool? value)
            : base(value)
        { }
    }

    [Serializable]
    public class GuidCase : SerializationCase<Guid>
    {
        private GuidCase()
            : base(Guid.Empty)
        { }

        public GuidCase(Guid value)
            : base(value)
        { }
    }

    [Serializable]
    public class NullableGuidCase : SerializationCase<Guid?>
    {
        private NullableGuidCase()
            : base(null)
        { }

        public NullableGuidCase(Guid? value)
            : base(value)
        { }
    }

    [Serializable]
    public class TimeSpanCase : SerializationCase<TimeSpan>
    {
        private TimeSpanCase()
            : base(TimeSpan.Zero)
        { }

        public TimeSpanCase(TimeSpan value)
            : base(value)
        { }
    }

    [Serializable]
    public class NullableTimeSpanCase : SerializationCase<TimeSpan?>
    {
        private NullableTimeSpanCase()
            : base(null)
        { }

        public NullableTimeSpanCase(TimeSpan? value)
            : base(value)
        { }
    }

    [Serializable]
    public class DateTimeCase : SerializationCase<DateTime>
    {
        private DateTimeCase()
            : base(DateTime.MinValue)
        { }

        public DateTimeCase(DateTime value)
            : base(value)
        { }
    }

    [Serializable]
    public class NullableDateTimeCase : SerializationCase<DateTime?>
    {
        private NullableDateTimeCase()
            : base(null)
        { }

        public NullableDateTimeCase(DateTime? value)
            : base(value)
        { }
    }

    [Serializable]
    public class StringCase : SerializationCase<string>
    {
        private StringCase()
            : base(null)
        { }

        public StringCase(string value)
            : base(value)
        { }
    }

    [Serializable]
    public class ArrayOfStringsCase : SerializationCase<string[]>
    {
        private ArrayOfStringsCase()
            : base(null)
        { }

        public ArrayOfStringsCase(string[] value)
            : base(value)
        { }
    }

    [Serializable]
    public class ArrayOfIntsCase : SerializationCase<int[]>
    {
        private ArrayOfIntsCase()
            : base(null)
        { }

        public ArrayOfIntsCase(int[] value)
            : base(value)
        { }
    }

    [Serializable]
    public class ArrayOfBytesCase : SerializationCase<byte[]>
    {
        private ArrayOfBytesCase()
            : base(null)
        { }

        public ArrayOfBytesCase(byte[] value)
            : base(value)
        { }
    }

    #pragma warning restore
}
