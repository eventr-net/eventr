namespace EventR.Spec
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using EventR.Abstractions;
    using EventR.Spec.Domain;

    public static class Data
    {
        public static async Task<CustomerAggregate> PrepareAggregate(
            IEventStore store,
            IAggregateRootServices services,
            ICollection<Action<CustomerAggregate>> useCaseActions)
        {
            Expect.NotNull(store, nameof(store));
            Expect.NotNull(services, nameof(services));
            Expect.NotEmpty(useCaseActions, nameof(useCaseActions));

            var streamId = Guid.NewGuid().ToString("N").Substring(24);
            var r = new CustomerAggregate(streamId, services);
            using (var sess = store.OpenSession())
            {
                foreach (var action in useCaseActions)
                {
                    action(r);
                    await sess.SaveUncommitedEvents(r).ConfigureAwait(false);
                }
            }

            return r;
        }

        public static async Task<CustomerAggregate> GetAggregate(
            string streamId,
            IEventStore store,
            IAggregateRootServices services)
        {
            var r = new CustomerAggregate(streamId, services);
            using (var sess = store.OpenSession())
            {
                await sess.Hydrate(r).ConfigureAwait(false);
            }

            return r;
        }

        public static async Task<EventsLoad> GetEvents(
            string streamId,
            IEventStore store,
            IAggregateRootServices services)
        {
            var r = new CustomerAggregate(streamId, services);
            using (var sess = store.OpenSession())
            {
                return await sess.LoadEvents<CustomerAggregate, Customer>(r).ConfigureAwait(false);
            }
        }
    }
}
