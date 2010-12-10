using System.Linq;

namespace ZDebug.Core.Instructions
{
    internal static class OpcodeRoutines
    {
        ///////////////////////////////////////////////////////////////////////////////////////////
        // Arithmetic routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine add = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var x = (short)context.GetOperandValue(i.Operands[0]);
            var y = (short)context.GetOperandValue(i.Operands[1]);

            var result = Value.Number((ushort)(x + y));

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine div = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var x = (short)context.GetOperandValue(i.Operands[0]);
            var y = (short)context.GetOperandValue(i.Operands[1]);

            var result = Value.Number((ushort)(x / y));

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine mod = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var x = (short)context.GetOperandValue(i.Operands[0]);
            var y = (short)context.GetOperandValue(i.Operands[1]);

            var result = Value.Number((ushort)(x % y));

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine mul = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var x = (short)context.GetOperandValue(i.Operands[0]);
            var y = (short)context.GetOperandValue(i.Operands[1]);

            var result = Value.Number((ushort)(x * y));

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine sub = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var x = (short)context.GetOperandValue(i.Operands[0]);
            var y = (short)context.GetOperandValue(i.Operands[1]);

            var result = Value.Number((ushort)(x - y));

            context.WriteVariable(i.StoreVariable, result);
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Jump routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine je = (i, context) =>
        {
            Strict.OperandCountInRange(i, 2, 4);
            Strict.HasBranch(i);

            var x = (ushort)context.GetOperandValue(i.Operands[0]);
            var result = i.Operands.Skip(1).Any(op => x == (ushort)context.GetOperandValue(op));

            if (result == i.Branch.Condition)
            {
                context.Jump(i.Branch);
            }
        };

        public static readonly OpcodeRoutine jump = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var offset = (short)context.GetOperandValue(i.Operands[0]);

            context.Jump(offset);
        };

        public static readonly OpcodeRoutine jz = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);
            Strict.HasBranch(i);

            var x = (ushort)context.GetOperandValue(i.Operands[0]);
            var result = x == 0;

            if (result == i.Branch.Condition)
            {
                context.Jump(i.Branch);
            }
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Call routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine call_2s = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var addressOpValue = context.GetOperandValue(i.Operands[0]);
            var address = context.UnpackRoutineAddress(addressOpValue.RawValue);

            var args = i.Operands.Skip(1).ToArray();

            context.Call(address, args, i.StoreVariable);
        };

        public static readonly OpcodeRoutine call_vs = (i, context) =>
        {
            Strict.OperandCountInRange(i, 1, 4);
            Strict.HasStoreVariable(i);

            var addressOpValue = context.GetOperandValue(i.Operands[0]);
            var address = context.UnpackRoutineAddress(addressOpValue.RawValue);

            var args = i.Operands.Skip(1).ToArray();

            context.Call(address, args, i.StoreVariable);
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Return routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine ret = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var value = context.GetOperandValue(i.Operands[0]);

            context.Return(value);
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Load/Store routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine load = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);
            Strict.HasStoreVariable(i);

            var varIdx = (ushort)context.GetOperandValue(i.Operands[0]);
            Strict.IsByte(i, varIdx);

            var variable = Variable.FromByte((byte)varIdx);

            var value = context.ReadVariableIndirectly(variable);

            context.WriteVariable(i.StoreVariable, value);
        };

        public static readonly OpcodeRoutine loadb = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var array = (ushort)context.GetOperandValue(i.Operands[0]);
            var byteIndex = (ushort)context.GetOperandValue(i.Operands[1]);

            var address = array + byteIndex;
            var value = context.ReadByte(address);

            context.WriteVariable(i.StoreVariable, value);
        };

        public static readonly OpcodeRoutine loadw = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var array = (ushort)context.GetOperandValue(i.Operands[0]);
            var wordIndex = (ushort)context.GetOperandValue(i.Operands[1]);

            var address = array + (wordIndex * 2);
            var value = context.ReadWord(address);

            context.WriteVariable(i.StoreVariable, value);
        };

        public static readonly OpcodeRoutine store = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);

            var varIdx = (ushort)context.GetOperandValue(i.Operands[0]);
            Strict.IsByte(i, varIdx);

            var variable = Variable.FromByte((byte)varIdx);
            var value = context.GetOperandValue(i.Operands[1]);

            context.WriteVariableIndirectly(variable, value);
        };

        public static readonly OpcodeRoutine storeb = (i, context) =>
        {
            Strict.OperandCountIs(i, 3);

            var array = (ushort)context.GetOperandValue(i.Operands[0]);
            var byteIndex = (ushort)context.GetOperandValue(i.Operands[1]);
            var value = (ushort)context.GetOperandValue(i.Operands[2]);
            Strict.IsByte(i, value);

            var address = array + byteIndex;

            context.WriteByte(address, (byte)value);
        };

        public static readonly OpcodeRoutine storew = (i, context) =>
        {
            Strict.OperandCountIs(i, 3);

            var array = (ushort)context.GetOperandValue(i.Operands[0]);
            var wordIndex = (ushort)context.GetOperandValue(i.Operands[1]);
            var value = (ushort)context.GetOperandValue(i.Operands[2]);

            var address = array + (wordIndex * 2);

            context.WriteWord(address, value);
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Object routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine put_prop = (i, context) =>
        {
            Strict.OperandCountIs(i, 3);

            var objNum = (ushort)context.GetOperandValue(i.Operands[0]);
            var propNum = (ushort)context.GetOperandValue(i.Operands[1]);
            var value = (ushort)context.GetOperandValue(i.Operands[2]);

            context.WriteProperty(objNum, propNum, value);
        };

        public static readonly OpcodeRoutine test_attr = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasBranch(i);

            var objNum = (ushort)context.GetOperandValue(i.Operands[0]);
            var attrNum = (ushort)context.GetOperandValue(i.Operands[1]);

            var result = context.HasAttribute(objNum, attrNum);

            if (result == i.Branch.Condition)
            {
                context.Jump(i.Branch);
            }
        };
    }
}
