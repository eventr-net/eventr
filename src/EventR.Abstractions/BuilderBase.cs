namespace EventR.Abstractions
{
    public abstract class BuilderBase
    {
        protected BuilderBase(ConfigurationContext context)
        {
            Context = context;
        }

        public ConfigurationContext Context { get; }

        public (IEventStore, IAggregateRootServices) Build()
        {
            return Context.BuildMethod();
        }
    }
}
