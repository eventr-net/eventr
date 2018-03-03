namespace EventR
{
    using EventR.Abstractions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.Serialization;

    public class EventFactory : IEventFactory
    {
        private const string Suffix = "__Concrete";

        private static readonly Dictionary<Type, Type> Cache = new Dictionary<Type, Type>();

        private static readonly object Sync = new object();

        private static readonly ModuleBuilder Builder = CreateModuleBuilder();

        public EventFactory(IEnumerable<Type> initializeTypes)
        {
            Expect.NotNull(initializeTypes, nameof(initializeTypes));

            foreach (var type in initializeTypes.Where(t => t.IsInterface).Distinct())
            {
                if (Cache.ContainsKey(type))
                {
                    continue;
                }

                lock (Sync)
                {
                    if (Cache.ContainsKey(type))
                    {
                        continue;
                    }

                    var concreteType = CreateConcreteType(type);
                    Cache.Add(type, concreteType);
                }
            }
        }

        private static ModuleBuilder CreateModuleBuilder()
        {
            var @namespace = $"{typeof(EventFactory).Namespace}.{Suffix}";
            return AssemblyBuilder
                .DefineDynamicAssembly(new AssemblyName(@namespace), AssemblyBuilderAccess.Run)
                .DefineDynamicModule(@namespace);
        }

        public T Create<T>(Action<T> action)
        {
            Expect.NotNull(action, "action");
            var @event = (T)Create(typeof(T));
            action(@event);
            return @event;
        }

        public Type GetOriginalType(Type concreteType)
        {
            Expect.NotNull(concreteType, "concreteType");
            // ReSharper disable once InconsistentlySynchronizedField
            return Cache.Where(kvp => kvp.Value == concreteType).Select(kvp => kvp.Key).FirstOrDefault();
        }

        public object Create(Type type)
        {
            Expect.NotNull(type, "type");
            return FormatterServices.GetUninitializedObject(GetConcreteType(type));
        }

        public Type GetConcreteType(Type type)
        {
            if (!type.IsInterface)
            {
                return type;
            }

            lock (Sync)
            {
                Type concreteType;
                if (Cache.TryGetValue(type, out concreteType))
                {
                    return concreteType;
                }

                concreteType = CreateConcreteType(type);
                Cache.Add(type, concreteType);

                return concreteType;
            }
        }

        public Type[] GetKnownEvents()
        {
            return Cache.Keys.ToArray();
        }

        private static Type CreateConcreteType(Type type)
        {
            var typeName = $"{type.Namespace}.{Suffix}.{type.Name}";
            const TypeAttributes typeAttrs =
                TypeAttributes.Serializable | TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed;

            var typeBuilder = Builder.DefineType(typeName, typeAttrs, typeof(object));

            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            typeBuilder.AddInterfaceImplementation(type);

            const MethodAttributes methodAttrs =
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig
                | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.VtableLayoutMask;

            foreach (var propInfo in GetPropertyInfo(type))
            {
                var propName = propInfo.Name;
                var propType = propInfo.PropertyType;
                var fieldBuilder = typeBuilder.DefineField("_" + propName, propType, FieldAttributes.Private);
                var propBuilder = typeBuilder.DefineProperty(
                    propName,
                    propInfo.Attributes | PropertyAttributes.HasDefault,
                    propType,
                    null);
                var getMethodBuilder = typeBuilder.DefineMethod("get_" + propName, methodAttrs, propType, Type.EmptyTypes);
                var setMethodBuilder = typeBuilder.DefineMethod(
                    "set_" + propName,
                    getMethodBuilder.Attributes,
                    null,
                    new[] { propType });
                var getGenerator = getMethodBuilder.GetILGenerator();
                var setGenerator = setMethodBuilder.GetILGenerator();

                getGenerator.Emit(OpCodes.Ldarg_0);
                getGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
                getGenerator.Emit(OpCodes.Ret);

                setGenerator.Emit(OpCodes.Ldarg_0);
                setGenerator.Emit(OpCodes.Ldarg_1);
                setGenerator.Emit(OpCodes.Stfld, fieldBuilder);
                setGenerator.Emit(OpCodes.Ret);

                propBuilder.SetGetMethod(getMethodBuilder);
                propBuilder.SetSetMethod(setMethodBuilder);
            }

            return typeBuilder.CreateTypeInfo();
        }

        private static IEnumerable<PropertyInfo> GetPropertyInfo(Type type)
        {
            var propertyInfo = new List<PropertyInfo>(type.GetProperties());
            foreach (var subType in type.GetInterfaces())
            {
                propertyInfo.AddRange(GetPropertyInfo(subType));
            }

            return propertyInfo;
        }
    }
}
