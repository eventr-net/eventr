namespace EventR.Spec.Persistence
{
    using EventR.Abstractions;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

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

            using (var tx = Util.CreateTransactionScope())
            using (var sess = sut.OpenSession())
            {
                var ok = await sess.Save(c1).ConfigureAwait(false);
                Assert.True(ok);

                ok = await sess.Save(c2).ConfigureAwait(false);
                Assert.True(ok);

                tx.Complete(); // => should accept changes
            }

            string error;
            Assert.True(Fixture.HasBeenSaved(c1, out error), CreateMessage(error));
            Assert.True(Fixture.HasBeenSaved(c2, out error), CreateMessage(error));
        }

        [Fact]
        public async Task ShouldDropChangesWhenEnlistedInAmbientTransactionAndNotCompleted()
        {
            var c1 = Data.CreateValidCommits(1).First();
            var sut = Fixture.Persistence;

            using (var tx = Util.CreateTransactionScope())
            using (var sess = sut.OpenSession())
            {
                var ok = await sess.Save(c1).ConfigureAwait(false);
                Assert.True(ok);

                // tx disposed without tx.Complete() => should drop changes
            }

            Assert.False(Fixture.HasBeenSaved(c1, out string error), CreateMessage(error));
        }

        [Fact]
        public async Task ShouldIgnoreAmbientTransactionIfRequested()
        {
            var c1 = Data.CreateValidCommits(1).First();
            var sut = Fixture.Persistence;

            using (var tx = Util.CreateTransactionScope())
            using (var sess = sut.OpenSession(suppressAmbientTransaction: true))
            {
                var ok = await sess.Save(c1).ConfigureAwait(false);
                Assert.True(ok);

                // tx disposed without tx.Complete()
                // => still should accept changes, because session is configured not to use ambient transaction
            }

            Assert.True(Fixture.HasBeenSaved(c1, out string error), CreateMessage(error));
        }

        [Fact]
        public async Task ShouldNotRequireAmbientTransaction()
        {
            var c1 = Data.CreateValidCommits(1).First();
            var sut = Fixture.Persistence;

            using (var sess = sut.OpenSession())
            {
                var ok = await sess.Save(c1).ConfigureAwait(false);
                Assert.True(ok);
            }

            Assert.True(Fixture.HasBeenSaved(c1, out string error), CreateMessage(error));
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
                var ok = await sess.Save(c1).ConfigureAwait(false);
                Assert.True(ok);

                await AssertException.Is<VersionConflictException>(
                    () => sess.Save(c2),
                    ex => !string.IsNullOrEmpty(ex.StreamId) && ex.Version > 0);

                tx.Complete();
            }
        }

        [Theory]
        [MemberData(nameof(Data.ValidCommits), MemberType = typeof(Data))]
        public async Task ValidCommitsShouldBeSaved(Commit validCommit)
        {
            var sut = Fixture.Persistence;
            using (var sess = sut.OpenSession())
            {
                var ok = await sess.Save(validCommit).ConfigureAwait(false);
                Assert.True(ok);
            }
        }

        [Theory]
        [MemberData(nameof(Data.InvalidCommits), MemberType = typeof(Data))]
        public async Task InvalidCommitsShouldBeRejected(Commit invalidCommit)
        {
            var sut = Fixture.Persistence;
            using (var sess = sut.OpenSession())
            {
                await AssertException.Is<InvalidPersistenceDataException>(() => sess.Save(invalidCommit));
            }
        }

        [Fact]
        public async Task SaveCommitShouldThrowOnNullArg()
        {
            var sut = Fixture.Persistence;
            using (var sess = sut.OpenSession())
            {
                await AssertException.Is<ArgumentNullException>(() => sess.Save(null));
            }
        }

        [Fact]
        public async Task LoadCommitsShouldThrowOnNullArg()
        {
            var sut = Fixture.Persistence;
            using (var sess = sut.OpenSession())
            {
                await AssertException.Is<ArgumentNullException>(() => sess.LoadCommits(null));
                await AssertException.Is<ArgumentException>(() => sess.LoadCommits(string.Empty));
            }
        }

        [Fact]
        public async Task DeleteStream()
        {
            var c1 = Data.CreateValidCommits(1).First();
            var sut = Fixture.Persistence;

            using (var sess = sut.OpenSession())
            {
                var ok = await sess.Save(c1).ConfigureAwait(false);
                Assert.True(ok);
            }

            string error;
            Assert.True(Fixture.HasBeenSaved(c1, out error), CreateMessage(error));

            using (var sess = sut.OpenSession())
            {
                var ok = await sess.Delete(c1.StreamId).ConfigureAwait(false);
                Assert.True(ok);
            }

            Assert.False(Fixture.HasBeenSaved(c1, out error), CreateMessage("deleted stream should not be available"));
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
