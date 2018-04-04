namespace EventR.Spec.Domain
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class DeliveryReceipt
    {
        [DataMember(Order = 1, IsRequired = true)]
        public Guid OrderId { get; set; }

        [DataMember(Order = 2, IsRequired = true)]
        public DateTimeOffset Delivered { get; set; }

        [DataMember(Order = 3, IsRequired = true)]
        public string Agent { get; set; }
    }
}
