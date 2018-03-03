namespace EventR.Abstractions
{
    using System;

    public interface ITranslateTypeIds
    {
        string Translate(Type type);

        Type Translate(string typeId, bool throwOnMissing = true);
    }
}
