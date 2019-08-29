namespace EventR.InMemory
{
    using System.Collections.Concurrent;

    public sealed class StreamTelemetry
    {
        public string StreamId { get; set; }

        public ConcurrentQueue<LogEntry> Logs { get; }

        public ConcurrentQueue<MetricEntry> Metrics { get; }

        public ConcurrentQueue<OperationEntry> Operations { get; }

        public StreamTelemetry(string streamId)
        {
            StreamId = streamId;
            Logs = new ConcurrentQueue<LogEntry>();
            Metrics = new ConcurrentQueue<MetricEntry>();
            Operations = new ConcurrentQueue<OperationEntry>();
        }
    }
}
