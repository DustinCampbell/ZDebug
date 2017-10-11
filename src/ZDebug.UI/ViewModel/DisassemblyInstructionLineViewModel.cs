using ZDebug.Core.Instructions;

namespace ZDebug.UI.ViewModel
{
    internal sealed class DisassemblyInstructionLineViewModel : DisassemblyLineViewModel
    {
        private readonly Instruction instruction;
        private readonly bool isLast;

        public DisassemblyInstructionLineViewModel(Instruction instruction, bool isLast)
        {
            this.instruction = instruction;
            this.isLast = isLast;
        }

        public int Address
        {
            get { return instruction.Address; }
        }

        public bool IsLast
        {
            get { return isLast; }
        }

        public string OpcodeName
        {
            get { return instruction.Opcode.Name; }
        }

        public Instruction Instruction
        {
            get { return instruction; }
        }
    }
}
