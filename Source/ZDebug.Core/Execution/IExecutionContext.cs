using ZDebug.Core.Instructions;
using ZDebug.Core.Text;

namespace ZDebug.Core.Execution
{
    public interface IExecutionContext
    {
        byte ReadByte(int address);
        ushort ReadVariable(Variable variable);
        ushort ReadVariableIndirectly(Variable variable);
        ushort ReadWord(int address);

        void WriteByte(int address, byte value);
        void WriteProperty(int objNum, int propNum, ushort value);
        void WriteVariable(Variable variable, ushort value);
        void WriteVariableIndirectly(Variable variable, ushort value);
        void WriteWord(int address, ushort value);

        void Call(int address, ushort[] opValues, int opCount, Variable storeVariable = null);

        int GetArgumentCount();

        void Jump(short offset);
        void Jump(Branch branch);

        void Return(ushort value);

        int UnpackRoutineAddress(ushort byteAddress);
        int UnpackStringAddress(ushort byteAddress);

        int GetChild(ushort objNum);
        int GetParent(ushort objNum);
        int GetSibling(ushort objNum);
        string GetShortName(int objNum);
        int GetNextProperty(int objNum, int propNum);
        int GetPropertyData(int objNum, int propNum);
        int GetPropertyDataAddress(int objNum, int propNum);
        int GetPropertyDataLength(int dataAddress);
        bool HasAttribute(int objNum, int attrNum);
        void ClearAttribute(int objNum, int attrNum);
        void SetAttribute(int objNum, int attrNum);
        void RemoveFromParent(ushort objNum);
        void MoveTo(ushort objNum, ushort destNum);

        ushort[] ReadZWords(int address);
        string ParseZWords(ushort[] zwords);

        void SelectScreenStream();
        void DeselectScreenStream();

        void SelectTranscriptStream();
        void DeselectTranscriptStream();

        void SelectMemoryStream(int address);
        void DeselectMemoryStream();

        void Print(string text);
        void Print(char ch);

        ZCommandToken[] TokenizeCommand(string commandText, int? dictionaryAddress = null);
        void TokenizeLine(ushort text, ushort parse, ushort dictionary, bool flag);

        bool TryLookupWord(string word, int? dictionaryAddress, out ushort address);

        void Randomize(int seed);
        int NextRandom(int range);

        void Quit();
        bool VerifyChecksum();

        IScreen Screen { get; }
        IMessageLog MessageLog { get; }
    }
}
