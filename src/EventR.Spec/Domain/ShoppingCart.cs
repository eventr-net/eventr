namespace EventR.Spec.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    [Serializable]
    public sealed class ShoppingCart
    {
        [DataMember(Order = 1, IsRequired = true)]
        public ICollection<ShoppingCartItem> Items { get; set; }

        [DataMember(Order = 2, IsRequired = true)]
        public string Currency { get; set; }
    }
}
