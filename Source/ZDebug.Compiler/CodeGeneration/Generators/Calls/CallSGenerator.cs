using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class CallSGenerator : CallGenerator
    {
        private readonly Variable store;

        public CallSGenerator(Instruction instruction)
            : base(instruction)
        {
            this.store = instruction.StoreVariable;
        }

        protected override void PostCall(ILBuilder il, ICompiler compiler)
        {
            using (var result = il.NewLocal<ushort>())
            {
                result.Store();
                compiler.EmitStoreVariable(store, result, reuse: ReuseStoreVariable);
            }
        }

        public override bool CanReuseStoreVariable
        {
            get { return true; }
        }
    }
}
