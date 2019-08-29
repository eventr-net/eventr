namespace EventR.Abstractions.Attributes
{
    using System;
    using static System.AttributeTargets;

    [AttributeUsage(Interface, Inherited = false, AllowMultiple = false)]
    public sealed class EventAttribute : Attribute
    {
    }
}
