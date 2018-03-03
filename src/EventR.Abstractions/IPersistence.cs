namespace EventR.Abstractions
{
    using System;

    public interface IPersistence : IDisposable
    {
        IPersistenceSession OpenSession(bool suppressAmbientTransaction = false);
    }
}
