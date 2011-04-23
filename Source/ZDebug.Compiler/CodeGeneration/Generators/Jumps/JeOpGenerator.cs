using ZDebug.Compiler.Generate;
using ZDebug.Core.Collections;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class JeGenerator : OpcodeGenerator
    {
        private readonly ReadOnlyArray<Operand> ops;
        private readonly Branch branch;

        public JeGenerator(Instruction instruction)
            : base(instruction)
        {
            this.ops = instruction.Operands;
            this.branch = instruction.Branch;
        }

        private void GenerateForTwoOperands(ILBuilder il, ICompiler compiler)
        {
            // OPTIMIZE: Use IL evaluation stack if first op is SP and last instruction stored to SP.

            compiler.EmitLoadOperand(ops[0]);
            compiler.EmitLoadOperand(ops[1]);

            il.Compare.Equal();

            compiler.EmitBranch(branch);
        }

        private void GeneratorForMoreThanTwoOperands(ILBuilder il, ICompiler compiler)
        {
            // OPTIMIZE: Use IL evaluation stack if first op is SP and last instruction stored to SP.

            using (var x = il.NewLocal<ushort>())
            {
                compiler.EmitLoadOperand(ops[0]);
                x.Store();

                var success = il.NewLabel();
                var done = il.NewLabel();

                for (int j = 1; j < ops.Length; j++)
                {
                    compiler.EmitLoadOperand(ops[j]);
                    x.Load();

                    il.Compare.Equal();

                    // no need to write a branch for the last test
                    if (j < ops.Length - 1)
                    {
                        success.BranchIf(Condition.True, @short: true);
                    }
                    else
                    {
                        done.Branch(@short: true);
                    }
                }

                success.Mark();
                il.Load(1);

                done.Mark();
                compiler.EmitBranch(branch);
            }

        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            if (ops.Length == 2)
            {
                GenerateForTwoOperands(il, compiler);
            }
            else if (ops.Length == 3 || ops.Length == 4)
            {
                GeneratorForMoreThanTwoOperands(il, compiler);
            }
        }
    }
}
