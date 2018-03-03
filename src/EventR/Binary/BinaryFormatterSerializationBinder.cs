namespace EventR.Binary
{
    using EventR.Abstractions;
    using System;
    using System.Runtime.Serialization;

    public class BinaryFormatterSerializationBinder : SerializationBinder
    {
        private readonly IEventFactory eventFactory;

        public BinaryFormatterSerializationBinder(IEventFactory eventFactory)
        {
            Expect.NotNull(eventFactory, "eventFactory");
            this.eventFactory = eventFactory;
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            Expect.NotEmpty(assemblyName, "assemblyName");
            Expect.NotEmpty(typeName, "typeName");

            var type = Type.GetType(typeName + ", " + assemblyName);
            if (type == null)
            {
                throw new Exception($"Failed to create type '{typeName}, {assemblyName}'");
            }

            if (type.IsInterface)
            {
                var concreteType = eventFactory.GetConcreteType(type);
                return concreteType;
            }

            return type;
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            Expect.NotNull(serializedType, "serializedType");

            var mappedType = eventFactory.GetOriginalType(serializedType);
            if (mappedType != null)
            {
                assemblyName = mappedType.Assembly.GetName().Name;
                typeName = mappedType.FullName;
            }
            else
            {
                assemblyName = serializedType.Assembly.GetName().Name;
                typeName = serializedType.FullName;
            }
        }
    }
}
