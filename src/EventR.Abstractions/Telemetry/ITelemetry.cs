namespace EventR.Abstractions.Telemetry
{
    using System;

    public interface ITelemetry
    {
        void TrackSuccess(Operation operation, TimeSpan elapsed, string correlationId, string streamId);

        void TrackFailure(Operation operation, TimeSpan elapsed, Exception exception, string correlationId, string streamId = null);

        void Track(Metric metric, long value, string correlationId, string streamId);

        void Log(LogSeverity severity, string message, string correlationId, string streamId);
    }
}
