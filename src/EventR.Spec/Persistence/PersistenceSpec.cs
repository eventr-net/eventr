namespace EventR.Spec.Persistence
{
    using EventR.Abstractions;
    using EventR.Abstractions.Exceptions;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;
    using Util = EventR.Spec.Util;

    public abstract class PersistenceSpec<T> : IClassFixture<T>
        where T : class, IPersistenceSpecFixture, new()
    {
        protected PersistenceSpec(T fixture)
        {
            Expect.NotNull(fixture, nameof(fixture));
            Fixture = fixture;
        }

        protected T Fixture { get; }

        [Fact]
        public async Task ShouldAcceptChangesWhenEnlistedInAmbientTransaction()
        {
            var commits = Data.CreateValidCommits(2);
            var c1 = commits[0];
            var c2 = commits[1];
            var sut = Fixture.Persistence;

            using (var tx = Spec.Util.CreateTransactionScope())
            using (var sess = sut.OpenSession())
            {
                var ok = await sess.SaveAsync(c1).ConfigureAwait(false);
                Assert.True(ok);

                ok = await sess.SaveAsync(c2).ConfigureAwait(false);
                Assert.True(ok);

                tx.Complete(); // => should accept changes
            }

            var (ok1, error1) = await Fixture.HasBeenSavedAsync(c1).ConfigureAwait(false);
            var (ok2, error2) = await Fixture.HasBeenSavedAsync(c2).ConfigureAwait(false);
            Assert.True(ok1, CreateMessage(error1));
            Assert.True(ok2, CreateMessage(error2));
        }

        [Fact]
        public async Task ShouldDropChangesWhenEnlistedInAmbientTransactionAndNotCompleted()
        {
            var c1 = Data.CreateValidCommits(1).First();
            var sut = Fixture.Persistence;

            using (var tx = Spec.Util.CreateTransactionScope())
            using (var sess = sut.OpenSession())
            {
                var ok = await sess.SaveAsync(c1).ConfigureAwait(false);
                Assert.True(ok);

                // tx disposed without tx.Complete() => should drop changes
            }

            var (ok1, error1) = await Fixture.HasBeenSavedAsync(c1).ConfigureAwait(false);
            Assert.False(ok1, CreateMessage(error1));
        }

        [Fact]
        public async Task ShouldIgnoreAmbientTransactionIfRequested()
        {
            var c1 = Data.CreateValidCommits(1).First();
            var sut = Fixture.Persistence;

            using (var tx = Util.CreateTransactionScope())
            using (var sess = sut.OpenSession(suppressAmbientTransaction: true))
            {
                var ok = await sess.SaveAsync(c1).ConfigureAwait(false);
                Assert.True(ok);

                // tx disposed without tx.Complete()
                // => still should accept changes, because session is configured not to use ambient transaction
            }

            var (ok1, error1) = await Fixture.HasBeenSavedAsync(c1).ConfigureAwait(false);
            Assert.True(ok1, CreateMessage(error1));
        }

        [Fact]
        public async Task ShouldNotRequireAmbientTransaction()
        {
            var c1 = Data.CreateValidCommits(1).First();
            var sut = Fixture.Persistence;

            using (var sess = sut.OpenSession())
            {
                var ok = await sess.SaveAsync(c1).ConfigureAwait(false);
                Assert.True(ok);
            }

            var (ok1, error1) = await Fixture.HasBeenSavedAsync(c1).ConfigureAwait(false);
            Assert.True(ok1, CreateMessage(error1));
        }

        [Fact]
        public async Task ShouldNotAllowSavingCommitsWithSameStreamIdAndVersion()
        {
            var commits = Data.CreateValidCommits(2);
            var c1 = commits[0];
            var c2 = commits[1];
            c2.StreamId = c1.StreamId;
            c2.Version = c1.Version;

            var sut = Fixture.Persistence;

            using (var tx = Util.CreateTransactionScope())
            using (var sess = sut.OpenSession())
            {
                var ok = await sess.SaveAsync(c1).ConfigureAwait(false);
                Assert.True(ok);

                await AssertException.Is<VersionConflictException>(
                    () => sess.SaveAsync(c2),
                    ex => !string.IsNullOrEmpty(ex.StreamId) && ex.Version > 0);

                tx.Complete();
            }
        }

        [Theory]
        [MemberData(nameof(Data.ValidCommits), MemberType = typeof(Data))]
        public async Task ValidCommitsShouldBeSaved(Commit validCommit, int testIndex)
        {
            var sut = Fixture.Persistence;
            using (var sess = sut.OpenSession())
            {
                var ok = await sess.SaveAsync(validCommit).ConfigureAwait(false);
                Assert.True(ok, $"Test index: {testIndex} has failed.");
            }
        }

        [Theory]
        [MemberData(nameof(Data.InvalidCommits), MemberType = typeof(Data))]
        public async Task InvalidCommitsShouldBeRejected(Commit invalidCommit, int testIndex)
        {
            var sut = Fixture.Persistence;
            using (var sess = sut.OpenSession())
            {
                await AssertException.Is<InvalidPersistenceDataException>(() => sess.SaveAsync(invalidCommit), testIndex: testIndex);
            }
        }

        [Fact]
        public async Task SaveCommitShouldThrowOnNullArg()
        {
            var sut = Fixture.Persistence;
            using (var sess = sut.OpenSession())
            {
                await AssertException.Is<ArgumentNullException>(() => sess.SaveAsync(null));
            }
        }

        [Fact]
        public async Task LoadCommitsShouldThrowOnNullArg()
        {
            var sut = Fixture.Persistence;
            using (var sess = sut.OpenSession())
            {
                await AssertException.Is<ArgumentNullException>(() => sess.LoadCommitsAsync(null));
                await AssertException.Is<ArgumentException>(() => sess.LoadCommitsAsync(string.Empty));
            }
        }

        [Fact]
        public async Task DeleteStream()
        {
            var c1 = Data.CreateValidCommits(1).First();
            var sut = Fixture.Persistence;

            using (var sess = sut.OpenSession())
            {
                var ok = await sess.SaveAsync(c1).ConfigureAwait(false);
                Assert.True(ok);
            }

            var (ok1, error1) = await Fixture.HasBeenSavedAsync(c1).ConfigureAwait(false);
            Assert.True(ok1, CreateMessage(error1));

            using (var sess = sut.OpenSession())
            {
                var ok = await sess.DeleteAsync(c1.StreamId).ConfigureAwait(false);
                Assert.True(ok);
            }

            var (ok2, _) = await Fixture.HasBeenSavedAsync(c1).ConfigureAwait(false);
            Assert.False(ok2, CreateMessage("deleted stream should not be available"));
        }

        protected virtual string CreateMessage(string originalMessage, params object[] args)
        {
            var append = $" ; using fixture {typeof(T).Name} configured as {Fixture.Description}";
            return args != null && args.Length > 0
                ? string.Format(originalMessage, args) + append
                : originalMessage + append;
        }
    }
}
