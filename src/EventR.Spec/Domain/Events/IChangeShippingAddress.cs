namespace EventR.Spec.Domain.Events
{
    using System;

    public interface IChangeShippingAddress
    {
        Guid OrderId { get; set; }

        Address NewShippingAddress { get; set; }
    }
}
