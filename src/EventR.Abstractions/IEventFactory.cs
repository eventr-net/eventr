namespace EventR.Abstractions
{
    using System;

    /// <summary>
    /// Provides mapping of interfaces to concrete types in case there is no concrete type implementing it.
    /// It allows definition of the event to be only intreface without having to write also concrete class,
    /// because this factory will create that concrete type on-the-fly.
    /// It is important in serialization process, because you must have a concrete type in order to serialize
    /// the data.
    /// </summary>
    public interface IEventFactory
    {
        /// <summary>
        /// Creates concrete type whenever necessary based on nature of T.
        /// </summary>
        T Create<T>(Action<T> action);

        /// <summary>
        /// Creates concrete type whenever necessary based on nature of passed type.
        /// </summary>
        object Create(Type type);

        /// <summary>
        ///
        /// </summary>
        /// <param name="type"></param>
        Type GetConcreteType(Type type);

        /// <summary>
        /// If some interface has been mapped to concrete type; this provides backward mapping of such relation.
        /// </summary>
        Type GetOriginalType(Type concreteType);

        Type[] GetKnownEvents();
    }
}
