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

        int GetChild(int objNum);
        int GetParent(int objNum);
        int GetSibling(int objNum);
        string GetShortName(int objNum);
        int GetNextProperty(int objNum, int propNum);
        int GetPropertyData(int objNum, int propNum);
        int GetPropertyDataAddress(int objNum, int propNum);
        int GetPropertyDataLength(int dataAddress);
        bool HasAttribute(int objNum, int attrNum);
        void ClearAttribute(int objNum, int attrNum);
        void SetAttribute(int objNum, int attrNum);
        void RemoveFromParent(int objNum);
        void MoveTo(int objNum, int destNum);

        ushort[] ReadZWords(int address);
        string ParseZWords(IList<ushort> zwords);

        void SelectOutputStream(int number, bool value);
        void Print(string text);
        void Print(char ch);

        void Randomize(int seed);
        int NextRandom(int range);

        void Quit();
        bool VerifyChecksum();

        IScreen Screen { get; }
    }
}
