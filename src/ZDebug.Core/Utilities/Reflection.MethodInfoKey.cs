using System;
using System.Reflection;

namespace ZDebug.Core.Utilities
{
    public static partial class Reflection<T>
    {
        private struct MethodInfoKey : IEquatable<MethodInfoKey>
        {
            private readonly string name;
            private readonly BindingFlags flags;
            private readonly Type[] types;

            public MethodInfoKey(string name, BindingFlags flags, Type[] types)
            {
                this.name = name;
                this.flags = flags;
                this.types = types;
            }

            public bool Equals(MethodInfoKey other)
            {
                if (!StringComparer.Ordinal.Equals(name, other.name))
                {
                    return false;
                }

                if (flags != other.flags)
                {
                    return false;
                }

                if (types.Length != other.types.Length)
                {
                    return false;
                }

                for (int i = 0; i < types.Length; i++)
                {
                    if (!types[i].Equals(other.types[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            public override bool Equals(object obj)
            {
                if (obj is MethodInfoKey)
                {
                    return Equals((MethodInfoKey)obj);
                }

                return false;
            }

            public override int GetHashCode()
            {
                int result = 0;
                result ^= StringComparer.Ordinal.GetHashCode(name);
                result ^= flags.GetHashCode();

                foreach (var type in types)
                {
                    result ^= type.GetHashCode();
                }

                return result;
            }
        }
    }
}
