namespace EventR.Spec.Domain.Events
{
    using System;

    public interface IPlaceOrder
    {
        Guid Id { get; set; }

        DateTimeOffset Date { get; set; }

        ShoppingCart ShoppingCart { get; set; }

        Address ShippingAddress { get; set; }
    }
}
