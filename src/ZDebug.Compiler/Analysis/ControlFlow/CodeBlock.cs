using System.Collections.Generic;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.Analysis.ControlFlow;

internal class CodeBlock : Block
{
    private readonly int address;
    private readonly List<Instruction> instructions;

    public CodeBlock(int address)
        : base(isEntry: false, isExit: false)
    {
        this.address = address;
        instructions = [];
    }

    public void AddInstruction(Instruction instruction) => instructions.Add(instruction);

    public int Address => address;

    public IEnumerable<Instruction> Instructions
    {
        get
        {
            foreach (var instruction in instructions)
            {
                yield return instruction;
            }
        }
    }

    public override string ToString() => "Code block: " + address.ToString("x4");
}
