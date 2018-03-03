namespace EventR.Abstractions
{
    public interface IAggregateRootServices
    {
        int ErrorOnStreamLength { get; }

        IEventFactory EventFactory { get; }

        IEventHandlerRegistry EventHandlerRegistry { get; }
    }
}
