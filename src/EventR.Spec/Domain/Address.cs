namespace EventR.Spec.Domain
{
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class Address
    {
        [DataMember(Order = 1, IsRequired = true)]
        public string Name { get; set; }

        [DataMember(Order = 2, IsRequired = true)]
        public string ZipCode { get; set; }

        [DataMember(Order = 3, IsRequired = false, EmitDefaultValue = false)]
        public string Street { get; set; }

        [DataMember(Order = 4, IsRequired = false, EmitDefaultValue = false)]
        public string City { get; set; }

        [DataMember(Order = 5, IsRequired = false, EmitDefaultValue = false)]
        public string State { get; set; }

        [DataMember(Order = 6, IsRequired = false, EmitDefaultValue = false)]
        public string Country { get; set; }
    }
}
