namespace EventR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using EventR.Abstractions;

    public static class Util
    {
        public static Type[] FindEventTypes(Func<Type, bool> predicate, IEnumerable<Assembly> assemblies)
        {
            Expect.NotNull(assemblies, nameof(assemblies));

            var q = assemblies.SelectMany(a => a.GetTypes()).Where(t =>
                        t.IsPublic
                        && (t.IsInterface || (t.IsClass && !t.IsAbstract))
                        && !t.IsGenericType
                        && t.GetCustomAttribute<CompilerGeneratedAttribute>() == null);
            if (predicate != null)
            {
                q = q.Where(predicate);
            }

            return q.ToArray();
        }

        public static Type[] FindEventTypes(string nsEndsWith, IEnumerable<Assembly> assemblies)
        {
            Expect.NotEmpty(nsEndsWith, nameof(nsEndsWith));
            return FindEventTypes(t => t.Namespace != null && t.Namespace.EndsWith(nsEndsWith), assemblies);
        }

        /// <summary>
        /// Gets qualified name for type including assembly name, but without assembly version, culture, etc.
        /// </summary>
        public static string QualifiedNameWithoutVersion(this Type t)
        {
            return $"{t.FullName}, {t.Assembly.GetName().Name}";
        }
    }
}
