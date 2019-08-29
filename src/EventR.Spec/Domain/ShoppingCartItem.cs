namespace EventR.Spec.Domain
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage(
        "Microsoft.Performance",
        "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes",
        Justification = "performance (reflection) is not an issue for this type")]
    [Serializable]
    public struct ShoppingCartItem
    {
        public ShoppingCartItem(string sku, int quantity)
        {
            Sku = sku;
            Quantity = quantity;
        }

        public readonly string Sku;
        public readonly int Quantity;
    }
}
