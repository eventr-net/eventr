namespace EventR
{
    using EventR.Abstractions;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    public sealed class TypeIdTranslator : ITranslateTypeIds
    {
        private readonly Dictionary<Type, string> type2Ids = new Dictionary<Type, string>();
        private readonly Dictionary<string, Type> ids2Type = new Dictionary<string, Type>();
        private readonly object sync = new object();

        public TypeIdTranslator(IEnumerable<Type> initializeTypes)
        {
            Expect.NotNull(initializeTypes, nameof(initializeTypes));

            foreach (var type in initializeTypes)
            {
                var ids = CreateShortTypeIds(type);
                type2Ids.Add(type, ids[0]);
                foreach (var id in ids)
                {
                    ids2Type.Add(id, type);
                }
            }
        }

        public string Translate(Type type)
        {
            string id;
            if (type2Ids.TryGetValue(type, out id))
            {
                return id;
            }

            var ids = CreateShortTypeIds(type);
            lock (sync)
            {
                id = ids[0];
                if (!ids2Type.ContainsKey(id))
                {
                    type2Ids.Add(type, ids[0]);
                    foreach (var id2 in ids)
                    {
                        ids2Type.Add(id2, type);
                    }
                }
            }

            return id;
        }

        public Type Translate(string typeId, bool throwOnMissing = true)
        {
            Type t;
            if (ids2Type.TryGetValue(typeId, out t))
            {
                return t;
            }

            if (throwOnMissing)
            {
                throw new EventStoreException($"short ID {typeId} is not mapped to any type");
            }

            return null;
        }

        private static string[] CreateShortTypeIds(Type type)
        {
            var ids = new List<string>();

            var dn = type.GetCustomAttribute<DisplayNameAttribute>();
            if (dn != null)
            {
                ids.Add(dn.DisplayName);
            }

            var caps = new string(type.Name.Split('.').Last().Where(char.IsUpper).ToArray());
            var hashUint32 = MurmurHash2.Hash(Encoding.ASCII.GetBytes(type.FullName));
            var hashBytes = BitConverter.GetBytes(hashUint32);
            var hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
            ids.Add($"{caps}.{hash}");

            return ids.Distinct().ToArray();
        }
    }
}
