namespace EventR.InMemory
{
    using EventR.Abstractions;
    using EventR.Abstractions.Exceptions;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Transactions;
    using Stage = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<int>>;
    using Storage = System.Collections.Generic.Dictionary<string, System.Collections.Generic.SortedList<int, EventR.Abstractions.Commit>>;

    public sealed class InMemoryPersistenceSession : IPersistenceSession
    {
        private readonly Storage data;
        private readonly Stage stage;

        public bool SuppressAmbientTransaction { get; }

        public InMemoryPersistenceSession(Storage data, bool supressAmbientTransaction = false)
        {
            Expect.NotNull(data, "data");
            this.data = data;
            stage = new Stage();
            SuppressAmbientTransaction = supressAmbientTransaction;
        }

        public Task<CommitsLoad> LoadCommitsAsync(string streamId)
        {
            Expect.NotEmpty(streamId, "streamId");

            if (!data.TryGetValue(streamId, out SortedList<int, Commit> stream))
            {
                return Task.FromResult(CommitsLoad.Empty);
            }

            var commits = stream.Select(kvp => kvp.Value).ToArray();
            var result = commits.Length > 0
                            ? new CommitsLoad(commits, commits.Last().Version)
                            : CommitsLoad.Empty;
            return Task.FromResult(result);
        }

        public Task<bool> SaveAsync(Commit commit)
        {
            Expect.NotNull(commit, "commit");
            commit.ThrowIfContainsInvalidData();

            RegisterInStage(commit);

            if (!SuppressAmbientTransaction && Transaction.Current != null)
            {
                Transaction.Current.EnlistVolatile(
                    new EnlistedOperation(commit, SaveImpl),
                    EnlistmentOptions.None);
            }
            else
            {
                SaveImpl(commit);
            }

            return Task.FromResult(true);
        }

        private void RegisterInStage(Commit commit)
        {
            if (stage.ContainsKey(commit.StreamId))
            {
                if (stage[commit.StreamId].Contains(commit.Version))
                {
                    throw new VersionConflictException(commit.StreamId, commit.Version);
                }

                stage[commit.StreamId].Add(commit.Version);
            }
            else
            {
                stage.Add(commit.StreamId, new List<int> { commit.Version });
            }
        }

        public Task<bool> DeleteAsync(string streamId)
        {
            Expect.NotEmpty(streamId, "streamId");

            if (!data.ContainsKey(streamId))
            {
                return Task.FromResult(false);
            }

            data.Remove(streamId);

            return Task.FromResult(true);
        }

        public void Dispose()
        {
        }

        private void SaveImpl(Commit commit)
        {
            var streamId = commit.StreamId;
            if (data.TryGetValue(streamId, out SortedList<int, Commit> stream))
            {
                if (stream.ContainsKey(commit.Version))
                {
                    throw new VersionConflictException(streamId, commit.Version);
                }

                stream.Add(commit.Version, commit);
            }
            else
            {
                stream = new SortedList<int, Commit> { { commit.Version, commit } };
                data.Add(streamId, stream);
            }
        }
    }
}
