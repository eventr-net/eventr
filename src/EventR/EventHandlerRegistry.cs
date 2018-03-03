namespace EventR
{
    using EventR.Abstractions;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    // key: Tuple<Type, Type> -> first Type is aggregate and second Type is event
    // value: Action<object, object> -> handler method call
    using RegistryCache = System.Collections.Concurrent.ConcurrentDictionary<System.Tuple<System.Type, System.Type>, System.Action<object, object>>;

    public class EventHandlerRegistry : IEventHandlerRegistry
    {
        private static readonly RegistryCache Registry = new RegistryCache();

        public Action<object> GetHandler(object aggregate, Type eventType)
        {
            Expect.NotNull(aggregate, "aggregate");
            Expect.NotNull(eventType, "eventType");

            var aggregateType = aggregate.GetType();
            var key = new Tuple<Type, Type>(aggregateType, eventType);
            Action<object, object> handler;
            if (Registry.TryGetValue(key, out handler))
            {
                return e => handler(aggregate, e);
            }

            handler = FindHandlerOrCreateVoidImpl(aggregateType, eventType);
            Registry.TryAdd(key, handler);

            return e => handler(aggregate, e);
        }

        private static Action<object, object> FindHandlerOrCreateVoidImpl(Type aggregateType, Type eventType)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var handlerMethodInfo = aggregateType.GetMethod("Handle", flags, null, new[] { eventType }, null);
            if (handlerMethodInfo == null)
            {
                // void implementation; does not change aggregate
                Debug.WriteLine(
                    "Event handler for {0} not found on {1}. Creating void implementation that does not effect the object data!",
                    eventType.Name,
                    aggregateType.Name);

                return (a, e) => { };
            }

            var pAggregate = Expression.Parameter(typeof(object), "aggregate");
            var pEvent = Expression.Parameter(typeof(object), "event");
            var eventInterfaceType = handlerMethodInfo.GetParameters().Single().ParameterType;

            var methodCall = Expression.Call(
                Expression.Convert(pAggregate, aggregateType),
                handlerMethodInfo,
                // ReSharper disable once PossiblyMistakenUseOfParamsMethod
                Expression.Convert(pEvent, eventInterfaceType));

            return Expression.Lambda<Action<object, object>>(methodCall, pAggregate, pEvent).Compile();
        }
    }
}
