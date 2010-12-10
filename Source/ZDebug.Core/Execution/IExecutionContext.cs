using ZDebug.Core.Instructions;

namespace ZDebug.Core.Execution
{
    public interface IExecutionContext
    {
        Value GetOperandValue(Operand operand);

        Value ReadByte(int address);
        Value ReadVariable(Variable variable);
        Value ReadVariableIndirectly(Variable variable);
        Value ReadWord(int address);

        void WriteByte(int address, byte value);
        void WriteProperty(int objNum, int propNum, ushort value);
        void WriteVariable(Variable variable, Value value);
        void WriteVariableIndirectly(Variable variable, Value value);
        void WriteWord(int address, ushort value);

        void Call(int address, Operand[] args, Variable storeVariable);

        void Jump(short offset);
        void Jump(Branch branch);

        void Return(Value value);

        int UnpackRoutineAddress(ushort byteAddress);

        bool HasAttribute(int objNum, int attrNum);
    }
}
