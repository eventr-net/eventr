namespace EventR
{
    using EventR.Abstractions;
    using System;
    using System.Diagnostics.Tracing;
    using System.Text;

    public class EventREventSource : EventSource
    {
        public static readonly EventREventSource Log = new EventREventSource();

        [Event(
            EventIds.LoadEvents,
            Level = EventLevel.Informational,
            Message = "Loaded events on stream {0}, count: {1}, bytes: {2}, elapsed: {3} ms, correlationId: {4}",
            Keywords = Keywords.Session)]
        public void LoadEvents(string streamId, int eventCount, int bytes, long elapsedMs, string correlationId)
        {
            if (IsEnabled())
            {
                WriteEvent(EventIds.LoadEvents, streamId, eventCount, bytes, elapsedMs, correlationId);
            }
        }

        [Event(
            EventIds.LoadEventsError,
            Level = EventLevel.Error,
            Message = "Failed to load events on stream {0}, elapsed: {2} ms, correlationId: {3}, error: {4}",
            Keywords = Keywords.Session)]
        public void LoadEventsError(string streamId, long elapsedMs, string correlationId, string errorDetail)
        {
            if (IsEnabled())
            {
                WriteEvent(EventIds.LoadEventsError, streamId, elapsedMs, correlationId, errorDetail);
            }
        }

        public void LoadEventsError(Exception ex, string streamId, long elapsedMs, string correlationId)
        {
            if (IsEnabled())
            {
                LoadEventsError(streamId, elapsedMs, correlationId, Describe(ex));
            }
        }

        [Event(
            EventIds.EventStreamTooLongWarn,
            Level = EventLevel.Warning,
            Message = "Event stream {0} total payload is too big, bytes: {1} > limit: {2}, correlationId: {3}",
            Keywords = Keywords.Session)]
        public void EventStreamTooLongWarn(string streamId, int streamByteLength, int limit, string correlationId)
        {
            if (IsEnabled())
            {
                WriteEvent(EventIds.EventStreamTooLongWarn, streamId, streamByteLength, limit, correlationId);
            }
        }

        [Event(
            EventIds.EventStreamTooLongError,
            Level = EventLevel.Error,
            Message = "Event stream {0} total payload is too big, bytes: {1} > limit: {2}, correlationId: {3}",
            Keywords = Keywords.Session)]
        public void EventStreamTooLongError(string streamId, int streamByteLength, int limit, string correlationId)
        {
            if (IsEnabled())
            {
                WriteEvent(EventIds.EventStreamTooLongError, streamId, streamByteLength, limit, correlationId);
            }
        }

        [Event(
            EventIds.SaveUncommitedEvents,
            Level = EventLevel.Informational,
            Message = "Saved uncommited events on stream {0}, elapsed: {1} ms, correlationId: {2}",
            Keywords = Keywords.Session)]
        public void SaveUncommitedEvents(string streamId, long elapsedMs, string correlationId)
        {
            if (IsEnabled())
            {
                WriteEvent(EventIds.SaveUncommitedEvents, streamId, elapsedMs, correlationId);
            }
        }

        [Event(
            EventIds.SaveUncommitedEventsError,
            Level = EventLevel.Error,
            Message = "Failed to save uncommited events on stream {0}, elapsed: {2} ms, correlationId: {3}, error: {4}",
            Keywords = Keywords.Session)]
        public void SaveUncommitedEventsError(string streamId, long elapsedMs, string correlationId, string errorDetail)
        {
            if (IsEnabled())
            {
                WriteEvent(EventIds.SaveUncommitedEventsError, streamId, elapsedMs, correlationId, errorDetail);
            }
        }

        public void SaveUncommitedEventsError(Exception ex, string streamId, long elapsedMs, string correlationId)
        {
            if (IsEnabled())
            {
                SaveUncommitedEventsError(streamId, elapsedMs, correlationId, Describe(ex));
            }
        }

        [Event(
            EventIds.VersionConflict,
            Level = EventLevel.Warning,
            Message = "Saved uncommited events on stream {0}, elapsed: {1} ms, correlationId: {2}",
            Keywords = Keywords.Session)]
        public void VersionConflict(string streamId, int version, string correlationId)
        {
            if (IsEnabled())
            {
                WriteEvent(EventIds.VersionConflict, streamId, version, correlationId);
            }
        }

        [Event(
            EventIds.DeleteStream,
            Level = EventLevel.Informational,
            Message = "Deleted stream {0}, elapsed: {1} ms, correlationId: {2}",
            Keywords = Keywords.Session)]
        public void DeleteStream(string streamId, long elapsedMs, string correlationId)
        {
            if (IsEnabled())
            {
                WriteEvent(EventIds.DeleteStream, streamId, elapsedMs, correlationId);
            }
        }

        [Event(
            EventIds.DeleteStreamError,
            Level = EventLevel.Error,
            Message = "Failed to delete stream {0}, elapsed: {2} ms, correlationId: {3}, error: {4}",
            Keywords = Keywords.Session)]
        public void DeleteStreamError(string streamId, long elapsedMs, string correlationId, string errorDetail)
        {
            if (IsEnabled())
            {
                WriteEvent(EventIds.DeleteStreamError, streamId, elapsedMs, correlationId, errorDetail);
            }
        }

        public void DeleteStreamError(Exception ex, string streamId, long elapsedMs, string correlationId)
        {
            if (IsEnabled())
            {
                DeleteStreamError(streamId, elapsedMs, correlationId, Describe(ex));
            }
        }

        [Event(
            EventIds.SerializeCommit,
            Level = EventLevel.Verbose,
            Message = "Serialized uncommited events on stream {0}, events: {1}, elapsed: {2} ms, correlationId: {3}",
            Keywords = Keywords.Session)]
        public void SerializeCommit(string streamId, int eventCount, long elapsedMs, string correlationId)
        {
            if (IsEnabled())
            {
                WriteEvent(EventIds.SerializeCommit, streamId, eventCount, elapsedMs, correlationId);
            }
        }

        [Event(
            EventIds.DeserializeCommits,
            Level = EventLevel.Verbose,
            Message = "Deserialized commits on stream {0}, events: {1}, elapsed: {2} ms, correlationId: {3}",
            Keywords = Keywords.Session)]
        public void DeserializeCommits(string streamId, int eventCount, long elapsedMs, string correlationId)
        {
            if (IsEnabled())
            {
                WriteEvent(EventIds.DeserializeCommits, streamId, eventCount, elapsedMs, correlationId);
            }
        }

        private static string Describe(Exception ex)
        {
            var sb = new StringBuilder();
            var curr = ex;
            do
            {
                sb.AppendLine($"{curr.GetType().Name}: {curr.Message}");
                curr = ex.InnerException;
            }
            while (curr != null);

            return sb.ToString();
        }

        internal static class EventIds
        {
            public const int LoadEvents = 1;
            public const int LoadEventsError = 2;
            public const int EventStreamTooLongWarn = 3;
            public const int EventStreamTooLongError = 4;
            public const int SaveUncommitedEvents = 5;
            public const int SaveUncommitedEventsError = 6;
            public const int VersionConflict = 6;
            public const int DeleteStream = 7;
            public const int DeleteStreamError = 8;
            public const int SerializeCommit = 9;
            public const int DeserializeCommits = 10;
        }
    }
}
