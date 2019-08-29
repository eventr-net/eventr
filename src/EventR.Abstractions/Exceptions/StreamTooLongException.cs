namespace EventR.Abstractions.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class StreamTooLongException : EventStoreException
    {
        public StreamTooLongException(string streamId, int version, int limit, string eventName)
            : base($"event stream {streamId} is too long ({version} >= limit {limit}); could not apply event {eventName}")
        {
            StreamId = streamId;
            Version = version;
        }

        protected StreamTooLongException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string StreamId { get; }

        public int Version { get; }
    }
}
