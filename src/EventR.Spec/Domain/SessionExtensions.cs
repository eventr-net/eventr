namespace EventR.Spec.Domain
{
    using EventR.Abstractions;
    using System.Threading.Tasks;

    public static class SessionExtensions
    {
        public static async Task<bool> Hydrate(this IEventStoreSession sess, CustomerAggregate aggregate)
        {
            return await sess.Hydrate<CustomerAggregate, Customer>(aggregate)
                             .ConfigureAwait(false);
        }

        public static async Task<(CustomerAggregate root, bool ok)> Load(
            this IEventStoreSession sess,
            string streamId,
            IAggregateRootServices services)
        {
            var aggregate = new CustomerAggregate(streamId, services);
            var success = await sess.Hydrate<CustomerAggregate, Customer>(aggregate)
                                    .ConfigureAwait(false);
            return success ? (aggregate, true) : (null, false);
        }

        public static async Task<bool> SaveUncommitedEvents(this IEventStoreSession sess, CustomerAggregate aggregate)
        {
            return await sess.SaveUncommitedEvents<CustomerAggregate, Customer>(aggregate)
                             .ConfigureAwait(false);
        }

        public static async Task<bool> DeleteStream(this IEventStoreSession sess, CustomerAggregate aggregate)
        {
            return await sess.DeleteStream<CustomerAggregate, Customer>(aggregate)
                             .ConfigureAwait(false);
        }
    }
}
