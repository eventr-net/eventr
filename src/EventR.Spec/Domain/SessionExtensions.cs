namespace EventR.Spec.Domain
{
    using EventR.Abstractions;
    using System.Threading.Tasks;

    public static class SessionExtensions
    {
        public static async Task<bool> HydrateAsync(this IEventStoreSession sess, CustomerAggregate aggregate)
        {
            return await sess.HydrateAsync<CustomerAggregate, Customer>(aggregate).ConfigureAwait(false);
        }

        public static async Task<(CustomerAggregate root, bool ok)> LoadAsync(
            this IEventStoreSession sess,
            string streamId,
            IAggregateRootServices services)
        {
            var aggregate = new CustomerAggregate(streamId, services);
            var success = await sess.HydrateAsync<CustomerAggregate, Customer>(aggregate).ConfigureAwait(false);
            return success ? (aggregate, true) : (null, false);
        }

        public static async Task<bool> SaveUncommitedEventsAsync(this IEventStoreSession sess, CustomerAggregate aggregate)
        {
            return await sess.SaveUncommitedEventsAsync<CustomerAggregate, Customer>(aggregate).ConfigureAwait(false);
        }

        public static async Task<bool> DeleteStream(this IEventStoreSession sess, CustomerAggregate aggregate)
        {
            return await sess.DeleteStreamAsync<CustomerAggregate, Customer>(aggregate).ConfigureAwait(false);
        }
    }
}
