namespace EventR.Spec.Domain.Events
{
    using System;

    public interface IConfirmDelivery
    {
        string OrderId { get; set; }

        DateTimeOffset Date { get; set; }

        string Agent { get; set; }
    }
}
