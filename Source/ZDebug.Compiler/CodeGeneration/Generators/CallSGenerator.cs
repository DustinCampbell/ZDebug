using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class CallSGenerator : OpcodeGenerator
    {
        private readonly Variable store;

        public CallSGenerator(Variable store)
            : base(OpcodeGeneratorKind.CallS)
        {
            this.store = store;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            compiler.EmitCall();

            using (var result = il.NewLocal<ushort>())
            {
                result.Store();
                compiler.EmitStoreVariable(store, result);
            }
        }
    }
}
