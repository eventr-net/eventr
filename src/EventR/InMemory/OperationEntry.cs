namespace EventR.InMemory
{
    using System;
    using EventR.Abstractions.Telemetry;

    public sealed class OperationEntry
    {
        public DateTimeOffset Time { get; }

        public Operation Operation { get; }

        public bool Success { get; }

        public TimeSpan Elapsed { get; }

        public string CorrelationId { get; }

        public OperationEntry(Operation operation, bool success, TimeSpan elapsed, string correlationId)
        {
            Time = DateTimeOffset.Now;
            Operation = operation;
            Success = success;
            Elapsed = elapsed;
            CorrelationId = correlationId;
        }
    }
}
