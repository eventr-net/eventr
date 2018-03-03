namespace EventR.Spec.Domain
{
    using System;
    using System.Collections.Generic;

    public sealed class ShoppingCart
    {
        public ICollection<ShoppingCartItem> Items { get; set; }

        public string Currency { get; set; }
    }
}
