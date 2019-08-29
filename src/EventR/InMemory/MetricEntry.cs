namespace EventR.InMemory
{
    using EventR.Abstractions.Telemetry;
    using System;

    public sealed class MetricEntry
    {
        public DateTimeOffset Time { get; }

        public Metric Metric { get; }

        public long Value { get; }

        public string CorrelationId { get; }

        public MetricEntry(Metric metric, long value, string correlationId)
        {
            Time = DateTimeOffset.Now;
            Metric = metric;
            Value = value;
            CorrelationId = correlationId;
        }
    }
}
