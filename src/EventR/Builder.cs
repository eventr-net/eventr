namespace EventR
{
    using EventR.Abstractions;
    using EventR.Binary;
    using System;
    using System.Linq;
    using System.Reflection;
    using App.Metrics;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Fluent API configuration related to core features.
    /// </summary>
    public class Builder
    {
        protected Builder(ConfigurationContext context)
        {
            Context = context;
        }

        public static Builder SetUp => new Builder(new ConfigurationContext());

        public ConfigurationContext Context { get; }

        public Builder AssembliesToScan(params Assembly[] assemblies)
        {
            Expect.NotNull(assemblies, nameof(assemblies));
            foreach (var assembly in assemblies)
            {
                Context.AssembliesToScanForEvents.Add(assembly);
            }

            return this;
        }

        public Builder AssembliesToScan(Func<Assembly, bool> filterAppDomain)
        {
            Expect.NotNull(filterAppDomain, nameof(filterAppDomain));
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(filterAppDomain).ToArray();
            AssembliesToScan(assemblies);
            return this;
        }

        public Builder FindEventsUsing(Func<Type, bool> predicate)
        {
            Context.EventPredicate = predicate;
            return this;
        }

        public Builder InMemory()
        {
            Context.PersistenceFactory = () => new InMemory.InMemoryPersistence();
            return this;
        }

        public Builder BinaryFormatter()
        {
            Context.RegisterSerializer(ef => new BinaryFormatterSerializer(ef), BinaryFormatterSerializer.Id);
            return this;
        }

        public Builder StreamLengthLimits(int warnAt = 0, int errorAt = 0)
        {
            Context.WarnOnStreamLength = warnAt;
            Context.ErrorOnStreamLength = errorAt;
            return this;
        }

        public Builder Metrics(IMetrics metrics)
        {
            return Metrics(() => metrics);
        }

        public Builder Metrics(Func<IMetrics> metricsFactory)
        {
            Context.MetricsFactory = metricsFactory;
            return this;
        }

        public Builder Logging(ILoggerFactory loggerFactory)
        {
            return Logging(() => loggerFactory);
        }

        public Builder Logging(Func<ILoggerFactory> loggerFactory)
        {
            Context.LoggerFactory = loggerFactory;
            return this;
        }

        public void Build(out IEventStore eventStore, out IAggregateRootServices aggregateRootOpts)
        {
            var eventTypes = Util.FindEventTypes(Context.EventPredicate, Context.AssembliesToScanForEvents);
            IEventFactory eventFactory = new EventFactory(eventTypes);

            IPersistence peristence = Context.PersistenceFactory != null ? Context.PersistenceFactory() : null;

            var serializers = Context.SerializerFactories.Select(fn => fn(eventFactory)).ToArray();
            IProvideSerializers serializerProvider = new Serializers(serializers, Context.DefaultSerializerId);

            IMetrics metrics = Context.MetricsFactory != null ? Context.MetricsFactory() : null;

            ILoggerFactory loggers = Context.LoggerFactory != null ? Context.LoggerFactory() : null;

            eventStore = new EventStore(peristence, serializerProvider, metrics, loggers)
            {
                WarnOnStreamLength = Context.WarnOnStreamLength,
                ErrorOnStreamLength = Context.ErrorOnStreamLength,
            };
            aggregateRootOpts = new AggregateRootServices(eventFactory, new EventHandlerRegistry(), Context.ErrorOnStreamLength);
        }

        public IEventStore Build()
        {
            IEventStore store;
            IAggregateRootServices options;
            Build(out store, out options);

            AggregateRootServices.InitCurrent(options);

            return store;
        }
    }
}
