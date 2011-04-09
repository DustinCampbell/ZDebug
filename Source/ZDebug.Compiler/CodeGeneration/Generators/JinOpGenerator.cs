using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration
{
    internal class JinGenerator : OpcodeGenerator
    {
        private readonly Operand op1;
        private readonly Operand op2;
        private readonly Branch branch;

        public JinGenerator(Operand op1, Operand op2, Branch branch)
            : base(OpcodeGeneratorKind.Jin)
        {
            this.op1 = op1;
            this.op2 = op2;
            this.branch = branch;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            // OPTIMIZE: Use IL evaluation stack if first op is SP and last instruction stored to SP.

            compiler.EmitObjectParentLoad(op1);
            compiler.EmitOperandLoad(op2);

            il.Compare.Equal();
            compiler.EmitBranch(branch);
        }
    }
}
