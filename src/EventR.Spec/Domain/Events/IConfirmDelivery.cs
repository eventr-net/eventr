namespace EventR.Spec.Domain.Events
{
    using System;
    using System.Runtime.Serialization;

    public interface IConfirmDelivery
    {
        [DataMember(Order = 1)]
        Guid OrderId { get; set; }

        [DataMember(Order = 2)]
        DateTimeOffset Date { get; set; }

        [DataMember(Order = 3)]
        string Agent { get; set; }
    }
}
