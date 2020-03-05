namespace EventR.Spec.Serialization
{
    using EventR.Spec.Domain;
    using KellermanSoftware.CompareNetObjects;
    using KellermanSoftware.CompareNetObjects.TypeComparers;
    using System;
    using System.Collections.Generic;

    public class ShoppingCartItemArrayComparer : BaseTypeComparer
    {
        private readonly CollectionComparer cmp;

        public ShoppingCartItemArrayComparer(RootComparer rootComparer)
            : base(rootComparer)
        {
            cmp = new CollectionComparer(rootComparer);
        }

        public override void CompareType(CompareParms parms)
        {
            parms.Object2 = ((List<ShoppingCartItem>)parms.Object2).ToArray();
            parms.Object2Type = typeof(ShoppingCartItem[]);
            cmp.CompareType(parms);
        }

        public override bool IsTypeMatch(Type type1, Type type2)
        {
            return typeof(ShoppingCartItem[]) == type1 && typeof(List<ShoppingCartItem>) == type2;
        }
    }
}
