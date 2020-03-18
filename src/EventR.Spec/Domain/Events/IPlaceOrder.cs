namespace EventR.Spec.Domain.Events
{
    using System;
    using System.Runtime.Serialization;

    public interface IPlaceOrder
    {
        [DataMember(Order = 1)]
        Guid Id { get; set; }

        [DataMember(Order = 2)]
        DateTimeOffset Date { get; set; }

        [DataMember(Order = 3)]
        ShoppingCart ShoppingCart { get; set; }

        [DataMember(Order = 4)]
        Address ShippingAddress { get; set; }
    }
}
