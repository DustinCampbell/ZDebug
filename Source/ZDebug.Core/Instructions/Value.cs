using System;

namespace ZDebug.Core.Instructions
{
    public struct Value : IEquatable<Value>, IComparable, IComparable<Value>
    {
        private readonly ValueKind kind;
        private readonly ushort rawValue;

        public Value(ValueKind kind, ushort rawValue)
        {
            this.kind = kind;
            this.rawValue = rawValue;
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
            if (this.kind < other.kind)
            {
                return -1;
            }
            else if (this.kind > other.kind)
            {
                return 1;
            }

            return this.rawValue.CompareTo(other.rawValue);
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
            return this.kind == other.kind && this.rawValue == other.rawValue;
        }

        public override int GetHashCode()
        {
            return (int)kind ^ (int)rawValue;
        }

        public override string ToString()
        {
            switch (kind)
            {
                case ValueKind.Number:
                    return rawValue.ToString("x4");
                default:
                    throw new InvalidOperationException("Invalid kind: " + kind);
            }
        }

        public ValueKind Kind
        {
            get { return kind; }
        }

        public ushort RawValue
        {
            get { return rawValue; }
        }

        public static explicit operator ushort(Value value)
        {
            return value.RawValue;
        }

        public static explicit operator short(Value value)
        {
            return (short)value.RawValue;
        }

        private static readonly Value zero = new Value(ValueKind.Number, 0);

        public static Value Zero
        {
            get { return zero; }
        }
    }
}
