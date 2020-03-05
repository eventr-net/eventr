namespace EventR.Abstractions.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class SerializationException : EventStoreException
    {
        public Commit Commit { get; }

        public SerializationException(string message, Commit commit)
            : base(message)
        {
            Commit = commit;
        }

        public SerializationException(string message, Exception ex, Commit commit)
            : base(message, ex)
        {
            Commit = commit;
        }

        protected SerializationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
