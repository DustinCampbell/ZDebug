using ZDebug.Core.Instructions;

namespace ZDebug.Core.Execution
{
    public sealed partial class Processor
    {
        // constants used for instruction parsing
        private const byte opKind_LargeConstant = 0;
        private const byte opKind_SmallConstant = 1;
        private const byte opKind_Variable = 2;
        private const byte opKind_Omitted = 3;

        // instruction state
        private int startAddress;
        private Opcode opcode;
        private byte[] operandKinds = new byte[8];
        private ushort[] operandValues = new ushort[8];
        private int operandCount;
        private Variable store;
        private Branch branch;
        private ushort[] zwords;

        private void ReadNextInstruction()
        {
            instructions.Address = pc;
            var i = instructions.NextInstruction();
            pc += i.Length;

            executingInstruction = i;

            startAddress = i.Address;
            opcode = i.Opcode;

            var operands = i.Operands;
            var startIndex = operands.StartIndex;
            var innerArray = operands.InnerArray;
            operandCount = i.OperandCount;
            for (int j = 0; j < operandCount; j++)
            {
                operandValues[j] = GetOperandValue(innerArray[startIndex + j]);
            }

            store = i.StoreVariable;
            branch = i.Branch;
            zwords = i.ZText;
        }
    }
}
