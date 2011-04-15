using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class UnknownOpcodeGenerator : OpcodeGenerator
    {
        private readonly int address;
        private readonly Opcode opcode;

        public UnknownOpcodeGenerator(int address, Opcode opcode)
            : base(OpcodeGeneratorKind.Unknown)
        {
            this.address = address;
            this.opcode = opcode;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            il.RuntimeError(string.Format("Unknown opcode at {0:x4}: {1}", address, opcode.Name));
        }
    }
}
