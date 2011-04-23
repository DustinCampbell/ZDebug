using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class GetParentGenerator : OpcodeGenerator
    {
        private readonly Operand op;
        private readonly Variable store;

        public GetParentGenerator(Instruction instruction)
            : base(instruction)
        {
            this.op = instruction.Operands[0];
            this.store = instruction.StoreVariable;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            compiler.EmitLoadObjectParent(op);

            using (var result = il.NewLocal<ushort>())
            {
                result.Store();
                compiler.EmitStoreVariable(store, result);
            }
        }
    }
}
