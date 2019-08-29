namespace EventR.Abstractions.Exceptions
{
    using System;
    using System.Runtime.Serialization;

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
}
