using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class GetChildGenerator : OpcodeGenerator
    {
        private readonly Operand op;
        private readonly Variable store;
        private readonly Branch branch;

        public GetChildGenerator(Instruction instruction)
            : base(instruction)
        {
            this.op = instruction.Operands[0];
            this.store = instruction.StoreVariable;
            this.branch = instruction.Branch;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            compiler.EmitLoadObjectChild(op);

            using (var result = il.NewLocal<ushort>())
            {
                result.Store();
                compiler.EmitStoreVariable(store, result);

                result.Load();
                il.Load(0);
                il.Compare.GreaterThan();
                compiler.EmitBranch(branch);
            }
        }
    }
}
