using System;
namespace ZDebug.Core.Instructions
{
    public struct Operand
    {
        public readonly OperandKind Kind;
        public readonly ushort RawValue;

        public Operand(OperandKind kind, ushort rawValue)
        {
            this.Kind = kind;
            this.RawValue = rawValue;
        }

        public Value AsLargeConstant()
        {
            if (Kind != OperandKind.LargeConstant)
            {
                throw new InvalidOperationException();
            }

            return Value.Number(RawValue);
        }

        public Value AsSmallConstant()
        {
            if (Kind != OperandKind.SmallConstant)
            {
                throw new InvalidOperationException();
            }

            if (RawValue > 255)
            {
                throw new InvalidOperationException();
            }

            return Value.Number((byte)RawValue);
        }

        public Variable AsVariable()
        {
            if (Kind != OperandKind.Variable)
            {
                throw new InvalidOperationException();
            }

            if (RawValue > 255)
            {
                throw new InvalidOperationException();
            }

            return Variable.FromByte((byte)RawValue);
        }
    }
}
