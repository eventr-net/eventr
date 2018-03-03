namespace EventR.Abstractions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402", Justification = "simplest 'close' classes and rarely touched")]
    [Serializable]
    public class EventStoreException : Exception
    {
        public EventStoreException()
        {
        }

        public EventStoreException(string message)
            : base(message)
        {
        }

        public EventStoreException(string message, Exception ex)
            : base(message, ex)
        {
        }

        protected EventStoreException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402", Justification = "simplest 'close' classes and rarely touched")]
    [Serializable]
    public class InvalidPersistenceDataException : EventStoreException
    {
        public InvalidPersistenceDataException()
        {
        }

        public InvalidPersistenceDataException(string message)
            : base(message)
        {
        }

        public InvalidPersistenceDataException(string message, Exception ex)
            : base(message, ex)
        {
        }

        protected InvalidPersistenceDataException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402", Justification = "simplest 'close' classes and rarely touched")]
    [SuppressMessage("Microsoft.Design", "CA1032", Justification = "values for properties are mandatory")]
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

    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402", Justification = "simplest 'close' classes and rarely touched")]
    [SuppressMessage("Microsoft.Design", "CA1032", Justification = "values for properties are mandatory")]
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
