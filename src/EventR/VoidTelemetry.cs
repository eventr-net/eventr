namespace EventR
{
    using System;
    using EventR.Abstractions.Telemetry;

    public sealed class VoidTelemetry : ITelemetry
    {
        public void Log(LogSeverity severity, string message, string correlationId, string streamId)
        {
        }

        public void Track(Metric metric, long value, string correlationId, string streamId)
        {
        }

        public void TrackFailure(Operation operation, TimeSpan elapsed, Exception exception, string correlationId, string streamId = null)
        {
        }

        public void TrackSuccess(Operation operation, TimeSpan elapsed, string correlationId, string streamId)
        {
        }
    }
}
