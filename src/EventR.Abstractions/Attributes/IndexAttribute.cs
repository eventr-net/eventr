namespace EventR.Abstractions.Attributes
{
    using System;
    using static System.AttributeTargets;

    [AttributeUsage(Property, Inherited = true, AllowMultiple = false)]
    public sealed class IndexAttribute : Attribute
    {
        public IndexAttribute(int index)
        {
            Expect.Range(index, 1, int.MaxValue, nameof(index));
            Value = index;
        }

        public int Value { get; }
    }
}
