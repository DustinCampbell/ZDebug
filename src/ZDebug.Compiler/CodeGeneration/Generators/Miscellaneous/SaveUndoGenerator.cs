using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class SaveUndoGenerator : OpcodeGenerator
    {
        private readonly Variable store;

        public SaveUndoGenerator(Instruction instruction)
            : base(instruction)
        {
            this.store = instruction.StoreVariable;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            using (var result = il.NewLocal<ushort>())
            {
                il.Load(-1);
                il.Convert.ToUInt16();

                result.Store();

                compiler.EmitStoreVariable(store, result);
            }
        }
    }
}
