namespace EventR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using EventR.Abstractions;

    public class Serializers : IProvideSerializers
    {
        private readonly Dictionary<string, ISerializeEvents> registry = new Dictionary<string, ISerializeEvents>();
        private readonly string defaultSerializerId;

        public Serializers(ICollection<ISerializeEvents> serializers, string defaultSerializerId)
        {
            Expect.NotEmpty(serializers, "serializers");
            Expect.NotEmpty(defaultSerializerId, "defaultSerializerId");

            foreach (var serializer in serializers.Where(serializer => !registry.ContainsKey(serializer.SerializerId)))
            {
                registry.Add(serializer.SerializerId, serializer);
            }

            if (!registry.ContainsKey(defaultSerializerId))
            {
                var errMsg = $"No serializer of Id = '{defaultSerializerId}' is registrered.";
                throw new ArgumentException(errMsg, nameof(defaultSerializerId));
            }

            this.defaultSerializerId = defaultSerializerId;
        }

        public ISerializeEvents Get(string serializerId)
        {
            if (string.IsNullOrEmpty(serializerId))
            {
                return DefaultSerializer;
            }

            ISerializeEvents serializer;
            if (registry.TryGetValue(serializerId, out serializer))
            {
                return serializer;
            }

            throw new Exception($"No serializer of Id = '{serializerId}' is available.");
        }

        public ISerializeEvents DefaultSerializer => registry[defaultSerializerId];
    }
}
