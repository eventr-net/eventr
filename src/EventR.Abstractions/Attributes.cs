namespace EventR.Abstractions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using static System.AttributeTargets;

    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402", Justification = "simplest 'close' classes and rarely touched")]
    [AttributeUsage(Interface, Inherited = false, AllowMultiple = false)]
    public sealed class EventAttribute : Attribute
    {
    }

    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402", Justification = "simplest 'close' classes and rarely touched")]
    [AttributeUsage(Property, Inherited = true, AllowMultiple = false)]
    public sealed class IndexAttribute : Attribute
    {
        public IndexAttribute(int index)
        {
            Expect.Range(index, 1, int.MaxValue, nameof(index));
            Value = index;
        }

        public int Value { get; private set; }
    }
}
