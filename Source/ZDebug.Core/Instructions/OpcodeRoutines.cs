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
            Strict.HasStackVariable(i);

            var x = (short)context.GetOperandValue(i.Operands[0]);
            var y = (short)context.GetOperandValue(i.Operands[1]);

            var result = Value.Number((ushort)(x + y));

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine div = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStackVariable(i);

            var x = (short)context.GetOperandValue(i.Operands[0]);
            var y = (short)context.GetOperandValue(i.Operands[1]);

            var result = Value.Number((ushort)(x / y));

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine mod = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStackVariable(i);

            var x = (short)context.GetOperandValue(i.Operands[0]);
            var y = (short)context.GetOperandValue(i.Operands[1]);

            var result = Value.Number((ushort)(x % y));

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine mul = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStackVariable(i);

            var x = (short)context.GetOperandValue(i.Operands[0]);
            var y = (short)context.GetOperandValue(i.Operands[1]);

            var result = Value.Number((ushort)(x * y));

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine sub = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStackVariable(i);

            var x = (short)context.GetOperandValue(i.Operands[0]);
            var y = (short)context.GetOperandValue(i.Operands[1]);

            var result = Value.Number((ushort)(x - y));

            context.WriteVariable(i.StoreVariable, result);
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Call routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine call_vs = (i, context) =>
        {
            Strict.OperandCountInRange(i, 1, 4);
            Strict.HasStackVariable(i);

            var addressOpValue = context.GetOperandValue(i.Operands[0]);
            var address = context.UnpackRoutineAddress(addressOpValue.RawValue);

            var args = i.Operands.Skip(1).ToArray();

            context.Call(address, args, i.StoreVariable);
        };
    }
}
