namespace EventR.Abstractions
{
    using EventR.Abstractions.Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Parent for all root aggregate implementations which would like to participate in event sourcing.
    /// NOT thread-safe.
    /// </summary>
    /// <typeparam name="T">type of data snapshot</typeparam>
    public abstract class AggregateRoot<T>
        where T : class, new()
    {
        protected AggregateRoot(string streamId, IAggregateRootServices services)
        {
            Expect.NotEmpty(streamId, nameof(streamId));
            Expect.NotNull(services, nameof(services));

            StreamId = streamId;
            Data = new T();
            uncommitedEvents = new List<object>();
            this.services = services;
        }

        private readonly List<object> uncommitedEvents;
        private readonly IAggregateRootServices services;
        private readonly object dataWriteLock = new object();

        public string StreamId { get; }

        /// <summary>
        /// Sequential number of the events applied on this root aggregate.
        /// Useful for resolving concurrent updates collisions.
        /// </summary>
        public int CurrentVersion { get; private set; }

        protected T Data { get; private set; }

        internal int ErrorOnStreamLength => services.ErrorOnStreamLength;

        /// <summary>
        /// Every new event must be applied on the object through this method.
        /// </summary>
        public void Apply<TEvent>(Action<TEvent> eventSetUpExpr)
            where TEvent : class
        {
            if (services.ErrorOnStreamLength > 0 && CurrentVersion >= services.ErrorOnStreamLength)
            {
                throw new StreamTooLongException(StreamId, CurrentVersion, services.ErrorOnStreamLength, typeof(TEvent).Name);
            }

            var @event = services.EventFactory.Create(eventSetUpExpr);
            var handle = services.EventHandlerRegistry.GetHandler(this, @event.GetType());
            lock (dataWriteLock)
            {
                handle(@event);
                ++CurrentVersion;
                uncommitedEvents.Add(@event);
            }
        }

        /// <summary>
        /// Collection of events that have yet not been persisted.
        /// </summary>
        public object[] UncommitedEvents => uncommitedEvents.ToArray();

        public bool HasUncommitedEvents => uncommitedEvents.Any();

        internal void MarkAsCommited()
        {
            lock (dataWriteLock)
            {
                uncommitedEvents.Clear();
            }
        }

        internal void MarkAsDeleted()
        {
            lock (dataWriteLock)
            {
                uncommitedEvents.Clear();
                CurrentVersion = 0;
            }
        }

        /// <summary>
        /// Entry method for re-hydration; that is re-playing persisted events on new root aggregate instance.
        /// </summary>
        internal void Hydrate(EventsLoad load)
        {
            if (HasUncommitedEvents)
            {
                throw new InvalidOperationException("cannot hydrate aggregate which contains uncommited events");
            }

            lock (dataWriteLock)
            {
                foreach (var e in load.Events)
                {
                    var handle = services.EventHandlerRegistry.GetHandler(this, e.GetType());
                    handle(e);
                }

                CurrentVersion = load.Version;
            }
        }
    }
}
