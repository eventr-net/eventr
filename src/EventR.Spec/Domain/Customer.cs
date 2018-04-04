namespace EventR.Spec.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class Customer
    {
        [DataMember(Order = 1, IsRequired = true)]
        public string Email { get; set; }

        [DataMember(Order = 2, IsRequired = true)]
        public DateTimeOffset Created { get; set; }

        [DataMember(Order = 3, IsRequired = true)]
        public string PreferredLanguage { get; set; }

        [DataMember(Order = 4, IsRequired = false, EmitDefaultValue = false)]
        public int? Age { get; set; }

        [DataMember(Order = 5, IsRequired = false, EmitDefaultValue = false)]
        public ICollection<Order> Orders { get; set; }

        [DataMember(Order = 6, IsRequired = false, EmitDefaultValue = false)]
        public bool IsTerminated { get; set; }

        [DataMember(Order = 7, IsRequired = false, EmitDefaultValue = false)]
        public TerminateReason TerminateReason { get; set; }
    }
}
