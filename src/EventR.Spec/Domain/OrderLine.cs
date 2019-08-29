namespace EventR.Spec.Domain
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    [Serializable]
    public sealed class OrderLine
    {
        [DataMember(Order = 1, IsRequired = true)]
        public string Sku { get; set; }

        [DataMember(Order = 2, IsRequired = true)]
        public int Quantity { get; set; }

        [DataMember(Order = 1, IsRequired = false, EmitDefaultValue = false)]
        public string Label { get; set; }

        [DataMember(Order = 3, IsRequired = false, EmitDefaultValue = false)]
        public decimal Price { get; set; }

        [DataMember(Order = 4, IsRequired = false, EmitDefaultValue = false)]
        public decimal Tax { get; set; }
    }
}
