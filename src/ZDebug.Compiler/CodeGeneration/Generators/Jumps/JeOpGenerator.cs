using System;
using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class JeGenerator : OpcodeGenerator
    {
        private ReadOnlySpan<Operand> Ops => Instruction.Operands;
        private Branch Branch => Instruction.Branch;

        public JeGenerator(Instruction instruction)
            : base(instruction)
        {
        }

        private void GenerateForTwoOperands(ILBuilder il, ICompiler compiler)
        {
            if (ReuseFirstOperand)
            {
                compiler.EmitLoadOperand(Ops[1]);
            }
            else if (ReuseSecondOperand)
            {
                compiler.EmitLoadOperand(Ops[0]);
            }
            else
            {
                compiler.EmitLoadOperand(Ops[0]);
                compiler.EmitLoadOperand(Ops[1]);
            }

            il.Compare.Equal();

            compiler.EmitBranch(Branch);
        }

        private void GeneratorForMoreThanTwoOperands(ILBuilder il, ICompiler compiler)
        {
            using (var x = il.NewLocal<ushort>())
            {
                if (!ReuseFirstOperand)
                {
                    compiler.EmitLoadOperand(Ops[0]);
                }

                x.Store();

                var success = il.NewLabel();
                var done = il.NewLabel();

                for (int j = 1; j < Ops.Length; j++)
                {
                    compiler.EmitLoadOperand(Ops[j]);
                    x.Load();

                    il.Compare.Equal();

                    // no need to write a branch for the last test
                    if (j < Ops.Length - 1)
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
                compiler.EmitBranch(Branch);
            }

        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            if (Ops.Length == 2)
            {
                GenerateForTwoOperands(il, compiler);
            }
            else if (Ops.Length == 3 || Ops.Length == 4)
            {
                GeneratorForMoreThanTwoOperands(il, compiler);
            }
        }

        public override bool CanReuseFirstOperand => true;

        public override bool CanReuseSecondOperand => Ops.Length == 2;
    }
}
