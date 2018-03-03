namespace EventR.Spec.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class Order
    {
        [DataMember(Order = 1, IsRequired = true)]
        public Guid Id { get; set; }

        [DataMember(Order = 2, IsRequired = true)]
        public DateTimeOffset Created { get; set; }

        [DataMember(Order = 3, IsRequired = true)]
        public ICollection<OrderLine> Lines { get; set; }

        [DataMember(Order = 4, IsRequired = false, EmitDefaultValue = false)]
        public Address ShippingAddress { get; set; }

        [DataMember(Order = 5, IsRequired = false, EmitDefaultValue = false)]
        public decimal ShippingFee { get; set; }

        [DataMember(Order = 6, IsRequired = false, EmitDefaultValue = false)]
        public DeliveryReceipt DeliveryReceipt { get; set; }
    }
}
