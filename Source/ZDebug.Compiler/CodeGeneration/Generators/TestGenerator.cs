using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration
{
    internal class TestGenerator : OpcodeGenerator
    {
        private readonly Operand op1;
        private readonly Operand op2;
        private readonly Branch branch;

        public TestGenerator(Operand op1, Operand op2, Branch branch)
            : base(OpcodeGeneratorKind.Test)
        {
            this.op1 = op1;
            this.op2 = op2;
            this.branch = branch;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            // OPTIMIZE: Use IL evaluation stack if second op is SP and last instruction stored to SP.

            using (var flags = il.NewLocal<ushort>())
            {
                compiler.EmitLoadOperand(op2);
                flags.Store();

                compiler.EmitLoadOperand(op1);
                flags.Load();
                il.Math.And();

                flags.Load();

                il.Compare.Equal();
                compiler.EmitBranch(branch);
            }
        }
    }
}
