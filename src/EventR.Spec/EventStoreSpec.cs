namespace EventR.Spec
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EventR.Abstractions;
    using EventR.Spec.Domain;
    using KellermanSoftware.CompareNetObjects;
    using Xunit;

    public abstract class EventStoreSpec<T> : IClassFixture<T>
        where T : class, IEventStoreSpecFixture, new()
    {
        protected EventStoreSpec(IEventStoreSpecFixture fixture)
        {
            Expect.NotNull(fixture, nameof(fixture));
            Fixture = fixture;
            Comparer = new CompareLogic();
        }

        protected IEventStoreSpecFixture Fixture { get; }

        protected CompareLogic Comparer { get; }

        [Fact]
        public async Task LoadEventsThrowsOnNullArg()
        {
            using (var sess = Fixture.Store.OpenSession())
            {
                await AssertException.Is<ArgumentNullException>(() => sess.LoadEvents<CustomerAggregate, Customer>(null));
            }
        }

        [Fact]
        public async Task HydrateThrowsOnNullArg()
        {
            using (var sess = Fixture.Store.OpenSession())
            {
                await AssertException.Is<ArgumentNullException>(() => sess.Hydrate<CustomerAggregate, Customer>(null));
            }
        }

        [Fact]
        public async Task SaveUncommitedEventsThrowsOnNullArg()
        {
            using (var sess = Fixture.Store.OpenSession())
            {
                await AssertException.Is<ArgumentNullException>(() => sess.SaveUncommitedEvents<CustomerAggregate, Customer>(null));
            }
        }

        [Fact]
        public async Task SaveUncommitedEventsSucceedsOnNoEvents()
        {
            var r = new CustomerAggregate(Guid.NewGuid().ToString("N"), Fixture.AggregateRootServices);
            using (var sess = Fixture.Store.OpenSession())
            {
                var ok = await sess.SaveUncommitedEvents<CustomerAggregate, Customer>(r).ConfigureAwait(false);
                Assert.True(ok, CreateMessage("saving while no uncommited events exist"));
                Assert.Equal(0, r.CurrentVersion);
            }
        }

        [Fact]
        public async Task DeleteStreamByIdThrowsOnNullArg()
        {
            using (var sess = Fixture.Store.OpenSession())
            {
                await AssertException.Is<ArgumentNullException>(() => sess.DeleteStream((string)null));
            }
        }

        [Fact]
        public async Task DeleteStreamByIdThrowsOnEmptyArg()
        {
            using (var sess = Fixture.Store.OpenSession())
            {
                await AssertException.Is<ArgumentException>(() => sess.DeleteStream(string.Empty));
            }
        }

        [Fact]
        public async Task DeleteStreamThrowsOnNullArg()
        {
            using (var sess = Fixture.Store.OpenSession())
            {
                await AssertException.Is<ArgumentNullException>(() => sess.DeleteStream<CustomerAggregate, Customer>(null));
            }
        }

        [Fact]
        public async Task SaveAndHydrateWithinOneSession()
        {
            var r1 = UseCases.Simple().AsDirtyCustomerAggregate(Fixture.AggregateRootServices);

            using (var sess = Fixture.Store.OpenSession())
            {
                var ok = await sess.SaveUncommitedEvents<CustomerAggregate, Customer>(r1).ConfigureAwait(false);
                Assert.True(ok, CreateMessage("saving uncommited events"));

                var r2 = new CustomerAggregate(r1.StreamId, Fixture.AggregateRootServices);
                ok = await sess.Hydrate<CustomerAggregate, Customer>(r2).ConfigureAwait(false);
                Assert.True(ok, "hydrating previously saved user");
                var result = Comparer.Compare(r1, r2);
                Assert.True(result.AreEqual, CreateMessage(result.DifferencesString));
            }
        }

        [Fact]
        public async Task SaveInsideTransactionScope()
        {
            var r1 = UseCases.Simple().AsDirtyCustomerAggregate(Fixture.AggregateRootServices);

            using (var tx = Util.CreateTransactionScope())
            using (var sess = Fixture.Store.OpenSession())
            {
                var savedOk = await sess.SaveUncommitedEvents<CustomerAggregate, Customer>(r1).ConfigureAwait(false);
                Assert.True(savedOk, CreateMessage("saving uncommited events"));

                tx.Complete();
            }

            var (ok, error) = await Verify(r1.StreamId, r1).ConfigureAwait(false);
            Assert.True(ok, CreateMessage(error));
        }

        [Fact]
        public async Task DropSaveInsideNotCompletedTransactionScope()
        {
            var r1 = UseCases.Simple().AsDirtyCustomerAggregate(Fixture.AggregateRootServices);

            using (var tx = Util.CreateTransactionScope())
            using (var sess = Fixture.Store.OpenSession())
            {
                var ok = await sess.SaveUncommitedEvents<CustomerAggregate, Customer>(r1).ConfigureAwait(false);
                Assert.True(ok, CreateMessage("saving uncommited events"));

                // tx.Complete(); => should ignore the save
            }

            var notExists = await VerifyNotExists(r1.StreamId).ConfigureAwait(false);
            Assert.True(notExists, CreateMessage("stream exists even after delete"));
        }

        [Fact]
        public async Task SaveInsideNotCompletedButSupressedTransactionScope()
        {
            var r1 = UseCases.Simple().AsDirtyCustomerAggregate(Fixture.AggregateRootServices);

            using (var tx = Util.CreateTransactionScope())
            using (var sess = Fixture.Store.OpenSession(suppressAmbientTransaction: true))
            {
                var savedOk = await sess.SaveUncommitedEvents<CustomerAggregate, Customer>(r1).ConfigureAwait(false);
                Assert.True(savedOk, CreateMessage("saving uncommited events"));

                // no tx.Complete(); => should still accept, because tx is supressed,
                // so it should behave like there is none.
            }

            var (ok, error) = await Verify(r1.StreamId, r1).ConfigureAwait(false);
            Assert.True(ok, CreateMessage(error));
        }

        [Fact]
        public async Task DeleteByStreamId()
        {
            var useCase = UseCases.Simple();
            var r1 = await Prepare(Fixture.Store, Fixture.AggregateRootServices, useCase).ConfigureAwait(false);

            using (var sess = Fixture.Store.OpenSession())
            {
                var ok = await sess.DeleteStream(r1.StreamId).ConfigureAwait(false);
                Assert.True(ok, CreateMessage("deleting previously saved aggregate"));
            }

            var notExists = await VerifyNotExists(r1.StreamId).ConfigureAwait(false);
            Assert.True(notExists, CreateMessage("stream exists even after delete"));
        }

        [Fact]
        public async Task LoadExtensionMethodShouldNotThrow()
        {
            var nonExistentStreamId = Guid.NewGuid().ToString("N");
            using (var sess = Fixture.Store.OpenSession())
            {
                var (r, ok) = await sess.Load(nonExistentStreamId, Fixture.AggregateRootServices).ConfigureAwait(false);
                Assert.Null(r);
                Assert.False(ok);
            }
        }

        protected virtual string CreateMessage(string originalMessage, params object[] args)
        {
            var append = $" ; using fixture {typeof(T).Name} configured as {Fixture.Description}";
            return args != null && args.Length > 0
                ? string.Format(originalMessage, args) + append
                : originalMessage + append;
        }

        protected async Task<CustomerAggregate> Prepare(
            IEventStore store,
            IAggregateRootServices services,
            ICollection<Action<CustomerAggregate>> useCaseActions)
        {
            Expect.NotNull(store, nameof(store));
            Expect.NotNull(services, nameof(services));
            Expect.NotEmpty(useCaseActions, nameof(useCaseActions));

            var streamId = Guid.NewGuid().ToString("N");
            var r = new CustomerAggregate(streamId, services);
            using (var sess = store.OpenSession(suppressAmbientTransaction: true))
            {
                foreach (var action in useCaseActions)
                {
                    action(r);
                    var ok = await sess.SaveUncommitedEvents<CustomerAggregate, Customer>(r).ConfigureAwait(false);
                    if (!ok)
                    {
                        throw new Exception("failed to prepare root aggregate");
                    }
                }
            }

            return r;
        }

        protected async Task<(bool ok, string error)> Verify(string streamId, CustomerAggregate expected)
        {
            var r = new CustomerAggregate(streamId, Fixture.AggregateRootServices);
            using (var sess = Fixture.Store.OpenSession(suppressAmbientTransaction: true))
            {
                var ok = await sess.Hydrate<CustomerAggregate, Customer>(r).ConfigureAwait(false);
                if (!ok)
                {
                    return (false, "failed to load & hydrate from the store");
                }
            }

            var result = Comparer.Compare(r, expected);
            return result.AreEqual
                ? (true, string.Empty)
                : (false, result.DifferencesString);
        }

        protected async Task<bool> VerifyNotExists(string streamId)
        {
            using (var sess = Fixture.Store.Persistence.OpenSession(suppressAmbientTransaction: true))
            {
                var commits = await sess.LoadCommits(streamId).ConfigureAwait(false);
                return commits.IsEmpty;
            }
        }
    }
}
