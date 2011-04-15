using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class JumpGenerator : OpcodeGenerator
    {
        private readonly int address;

        public JumpGenerator(int address, Operand op)
            : base(OpcodeGeneratorKind.Jump)
        {
            if (op.IsVariable)
            {
                throw new ZCompilerException("Variables are not supported for unconditional jumps.");
            }

            this.address = address + (short)op.Value - 2;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            var label = compiler.GetLabel(address);
            label.Branch(); // OPTIMIZE: Can we automatically use br.s?
        }
    }
}
