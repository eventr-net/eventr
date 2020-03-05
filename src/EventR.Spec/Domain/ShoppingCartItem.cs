namespace EventR.Spec.Domain
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    [Serializable]
    public sealed class ShoppingCartItem
    {
        [DataMember(Order = 1, IsRequired = true)]
        public string Sku { get; set; }

        [DataMember(Order = 2, IsRequired = true)]
        public int Quantity { get; set; }

        public ShoppingCartItem(string sku, int quantity)
        {
            Sku = sku;
            Quantity = quantity;
        }

        // Most serializers have trouble when parameterless ctor is not available
        private ShoppingCartItem()
        { }
    }
}
