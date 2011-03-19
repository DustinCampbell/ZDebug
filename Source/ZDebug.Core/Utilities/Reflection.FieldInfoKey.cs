using System;
using System.Reflection;

namespace ZDebug.Core.Utilities
{
    public static partial class Reflection<T>
    {
        private struct FieldInfoKey : IEquatable<FieldInfoKey>
        {
            private readonly string name;
            private readonly BindingFlags flags;

            public FieldInfoKey(string name, BindingFlags flags)
            {
                this.name = name;
                this.flags = flags;
            }

            public bool Equals(FieldInfoKey other)
            {
                return StringComparer.Ordinal.Equals(name, other.name) &&
                    flags == other.flags;
            }

            public override bool Equals(object obj)
            {
                if (obj is FieldInfoKey)
                {
                    return Equals((FieldInfoKey)obj);
                }

                return false;
            }

            public override int GetHashCode()
            {
                int result = 0;
                result ^= StringComparer.Ordinal.GetHashCode(name);
                result ^= flags.GetHashCode();

                return result;
            }
        }
    }
}
