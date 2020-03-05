namespace EventR
{
    using EventR.Abstractions;
    using EventR.Binary;
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Fluent API configuration related to core features.
    /// </summary>
    public sealed class Builder : BuilderBase
    {
        public Builder()
            : base(new ConfigurationContext())
        {
            Context.BuildMethod = BuildImpl;
        }

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

        private (IEventStore, IAggregateRootServices) BuildImpl()
        {
            var eventTypes = Util.FindEventTypes(Context.EventPredicate, Context.AssembliesToScanForEvents);
            IEventFactory eventFactory = new EventFactory(eventTypes);

            IPersistence peristence = Context.PersistenceFactory != null ? Context.PersistenceFactory() : null;

            var serializers = Context.SerializerFactories.Select(fn => fn(eventFactory)).ToArray();
            IProvideSerializers serializerProvider = new Serializers(serializers, Context.DefaultSerializerId);

            var eventStore = new EventStore(peristence, serializerProvider)
            {
                WarnOnStreamLength = Context.WarnOnStreamLength,
                ErrorOnStreamLength = Context.ErrorOnStreamLength,
            };
            var aggregateRootOpts = new AggregateRootServices(eventFactory, new EventHandlerRegistry(), Context.ErrorOnStreamLength);

            return (eventStore, aggregateRootOpts);
        }
    }
}
