using System;
namespace ZDebug.Core.Instructions
{
    public struct Operand
    {
        private readonly OperandKind kind;
        private readonly ushort rawValue;

        public Operand(OperandKind kind, ushort rawValue)
        {
            this.kind = kind;
            this.rawValue = rawValue;
        }

        public OperandKind Kind
        {
            get { return kind; }
        }

        public ushort RawValue
        {
            get { return rawValue; }
        }

        public Value AsLargeConstant()
        {
            if (kind != OperandKind.LargeConstant)
            {
                throw new InvalidOperationException();
            }

            return Value.Number(rawValue);
        }

        public Value AsSmallConstant()
        {
            if (kind != OperandKind.SmallConstant)
            {
                throw new InvalidOperationException();
            }

            if (rawValue > 255)
            {
                throw new InvalidOperationException();
            }

            return Value.Number((byte)rawValue);
        }

        public Variable AsVariable()
        {
            if (kind != OperandKind.Variable)
            {
                throw new InvalidOperationException();
            }

            if (rawValue > 255)
            {
                throw new InvalidOperationException();
            }

            return Variable.FromByte((byte)rawValue);
        }
    }
}
