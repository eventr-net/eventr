namespace EventR.Abstractions
{
    using System.Collections.Generic;

    /// <summary>
    /// Serializer for the events.
    /// </summary>
    public interface ISerializeEvents
    {
        /// <summary>
        /// Serializes events into <see cref="Commit.Payload"/>;
        /// also sets <see cref="Commit.ItemsCount"/> and <see cref="Commit.SerializerId"/>.
        /// </summary>
        void Serialize(Commit commit, IEnumerable<object> events);

        /// <summary>
        /// Deserializes <see cref="Commit.Payload"/> into array of events.
        /// </summary>
        object[] Deserialize(Commit commit);

        /// <summary>
        /// Serializer ID; will be used to set <see cref="Commit.SerializerId"/>.
        /// </summary>
        string SerializerId { get; }
    }
}
