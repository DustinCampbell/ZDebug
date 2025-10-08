using ZDebug.Core.Instructions;

namespace ZDebug.UI.ViewModel;

internal sealed class DisassemblyInstructionLineViewModel : DisassemblyLineViewModel
{
    private readonly Instruction instruction;
    private readonly bool isLast;

    public DisassemblyInstructionLineViewModel(Instruction instruction, bool isLast)
    {
        this.instruction = instruction;
        this.isLast = isLast;
    }

    public int Address => instruction.Address;

    public bool IsLast => isLast;

    public string OpcodeName => instruction.Opcode.Name;

    public Instruction Instruction => instruction;
}
