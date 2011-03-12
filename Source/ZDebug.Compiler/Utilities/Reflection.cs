using System;
using System.Collections.Generic;
using System.Reflection;

namespace ZDebug.Compiler.Utilities
{
    public static partial class Reflection<T>
    {
        public static readonly Type Type = typeof(T);

        private static readonly Dictionary<FieldInfoKey, FieldInfo> fieldCache = new Dictionary<FieldInfoKey, FieldInfo>();
        private static readonly Dictionary<MethodInfoKey, MethodInfo> methodCache = new Dictionary<MethodInfoKey, MethodInfo>();
        private static readonly Dictionary<ConstructorInfoKey, ConstructorInfo> constructorCache = new Dictionary<ConstructorInfoKey, ConstructorInfo>();

        private static BindingFlags GetFlags(bool @public, bool instance)
        {
            return (@public ? BindingFlags.Public : BindingFlags.NonPublic)
                | (instance ? BindingFlags.Instance : BindingFlags.Static);
        }

        private static string GetNotFoundMessage(string kind, string name = null)
        {
            return name != null
                ? string.Format("Could not find {0}.{1} {2} via reflection.", Type.FullName, name, kind)
                : string.Format("Could not find {0} {1} via reflection.", Type.FullName, kind);
        }

        public static FieldInfo GetField(string name, bool @public = true, bool instance = true)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            var flags = GetFlags(@public, instance);
            var key = new FieldInfoKey(name, flags);

            FieldInfo result;
            if (!fieldCache.TryGetValue(key, out result))
            {
                result = Type.GetField(name, flags);
                if (result == null)
                {
                    throw new ReflectionException(GetNotFoundMessage("field", name));
                }

                fieldCache.Add(key, result);
            }

            return result;
        }

        public static MethodInfo GetMethod(string name, Type[] types = null, bool @public = true, bool instance = true)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            types = types ?? Types.None;

            var flags = GetFlags(@public, instance);
            var key = new MethodInfoKey(name, flags, types);

            MethodInfo result;
            if (!methodCache.TryGetValue(key, out result))
            {
                result = Type.GetMethod(name, flags, binder: null, types: types, modifiers: null);
                if (result == null)
                {
                    throw new ReflectionException(GetNotFoundMessage("method", name));
                }

                methodCache.Add(key, result);
            }

            return result;
        }

        public static ConstructorInfo GetConstructor(Type[] types = null, bool @public = true, bool instance = true)
        {
            types = types ?? Types.None;

            var flags = GetFlags(@public, instance);
            var key = new ConstructorInfoKey(flags, types);

            ConstructorInfo result;
            if (!constructorCache.TryGetValue(key, out result))
            {
                result = Type.GetConstructor(flags, binder: null, types: types, modifiers: null);
                if (result == null)
                {
                    throw new ReflectionException(GetNotFoundMessage("constructor"));
                }

                constructorCache.Add(key, result);
            }

            return result;
        }
    }
}
