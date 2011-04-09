using ZDebug.Compiler.Generate;
using ZDebug.Core.Collections;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration
{
    internal class JeMultiOpGenerator : OpcodeGenerator
    {
        private readonly ReadOnlyArray<Operand> ops;
        private readonly Branch branch;

        public JeMultiOpGenerator(ReadOnlyArray<Operand> ops, Branch branch)
            : base(OpcodeGeneratorKind.JeMultiOp)
        {
            this.ops = ops;
            this.branch = branch;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            // OPTIMIZE: Use IL evaluation stack if first op is SP and last instruction stored to SP.

            using (var x = il.NewLocal<ushort>())
            {
                compiler.EmitOperandLoad(ops[0]);
                x.Store();

                var success = il.NewLabel();
                var done = il.NewLabel();

                for (int j = 1; j < ops.Length; j++)
                {
                    compiler.EmitOperandLoad(ops[j]);
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
    }
}
