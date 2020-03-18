namespace EventR.Spec.Domain.Events
{
    using System;
    using System.Runtime.Serialization;

    public interface IChangeShippingAddress
    {
        [DataMember(Order = 1)]
        Guid OrderId { get; set; }

        [DataMember(Order = 2)]
        Address NewShippingAddress { get; set; }
    }
}
