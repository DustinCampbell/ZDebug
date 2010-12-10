using System.Collections.Generic;
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

        void Call(int address, Operand[] args = null, Variable storeVariable = null);

        int GetArgumentCount();

        void Jump(short offset);
        void Jump(Branch branch);

        void Return(Value value);

        int UnpackRoutineAddress(ushort byteAddress);
        int UnpackStringAddress(ushort byteAddress);

        bool HasAttribute(int objNum, int attrNum);

        ushort[] ReadZWords(int address);
        string ParseZWords(IList<ushort> zwords);

        void Print(string text);
        void Print(char ch);
    }
}
