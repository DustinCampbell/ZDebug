using System;

namespace ZDebug.Core.Instructions
{
    public struct Value : IEquatable<Value>, IComparable, IComparable<Value>
    {
        public readonly ValueKind Kind;
        public readonly ushort RawValue;

        public Value(ValueKind kind, ushort rawValue)
        {
            this.Kind = kind;
            this.RawValue = rawValue;
        }

        public int CompareTo(object obj)
        {
            if (obj is Value)
            {
                return CompareTo((Value)obj);
            }

            throw new ArgumentException("Comparison is only legal between Values.", "obj");
        }

        public int CompareTo(Value other)
        {
            if (this.Kind < other.Kind)
            {
                return -1;
            }
            else if (this.Kind > other.Kind)
            {
                return 1;
            }

            return this.RawValue.CompareTo(other.RawValue);
        }

        public override bool Equals(object obj)
        {
            if (obj is Value)
            {
                return Equals((Value)obj);
            }

            return false;
        }

        public bool Equals(Value other)
        {
            return this.Kind == other.Kind && this.RawValue == other.RawValue;
        }

        public override int GetHashCode()
        {
            return (int)Kind ^ (int)RawValue;
        }

        public override string ToString()
        {
            switch (Kind)
            {
                case ValueKind.Number:
                    return RawValue.ToString("x4");
                default:
                    throw new InvalidOperationException("Invalid kind: " + Kind);
            }
        }

        public static bool operator ==(Value value1, Value value2)
        {
            return value1.Equals(value2);
        }

        public static bool operator !=(Value value1, Value value2)
        {
            return !value1.Equals(value2);
        }

        public static explicit operator ushort(Value value)
        {
            return value.RawValue;
        }

        public static explicit operator short(Value value)
        {
            return (short)value.RawValue;
        }

        public static Value Number(ushort value)
        {
            return new Value(ValueKind.Number, value);
        }

        public static readonly Value Zero = Value.Number(0);
        public static readonly Value One = Value.Number(1);
    }
}
