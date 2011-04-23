using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class PiracyGenerator : OpcodeGenerator
    {
        private readonly Branch branch;

        public PiracyGenerator(Instruction instruction)
            : base(instruction)
        {
            this.branch = instruction.Branch;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            il.Load(1);
            compiler.EmitBranch(branch);
        }
    }
}
