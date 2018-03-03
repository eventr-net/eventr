namespace EventR.Spec.Domain
{
    using System;
    using EventR.Abstractions;

    public sealed class CustomerAggregate : AggregateRoot<Customer>
    {
        public CustomerAggregate(string streamId, IAggregateRootServices services)
        : base(streamId, services)
        { }

        public CustomerAggregate(IAggregateRootServices services)
            : this(Guid.NewGuid().ToString("N"), services)
        { }
    }
}
