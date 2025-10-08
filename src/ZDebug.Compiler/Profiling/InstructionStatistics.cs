using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.Profiling;

public readonly struct InstructionStatistics
{
    private readonly Instruction instruction;
    private readonly int offset;
    private readonly int size;

    internal InstructionStatistics(Instruction instruction, int offset, int size)
    {
        this.instruction = instruction;
        this.offset = offset;
        this.size = size;
    }

    public Instruction Instruction => instruction;

    public int Offset => offset;

    public int Size => size;
}
