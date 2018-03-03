namespace EventR.Abstractions
{
    using System;

    /// <summary>
    /// Efficient registry (cache) for private methods that handle events on domain model objects.
    /// </summary>
    public interface IEventHandlerRegistry
    {
        /// <summary>
        /// Gets handler for given event on particular root aggregate instance.
        /// </summary>
        /// <param name="aggregate">Root aggregate (domain model) instance.</param>
        /// <param name="eventType">Event type; often an interface</param>
        Action<object> GetHandler(object aggregate, Type eventType);
    }
}
