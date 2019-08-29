namespace EventR.InMemory
{
    using EventR.Abstractions.Telemetry;
    using System.Collections.Generic;
    using System.Text;

    public class MetricsReport
    {
        public Dictionary<Operation, (int success, int failure, double rate, double min, double max, double avg, double med, double p95)> Operations { get; }

        public Dictionary<Metric, (int count, double sum, double rate, double min, double max, double avg, double med, double p95)> Metrics { get; }

        public MetricsReport()
        {
            Operations = new Dictionary<Operation, (int success, int failure, double rate, double min, double max, double avg, double med, double p95)>(3);
            Metrics = new Dictionary<Metric, (int count, double sum, double rate, double min, double max, double avg, double med, double p95)>(7);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("OPERATIONS");
            foreach (var kvp in Operations)
            {
                sb.AppendLine();
                sb.AppendFormat("{0} -------------------------", kvp.Key.ToString());
                sb.AppendFormat("count: {0:N0}\r\n", kvp.Value.success + kvp.Value.failure);
                sb.AppendFormat("success: {0:N0}\r\n", kvp.Value.success);
                sb.AppendFormat("failure: {0:N0}\r\n", kvp.Value.failure);
                sb.AppendFormat("rate: {0:F2} rps\r\n", kvp.Value.rate);
                sb.AppendFormat("min: {0} ms\r\n", kvp.Value.min);
                sb.AppendFormat("max: {0} ms\r\n", kvp.Value.max);
                sb.AppendFormat("avg: {0:F2} ms\r\n", kvp.Value.avg);
                sb.AppendFormat("med: {0:F2} ms\r\n", kvp.Value.med);
                sb.AppendFormat("p95: {0:F2} ms\r\n", kvp.Value.p95);
                sb.AppendLine("-------------------------");
                sb.AppendLine();
            }

            sb.AppendLine("METRICS");
            foreach (var kvp in Metrics)
            {
                sb.AppendLine();
                sb.AppendFormat("{0} -------------------------", kvp.Key.ToString());
                sb.AppendFormat("count: {0:N0}\r\n", kvp.Value.count);
                sb.AppendFormat("sum: {0:F2}\r\n", kvp.Value.sum);
                sb.AppendFormat("rate: {0:F2}\r\n", kvp.Value.rate);
                sb.AppendFormat("min: {0}\r\n", kvp.Value.min);
                sb.AppendFormat("max: {0}\r\n", kvp.Value.max);
                sb.AppendFormat("avg: {0:F2}\r\n", kvp.Value.avg);
                sb.AppendFormat("med: {0:F2}\r\n", kvp.Value.med);
                sb.AppendFormat("p95: {0:F2}\r\n", kvp.Value.p95);
                sb.AppendLine("-------------------------");
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
