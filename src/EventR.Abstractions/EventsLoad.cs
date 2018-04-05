namespace EventR.Abstractions
{
    using System;

    public sealed class EventsLoad
    {
        public EventsLoad(object[] events, int version)
        {
            Events = events;
            Version = version;
        }

        public static readonly EventsLoad Empty = new EventsLoad(Array.Empty<object>(), 0);

        public object[] Events { get; }

        public int Version { get; }

        public bool IsEmpty => Events.Length == 0 || Version == 0;
    }
}
