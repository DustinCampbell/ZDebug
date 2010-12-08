using ZDebug.Core.Instructions;

namespace ZDebug.Core.Execution
{
    public interface IExecutionContext
    {
        Value GetOperandValue(Operand operand);

        void WriteVariable(Variable variable, Value value);

        void Call(int address, Operand[] args, Variable storeVariable);

        void Jump(Branch branch);

        int UnpackRoutineAddress(ushort byteAddress);
    }
}
