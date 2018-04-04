namespace EventR.Spec.Domain.Events
{
    using System;

    public interface IConfirmDelivery
    {
        Guid OrderId { get; set; }

        DateTimeOffset Date { get; set; }

        string Agent { get; set; }
    }
}
