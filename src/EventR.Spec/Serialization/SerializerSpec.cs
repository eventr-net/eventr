namespace EventR.Spec.Serialization
{
    using System;
    using System.Linq;
    using EventR.Abstractions;
    using KellermanSoftware.CompareNetObjects;
    using Xunit;

    public abstract class SerializerSpec<T> : IClassFixture<T>
        where T : class, ISerializerSpecFixture, new()
    {
        protected SerializerSpec(T fixture)
        {
            Expect.NotNull(fixture, nameof(fixture));
            Fixture = fixture;
            Comparer = new CompareLogic();
        }

        protected T Fixture { get; }

        protected CompareLogic Comparer { get; }

        [Fact]
        public void SerializeShouldThrowOnNullArgs()
        {
            var sut = Fixture.Serializer;
            Assert.Throws<ArgumentNullException>(() => sut.Serialize(null, null));
        }

        [Fact]
        public void SerializeShouldThrowOnEventsNullArg()
        {
            var sut = Fixture.Serializer;
            Assert.Throws<ArgumentNullException>(() => sut.Serialize(new Commit(), null));
        }

        [Fact]
        public void SerializeShouldThrowOnCommitNullArg()
        {
            var sut = Fixture.Serializer;
            Assert.Throws<ArgumentNullException>(() => sut.Serialize(null, new[] { Data.TestCase() }));
        }

        [Fact]
        public void DeserializeShouldThrowOnCommitNullArg()
        {
            var sut = Fixture.Serializer;
            Assert.Throws<ArgumentNullException>(() => sut.Deserialize(null));
        }

        [Fact]
        public void DeserializeShouldThrowOnEmptyEventsArg()
        {
            var sut = Fixture.Serializer;
            Assert.Throws<ArgumentException>(() => sut.Serialize(new Commit(), Array.Empty<object>()));
        }

        [Theory]
        [MemberData(nameof(Data.Tests), MemberType = typeof(Data))]
        public void ShouldSerializeCommonCases(object expected)
        {
            var sut = Fixture.Serializer;
            var commit = new Commit();

            sut.Serialize(commit, new[] { expected });
            var events = sut.Deserialize(commit);
            var actual = events[0];

            var result = Comparer.Compare(expected, actual);
            Assert.True(result.AreEqual, CreateMessage(result.DifferencesString));
        }

        [Fact]
        public void ShouldSerializeSingleReferentialObject()
        {
            var sut = Fixture.Serializer;
            var expected = Data.TestCase();
            var commit = new Commit();

            sut.Serialize(commit, new[] { expected });

            var events = sut.Deserialize(commit);
            Assert.True(events != null && events.Length == 1, CreateMessage("1 event in response"));

            var actual = (TestCase)events[0];
            var result = Comparer.Compare(expected, actual);
            Assert.True(result.AreEqual, CreateMessage(result.DifferencesString));
        }

        [Fact]
        public void ShouldSerializeTwoReferentialObjects()
        {
            var sut = Fixture.Serializer;
            var expected = Data.TestCase();
            var commit = new Commit();

            sut.Serialize(commit, new[] { expected, expected });

            var events = sut.Deserialize(commit);
            Assert.True(events != null && events.Length == 2, CreateMessage("2 events in response"));

            foreach (TestCase actual in events)
            {
                var result = Comparer.Compare(expected, actual);
                Assert.True(result.AreEqual, CreateMessage(result.DifferencesString));
            }
        }

        [Fact]
        public void ShouldSerializeInterfaceThroughSurrogate()
        {
            var sut = Fixture.Serializer;
            var commit = new Commit();
            var expected = Fixture.EventsFromDomain.First();

            sut.Serialize(commit, new[] { expected });
            var events = sut.Deserialize(commit);
            var actual = events[0];

            var result = Comparer.Compare(expected, actual);
            Assert.True(result.AreEqual, CreateMessage(result.DifferencesString));
        }

        [Fact]
        public void ShouldSerializeMultipleInterfacesThroughSurrogates()
        {
            var sut = Fixture.Serializer;
            var commit = new Commit();
            var events = Fixture.EventsFromDomain;

            sut.Serialize(commit, events);
            var eventsDeserialized = sut.Deserialize(commit);

            Assert.True(
                eventsDeserialized != null && eventsDeserialized.Length == events.Length,
                CreateMessage("number of deserialized events does not match original ones"));

            // ReSharper disable once PossibleNullReferenceException
            for (var i = 0; i < eventsDeserialized.Length; i++)
            {
                var expected = events[i];
                var actual = eventsDeserialized[i];
                var result = Comparer.Compare(expected, actual);
                Assert.True(result.AreEqual, CreateMessage(result.DifferencesString));
            }
        }

        protected virtual string CreateMessage(string originalMessage, params object[] args)
        {
            var append = $" ; using fixture {typeof(T).Name} configured as {Fixture.Description}";
            return args != null && args.Length > 0
                ? string.Format(originalMessage, args) + append
                : originalMessage + append;
        }
    }
}
