namespace EventR.Spec.Domain.Events
{
    using System;

    public interface IAcceptOrder
    {
        DateTimeOffset Date { get; set; }

        ShoppingCart ShoppingCart { get; set; }

        Address ShippingAddress { get; set; }
    }
}
