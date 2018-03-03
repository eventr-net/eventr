namespace EventR
{
    using System;

    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class ForceAssemblyReferenceAttribute : Attribute
    {
        public ForceAssemblyReferenceAttribute(Type forcedType)
        {
            Action<Type> noop = _ => { };
            noop(forcedType);
        }
    }
}
