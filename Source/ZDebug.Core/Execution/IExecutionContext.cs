using ZDebug.Core.Instructions;

namespace ZDebug.Core.Execution
{
    public interface IExecutionContext
    {
        Value GetOperandValue(Operand operand);

        void Call(int address, Operand[] args, Variable storeVariable);

        int UnpackRoutineAddress(ushort byteAddress);
    }
}
