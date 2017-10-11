using System;
using System.Reflection;

namespace ZDebug.Core.Utilities
{
    public static partial class Reflection<T>
    {
        private struct ConstructorInfoKey : IEquatable<ConstructorInfoKey>
        {
            private readonly BindingFlags flags;
            private readonly Type[] types;

            public ConstructorInfoKey(BindingFlags flags, Type[] types)
            {
                this.flags = flags;
                this.types = types;
            }

            public bool Equals(ConstructorInfoKey other)
            {
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
                if (obj is ConstructorInfoKey)
                {
                    return Equals((ConstructorInfoKey)obj);
                }

                return false;
            }

            public override int GetHashCode()
            {
                int result = 0;
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
