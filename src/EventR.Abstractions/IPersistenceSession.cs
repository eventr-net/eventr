namespace EventR.Abstractions
{
    using System;
    using System.Threading.Tasks;

    public interface IPersistenceSession : IDisposable
    {
        bool SuppressAmbientTransaction { get; }

        /// <summary>
        /// Gets collection of commits that belong to particular event stream.
        /// </summary>
        /// <param name="streamId">Event stream ID</param>
        Task<CommitsLoad> LoadCommits(string streamId);

        /// <summary>
        /// Persists particular <see cref="Commit"/> instance.
        /// </summary>
        /// <param name="commit"><see cref="Commit"/> instance to be persisted.</param>
        /// <returns>If operation has suceeded <c>bool</c>; otherwise <c>false</c>.</returns>
        Task<bool> Save(Commit commit);

        /// <summary>
        /// Deletes both commits and snapshot of the event stream.
        /// </summary>
        /// <param name="streamId">Event stream ID</param>
        /// <returns>If operation has suceeded <c>bool</c>; otherwise <c>false</c>.</returns>
        Task<bool> Delete(string streamId);
    }
}
