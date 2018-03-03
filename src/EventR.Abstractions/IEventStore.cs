namespace EventR.Abstractions
{
    using System;

    public interface IEventStore : IDisposable
    {
        /// <summary>
        /// Creates an event store session, which is the main object
        /// through which to interact with underlying persistence.
        /// </summary>
        /// <param name="suppressAmbientTransaction">
        ///   Indicates whether the session should try to subscribe to ongoing transaction or not;
        ///   the default is <code>true</code>.
        /// </param>
        /// <returns>session instance</returns>
        IEventStoreSession OpenSession(bool suppressAmbientTransaction = false);

        IPersistence Persistence { get; }
    }
}
