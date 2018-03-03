namespace EventR.Spec
{
    using EventR.Abstractions;
    using System;

    public interface IEventStoreSpecFixture : IDisposable
    {
        IEventStore Store { get; }

        IAggregateRootServices AggregateRootServices { get; }

        string Description { get; }
    }
}
