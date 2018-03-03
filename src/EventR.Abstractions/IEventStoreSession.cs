namespace EventR.Abstractions
{
    using System;
    using System.Threading.Tasks;

    public interface IEventStoreSession : IDisposable
    {
        /// <summary>
        /// Hydrates root aggregate instance.
        /// </summary>
        /// <typeparam name="T">root aggregate type</typeparam>
        /// <typeparam name="TDataSnaphot">root aggregate data snapshot</typeparam>
        /// <param name="aggregate">root aggregate entity</param>
        /// <returns>If operation has suceeded <c>true</c>; otherwise <c>false</c>.</returns>
        Task<bool> Hydrate<T, TDataSnaphot>(T aggregate)
            where T : AggregateRoot<TDataSnaphot>
            where TDataSnaphot : class, new();

        /// <summary>
        /// Loads root aggregate deserialized events.
        /// </summary>
        /// <typeparam name="T">root aggregate type</typeparam>
        /// <typeparam name="TDataSnaphot">root aggregate data snapshot</typeparam>
        /// <param name="aggregate">root aggregate entity</param>
        Task<EventsLoad> LoadEvents<T, TDataSnaphot>(T aggregate)
            where T : AggregateRoot<TDataSnaphot>
            where TDataSnaphot : class, new();

        /// <summary>
        /// Saves uncommited events from the root aggregate.
        /// </summary>
        /// <typeparam name="T">root aggregate type</typeparam>
        /// <typeparam name="TDataSnaphot">root aggregate data snapshot</typeparam>
        /// <param name="aggregate">root aggregate entity</param>
        /// <returns>If operation has suceeded <c>true</c>; otherwise <c>false</c>.</returns>
        Task<bool> SaveUncommitedEvents<T, TDataSnaphot>(T aggregate)
            where T : AggregateRoot<TDataSnaphot>
            where TDataSnaphot : class, new();

        /// <summary>
        /// Delete whole event stream.
        /// </summary>
        /// <typeparam name="T">root aggregate type</typeparam>
        /// <typeparam name="TDataSnaphot">root aggregate data snapshot</typeparam>
        /// <param name="aggregate">root aggregate entity</param>
        /// <returns>If operation has suceeded <c>true</c>; otherwise <c>false</c>.</returns>
        Task<bool> DeleteStream<T, TDataSnaphot>(T aggregate)
            where T : AggregateRoot<TDataSnaphot>
            where TDataSnaphot : class, new();

        /// <summary>
        /// Delete whole event stream.
        /// </summary>
        /// <param name="streamId">root aggregate entity</param>
        /// <returns>If operation has suceeded <c>true</c>; otherwise <c>false</c>.</returns>
        Task<bool> DeleteStream(string streamId);
    }
}
