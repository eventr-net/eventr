namespace EventR.InMemory
{
    using EventR.Abstractions.Telemetry;
    using System;

    public sealed class LogEntry
    {
        public DateTimeOffset Time { get; }

        public LogSeverity Severity { get; }

        public string Message { get; }

        public string CorrelationId { get; }

        public LogEntry(LogSeverity severity, string message, string correlationId)
        {
            Time = DateTimeOffset.Now;
            Severity = severity;
            Message = message;
            CorrelationId = correlationId;
        }
    }
}
