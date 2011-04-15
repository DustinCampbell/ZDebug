using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class UnknownOpcodeGenerator : OpcodeGenerator
    {
        private readonly int address;
        private readonly Opcode opcode;

        public UnknownOpcodeGenerator(Instruction instruction)
            : base(instruction)
        {
            this.address = instruction.Address;
            this.opcode = instruction.Opcode;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            il.RuntimeError(
                string.Format("Unknown opcode: {0} ({1} {2:x2})", opcode.Name, opcode.Kind, opcode.Number));
        }
    }
}
