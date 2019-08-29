namespace EventR.Abstractions.Exceptions
{
    using System;
    using System.Runtime.Serialization;

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
}
