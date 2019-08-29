namespace EventR.Abstractions.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class VersionConflictException : EventStoreException
    {
        public VersionConflictException(string streamId, int version)
            : base($"conflict on stream '{streamId}' occured; attempt to save duplicate version {version}")
        {
            StreamId = streamId;
            Version = version;
        }

        protected VersionConflictException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string StreamId { get; }

        public int Version { get; }

        public string Detail { get; set; }
    }
}
