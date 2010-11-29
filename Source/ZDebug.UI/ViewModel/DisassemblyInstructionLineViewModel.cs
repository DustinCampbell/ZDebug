using ZDebug.Core.Instructions;

namespace ZDebug.UI.ViewModel
{
    internal sealed class DisassemblyInstructionLineViewModel : DisassemblyLineViewModel
    {
        private readonly Instruction instruction;

        public DisassemblyInstructionLineViewModel(Instruction instruction)
        {
            this.instruction = instruction;
        }

        public int Address
        {
            get { return instruction.Address; }
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
