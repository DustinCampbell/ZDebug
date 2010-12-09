using ZDebug.Core.Instructions;

namespace ZDebug.Core.Execution
{
    public interface IExecutionContext
    {
        Value GetOperandValue(Operand operand);

        Value ReadWord(int address);

        void WriteVariable(Variable variable, Value value);
        void WriteWord(int address, ushort value);

        void Call(int address, Operand[] args, Variable storeVariable);

        void Jump(Branch branch);

        void Return(Value value);

        int UnpackRoutineAddress(ushort byteAddress);
    }
}
