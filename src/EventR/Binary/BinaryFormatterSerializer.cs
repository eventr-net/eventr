namespace EventR.Binary
{
    using EventR.Abstractions;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;

    public class BinaryFormatterSerializer : ISerializeEvents
    {
        public const string Id = "bin";
        private readonly BinaryFormatter serializer;

        public BinaryFormatterSerializer(IEventFactory eventFactory)
        {
            Expect.NotNull(eventFactory, "eventFactory");
            serializer = new BinaryFormatter
            {
                Binder = new BinaryFormatterSerializationBinder(eventFactory),
            };
        }

        public void Serialize(Commit commit, IEnumerable<object> events)
        {
            Expect.NotNull(commit, "commit");
            Expect.NotNull(events, "events");

            var items = events.ToArray();
            Expect.NotEmpty(items, "items");

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, items);
                commit.Payload = stream.ToArray();
            }

            commit.ItemsCount = (short)items.Length;
            commit.SerializerId = SerializerId;
        }

        public object[] Deserialize(Commit commit)
        {
            Expect.NotNull(commit, "commit");

            using (var stream = new MemoryStream(commit.Payload))
            {
                var deserialized = serializer.Deserialize(stream);
                return (object[])deserialized;
            }
        }

        public string SerializerId => Id;
    }
}
