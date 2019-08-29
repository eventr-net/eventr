namespace EventR.InMemory
{
    using EventR.Abstractions.Telemetry;
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    public sealed partial class InMemoryTelemetry : ITelemetry
    {
        public ConcurrentDictionary<string, StreamTelemetry> Storage { get; private set; }

        public ConcurrentQueue<OperationEntry> FailedOperationsWithoutStreamId { get; private set; }

        public InMemoryTelemetry()
        {
            Storage = new ConcurrentDictionary<string, StreamTelemetry>();
            FailedOperationsWithoutStreamId = new ConcurrentQueue<OperationEntry>();
        }

        public void Log(LogSeverity severity, string message, string correlationId, string streamId)
        {
            var entry = new LogEntry(severity, message, correlationId);
            do
            {
                if (Storage.TryGetValue(streamId, out var streamTelemetry))
                {
                    streamTelemetry.Logs.Enqueue(entry);
                    return;
                }

                streamTelemetry = new StreamTelemetry(streamId);
                if (Storage.TryAdd(streamId, streamTelemetry))
                {
                    streamTelemetry.Logs.Enqueue(entry);
                    return;
                }
            }
            while (true);
        }

        public void Track(Metric metric, long value, string correlationId, string streamId)
        {
            var entry = new MetricEntry(metric, value, correlationId);
            do
            {
                if (Storage.TryGetValue(streamId, out var streamTelemetry))
                {
                    streamTelemetry.Metrics.Enqueue(entry);
                    return;
                }

                streamTelemetry = new StreamTelemetry(streamId);
                if (Storage.TryAdd(streamId, streamTelemetry))
                {
                    streamTelemetry.Metrics.Enqueue(entry);
                    return;
                }
            }
            while (true);
        }

        public void TrackFailure(Operation operation, TimeSpan elapsed, Exception exception, string correlationId, string streamId = null)
        {
            var entry = new OperationEntry(operation, false, elapsed, correlationId);
            if (streamId == null)
            {
                FailedOperationsWithoutStreamId.Enqueue(entry);
                return;
            }

            do
            {
                if (Storage.TryGetValue(streamId, out var streamTelemetry))
                {
                    streamTelemetry.Operations.Enqueue(entry);
                    return;
                }

                streamTelemetry = new StreamTelemetry(streamId);
                if (Storage.TryAdd(streamId, streamTelemetry))
                {
                    streamTelemetry.Operations.Enqueue(entry);
                    return;
                }
            }
            while (true);
        }

        public void TrackSuccess(Operation operation, TimeSpan elapsed, string correlationId, string streamId)
        {
            var entry = new OperationEntry(operation, true, elapsed, correlationId);
            do
            {
                if (Storage.TryGetValue(streamId, out var streamTelemetry))
                {
                    streamTelemetry.Operations.Enqueue(entry);
                    return;
                }

                streamTelemetry = new StreamTelemetry(streamId);
                if (Storage.TryAdd(streamId, streamTelemetry))
                {
                    streamTelemetry.Operations.Enqueue(entry);
                    return;
                }
            }
            while (true);
        }

        public void Reset()
        {
            Storage = new ConcurrentDictionary<string, StreamTelemetry>();
            FailedOperationsWithoutStreamId = new ConcurrentQueue<OperationEntry>();
        }

        public MetricsReport CreateMetricsReport()
        {
            var allOperations = Storage.Values
                .SelectMany(x => x.Operations.ToArray())
                .Union(FailedOperationsWithoutStreamId.ToArray())
                .OrderBy(x => x.Time)
                .ToArray();

            var allMetrics = Storage.Values
                .SelectMany(x => x.Metrics.ToArray())
                .OrderBy(x => x.Time)
                .ToArray();

            return new MetricsReport();
        }

        private static void CalculateOperation(Operation operation, OperationEntry[] operations, MetricsReport report)
        {
            var data = operations.Where(x => x.Operation == operation).ToArray();
            var cnt = data.Length;
            if (cnt == 0)
            {
                return;
            }

            var success = data.Count(x => x.Success);
            var failure = cnt - success;

            var lastEntry = data[cnt - 1];
            var minTime = data[0].Time;
            var maxTime = lastEntry.Time.Add(lastEntry.Elapsed);
            var deltaTimeMillis = (maxTime - minTime).TotalMilliseconds;
            var rate = 1000 * cnt / deltaTimeMillis;

            var orderedData = data.Select(x => x.Elapsed.TotalMilliseconds).OrderBy(x => x).ToArray();
            var min = orderedData[0];
            var max = orderedData[cnt - 1];
            var avg = Avg(orderedData);
            var med = Median(orderedData);
            var p95 = P95(orderedData);

            report.Operations.Add(operation, (success, failure, rate, min, max, avg, med, p95));
        }

        private static void CalculateMetric(Metric metric, MetricEntry[] metrics, MetricsReport report, bool isTime)
        {
            var data = metrics.Where(x => x.Metric == metric).ToArray();
            var cnt = data.Length;
            if (cnt == 0)
            {
                return;
            }

            var lastEntry = data[cnt - 1];
            var minTime = data[0].Time;
            var maxTime = isTime ? lastEntry.Time.AddMilliseconds(lastEntry.Value) : lastEntry.Time;
            var deltaTimeMillis = (maxTime - minTime).TotalMilliseconds;
            var rate = 1000 * cnt / deltaTimeMillis;

            var orderedData = data.Select(x => (double)x.Value).OrderBy(x => x).ToArray();
            var sum = orderedData.Sum();
            var min = orderedData[0];
            var max = orderedData[cnt - 1];
            var avg = Avg(orderedData);
            var med = Median(orderedData);
            var p95 = P95(orderedData);

            report.Metrics.Add(metric, (cnt, sum, rate, min, max, avg, med, p95));
        }

        private static double Avg(double[] values, int offset = 0, int count = 0)
        {
            var c = count > 0 ? count : values.Length;
            double sum = 0;
            for (int i = offset; i < (offset + c); i++)
            {
                sum += values[i];
            }

            return sum / c;
        }

        private static double Median(double[] descOrderedValues)
        {
            var idx = descOrderedValues.Length / 2;
            return descOrderedValues[idx];
        }

        private static double P95(double[] descOrderedValues)
        {
            var len = descOrderedValues.Length;
            if (len < 40)
            {
                return descOrderedValues[len - 1];
            }

            var count = (int)Math.Floor(len * 0.05);
            var offset = len - count;
            return Avg(descOrderedValues, offset, count);
        }
    }
}
