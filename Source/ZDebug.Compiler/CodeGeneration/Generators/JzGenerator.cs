using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration
{
    internal class JzGenerator : OpcodeGenerator
    {
        private readonly Operand op;
        private readonly Branch branch;

        public JzGenerator(Operand op, Branch branch)
            : base(OpcodeGeneratorKind.Jz)
        {
            this.op = op;
            this.branch = branch;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            // OPTIMIZE: Use IL evaluation stack if first op is SP and last instruction stored to SP.
            // OPTIMIZE: Can we do better by using brfalse instead of loading 0 and doing ceq?

            compiler.EmitLoadOperand(op);
            il.Load(0);
            il.Compare.Equal();

            compiler.EmitBranch(branch);
        }
    }
}
