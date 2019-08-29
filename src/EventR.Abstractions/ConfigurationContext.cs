namespace EventR.Abstractions
{
    using EventR.Abstractions.Telemetry;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public sealed class ConfigurationContext
    {
        public ConfigurationContext()
        {
            AssembliesToScanForEvents = new List<Assembly>();
            SerializerFactories = new List<Func<IEventFactory, ISerializeEvents>>();

            EventPredicate = t =>
                t.Namespace != null
                && (t.Namespace.EndsWith(".InternalEvents") || t.Namespace.EndsWith(".Events"));
        }

        private int warnOnStreamLength;

        private int errorOnStreamLength;

        private Func<IPersistence> persistenceFactory;

        private Func<Type, bool> eventPredicate;

        private Func<ITelemetry> telemetryFactory;

        private Func<(IEventStore, IAggregateRootServices)> buildMethod;

        public ICollection<Assembly> AssembliesToScanForEvents { get; }

        public Func<Type, bool> EventPredicate
        {
            get => eventPredicate;
            set
            {
                Expect.NotNull(value, nameof(value));
                eventPredicate = value;
            }
        }

        public ICollection<Func<IEventFactory, ISerializeEvents>> SerializerFactories { get; }

        public string DefaultSerializerId { get; private set; }

        public int WarnOnStreamLength
        {
            get => warnOnStreamLength;
            set
            {
                if (value > 0)
                {
                    warnOnStreamLength = value;
                }
            }
        }

        public int ErrorOnStreamLength
        {
            get => errorOnStreamLength;
            set
            {
                if (value > 0)
                {
                    errorOnStreamLength = value;
                }
            }
        }

        public Func<IPersistence> PersistenceFactory
        {
            get => persistenceFactory;
            set
            {
                Expect.NotNull(value, nameof(value));
                Expect.IsNotSet(persistenceFactory, nameof(PersistenceFactory));
                persistenceFactory = value;
            }
        }

        public Func<ITelemetry> TelemetryFactory
        {
            get => telemetryFactory;
            set
            {
                Expect.NotNull(value, nameof(value));
                Expect.IsNotSet(telemetryFactory, nameof(telemetryFactory));
                telemetryFactory = value;
            }
        }

        public Func<(IEventStore, IAggregateRootServices)> BuildMethod
        {
            get => buildMethod;
            set
            {
                Expect.NotNull(value, nameof(value));
                Expect.IsNotSet(buildMethod, nameof(buildMethod));
                buildMethod = value;
            }
        }

        public void RegisterSerializer(Func<IEventFactory, ISerializeEvents> factory, string id = null)
        {
            Expect.NotNull(factory, nameof(factory));

            SerializerFactories.Add(factory);
            if (!string.IsNullOrEmpty(id))
            {
                DefaultSerializerId = id;
            }
        }
    }
}
