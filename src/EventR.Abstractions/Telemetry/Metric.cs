namespace EventR.Abstractions.Telemetry
{
    public enum Metric
    {
        CommitsPerLoad,
        BytesPerLoad,
        VersionConflict,
        StreamTooLong,
        DeserializeTime,
        SerializeTime,
        EmptyStream,
    }
}
