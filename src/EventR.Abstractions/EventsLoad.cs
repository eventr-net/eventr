namespace EventR.Abstractions
{
    public sealed class EventsLoad
    {
        public EventsLoad(object[] events, int version)
        {
            Events = events;
            Version = version;
        }

        public object[] Events { get; }

        public int Version { get; }
    }
}
