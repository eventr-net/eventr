namespace EventR.Abstractions
{
    /// <summary>
    /// Factory for supported serializers.
    /// </summary>
    public interface IProvideSerializers
    {
        /// <summary>
        /// Gets serializer instance based on serializer ID.
        /// </summary>
        ISerializeEvents Get(string serializerId);

        /// <summary>
        /// Gets default serializer instance.
        /// </summary>
        ISerializeEvents DefaultSerializer { get; }
    }
}
