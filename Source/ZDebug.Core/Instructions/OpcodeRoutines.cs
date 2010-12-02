using System.Linq;

namespace ZDebug.Core.Instructions
{
    internal static class OpcodeRoutines
    {
        public static readonly OpcodeRoutine call_vs = (i, context) =>
        {
            // TODO: Check args and store variable

            var addressOpValue = context.GetOperandValue(i.Operands[0]);
            var address = context.UnpackRoutineAddress(addressOpValue.RawValue);

            var args = i.Operands.Skip(1).ToArray();

            context.Call(address, args, i.StoreVariable);
        };
    }
}
