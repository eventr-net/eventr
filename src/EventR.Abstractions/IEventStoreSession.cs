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
        /// <param name="correlationId">telemetry identifier</param>
        /// <returns>If operation has suceeded <c>true</c>; otherwise <c>false</c>.</returns>
        Task<bool> HydrateAsync<T, TDataSnaphot>(T aggregate, string correlationId = null)
            where T : AggregateRoot<TDataSnaphot>
            where TDataSnaphot : class, new();

        /// <summary>
        /// Loads root aggregate deserialized events.
        /// </summary>
        /// <typeparam name="T">root aggregate type</typeparam>
        /// <typeparam name="TDataSnaphot">root aggregate data snapshot</typeparam>
        /// <param name="aggregate">root aggregate entity</param>
        /// <param name="correlationId">telemetry identifier</param>
        Task<EventsLoad> LoadEventsAsync<T, TDataSnaphot>(T aggregate, string correlationId = null)
            where T : AggregateRoot<TDataSnaphot>
            where TDataSnaphot : class, new();

        /// <summary>
        /// Saves uncommited events from the root aggregate.
        /// </summary>
        /// <typeparam name="T">root aggregate type</typeparam>
        /// <typeparam name="TDataSnaphot">root aggregate data snapshot</typeparam>
        /// <param name="aggregate">root aggregate entity</param>
        /// <param name="correlationId">telemetry identifier</param>
        /// <returns>If operation has suceeded <c>true</c>; otherwise <c>false</c>.</returns>
        Task<bool> SaveUncommitedEventsAsync<T, TDataSnaphot>(T aggregate, string correlationId = null)
            where T : AggregateRoot<TDataSnaphot>
            where TDataSnaphot : class, new();

        /// <summary>
        /// Delete whole event stream.
        /// </summary>
        /// <typeparam name="T">root aggregate type</typeparam>
        /// <typeparam name="TDataSnaphot">root aggregate data snapshot</typeparam>
        /// <param name="aggregate">root aggregate entity</param>
        /// <param name="correlationId">telemetry identifier</param>
        /// <returns>If operation has suceeded <c>true</c>; otherwise <c>false</c>.</returns>
        Task<bool> DeleteStreamAsync<T, TDataSnaphot>(T aggregate, string correlationId = null)
            where T : AggregateRoot<TDataSnaphot>
            where TDataSnaphot : class, new();

        /// <summary>
        /// Delete whole event stream.
        /// </summary>
        /// <param name="streamId">root aggregate entity</param>
        /// <param name="correlationId">telemetry identifier</param>
        /// <returns>If operation has suceeded <c>true</c>; otherwise <c>false</c>.</returns>
        Task<bool> DeleteStreamAsync(string streamId, string correlationId = null);
    }
}
