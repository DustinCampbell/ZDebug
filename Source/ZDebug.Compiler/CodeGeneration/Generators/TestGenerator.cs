using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class TestGenerator : OpcodeGenerator
    {
        private readonly Operand op1;
        private readonly Operand op2;
        private readonly Branch branch;

        public TestGenerator(Instruction instruction)
            : base(instruction)
        {
            this.op1 = instruction.Operands[0];
            this.op2 = instruction.Operands[1];
            this.branch = instruction.Branch;
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
