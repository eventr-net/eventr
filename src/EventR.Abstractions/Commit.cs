namespace EventR.Abstractions
{
    using EventR.Abstractions.Exceptions;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Represents data object that will be persisted.
    /// It is an intermediary between collection of events and their representation in persistence layer.
    /// </summary>
    public sealed class Commit
    {
        private const int MaxPayloadSize = 65536; // 64KB
        private const string StreamIdRxPattern = @"^[\w\-\/\.]{1,100}$";
        private const string SerializerIdRxPattern = @"^[\w\-\/\.]{1,20}$";
        private const RegexOptions RxOpts = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
        private static readonly Regex StreamIdRx = new Regex(StreamIdRxPattern, RxOpts);
        private static readonly Regex SerializerIdRx = new Regex(SerializerIdRxPattern, RxOpts);

        public TimeGuid Id { get; set; }

        /// <summary>
        /// Identifies a stream of events that belongs to a root aggregate.
        /// </summary>
        public string StreamId { get; set; }

        /// <summary>
        /// Sequential number of the commit.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Number of events serialized in the <see cref="Payload"/>.
        /// </summary>
        public short ItemsCount { get; set; }

        /// <summary>
        /// Serializer ID that has been used to serialize the <see cref="Payload"/>.
        /// </summary>
        public string SerializerId { get; set; }

        /// <summary>
        /// Single event or array of events serialized.
        /// </summary>
        public byte[] Payload { get; set; }

        /// <summary>
        ///
        /// </summary>
        public PayloadLayout PayloadLayout { get; set; }

        /// <summary>
        /// Checks properties of the instance and throws <see cref="InvalidPersistenceDataException"/> if it finds
        /// something invalid.
        /// </summary>
        public void ThrowIfContainsInvalidData()
        {
            if (Version < 1)
            {
                throw new InvalidPersistenceDataException($"Version is lesser than 1 in commit {StreamId}/{Version}");
            }

            if (ItemsCount < 1)
            {
                throw new InvalidPersistenceDataException($"ItemsCount is lesser than 1 in commit {StreamId}/{Version}");
            }

            if (StreamId == null || !StreamIdRx.IsMatch(StreamId))
            {
                throw new InvalidPersistenceDataException(
                    $"StreamId is not null or matching pattern '{StreamIdRxPattern}' in commit {StreamId}/{Version}");
            }

            if (SerializerId == null || !SerializerIdRx.IsMatch(SerializerId))
            {
                throw new InvalidPersistenceDataException(
                    $"SerializerId is not null or matching pattern '{SerializerIdRxPattern}' in commit {StreamId}/{Version}");
            }

            if (Payload == null || Payload.Length == 0)
            {
                throw new InvalidPersistenceDataException($"Payload is null or empty in commit {StreamId}/{Version}");
            }

            if (Payload.Length > MaxPayloadSize)
            {
                throw new InvalidPersistenceDataException(
                    $"Payload is too large ({Payload.Length} bytes; limit is {MaxPayloadSize}) in commit {StreamId}/{Version}");
            }

            if (Id == TimeGuid.Empty)
            {
                throw new InvalidPersistenceDataException($"Id must not be empty TimeGuid in commit {StreamId}/{Version}");
            }
        }

        public override string ToString()
        {
            return $"{StreamId}:{Version}";
        }
    }
}
