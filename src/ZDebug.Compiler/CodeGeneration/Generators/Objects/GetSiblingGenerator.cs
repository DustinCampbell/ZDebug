using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class GetSiblingGenerator : OpcodeGenerator
    {
        private readonly Operand op;
        private readonly Variable store;
        private readonly Branch branch;

        public GetSiblingGenerator(Instruction instruction)
            : base(instruction)
        {
            this.op = instruction.Operands[0];
            this.store = instruction.StoreVariable;
            this.branch = instruction.Branch;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            compiler.EmitLoadObjectSibling(op, reuse: ReuseFirstOperand);

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

        public override bool CanReuseFirstOperand
        {
            get { return true; }
        }
    }
}
