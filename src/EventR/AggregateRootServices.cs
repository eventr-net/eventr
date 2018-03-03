namespace EventR
{
    using EventR.Abstractions;
    using System;
    using System.Diagnostics;

    public sealed class AggregateRootServices : IAggregateRootServices
    {
        public AggregateRootServices(IEventFactory eventFactory, IEventHandlerRegistry eventHandlerRegistry, int errorOnStreamLength)
        {
            Expect.NotNull(eventFactory, nameof(eventFactory));
            Expect.NotNull(eventHandlerRegistry, nameof(eventHandlerRegistry));
            Expect.Range(errorOnStreamLength, 0, int.MaxValue, nameof(errorOnStreamLength));

            EventFactory = eventFactory;
            EventHandlerRegistry = eventHandlerRegistry;
            ErrorOnStreamLength = errorOnStreamLength;
        }

        public int ErrorOnStreamLength { get; }

        public IEventFactory EventFactory { get; }

        public IEventHandlerRegistry EventHandlerRegistry { get; }

        public static IAggregateRootServices Current
        {
            get
            {
                if (current == null)
                {
                    throw new InvalidOperationException("IAggregateRootOptions instance is not ready. " +
                        "Before using AggregateRootOptions.Current the AggregateRootOptions.InitCurrent must be called.");
                }

                return current;
            }
        }

        public static void InitCurrent(IAggregateRootServices instance)
        {
            Expect.NotNull(instance, nameof(instance));
            if (current != null)
            {
                Trace.TraceWarning("AggregateRootServices.Current is already set");
            }

            current = instance;
        }

        private static IAggregateRootServices current;
    }
}
