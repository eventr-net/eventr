namespace EventR.Tests
{
    using EventR.Abstractions;
    using EventR.Spec.Domain;
    using System;

    internal static class Helper
    {
        internal static Type[] AllEventTypesFromDomain { get; }

        internal static IEventFactory EventFactory { get; }

        internal static IEventHandlerRegistry EventHandlerRegistry { get; }

        static Helper()
        {
            AllEventTypesFromDomain = Util.FindEventTypes(".Events", new[] { typeof(CustomerAggregate).Assembly });
            EventFactory = new EventFactory(AllEventTypesFromDomain);
            EventHandlerRegistry = new EventHandlerRegistry();
        }

        internal static IAggregateRootServices CreateAggregateRootServices(int errorOnStreamLength)
        {
            return new AggregateRootServices(EventFactory, EventHandlerRegistry, errorOnStreamLength);
        }
    }
}
