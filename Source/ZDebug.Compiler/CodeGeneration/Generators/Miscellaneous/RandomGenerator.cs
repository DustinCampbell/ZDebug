using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;
using ZDebug.Core.Utilities;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class RandomGenerator : OpcodeGenerator
    {
        private readonly Operand rangeOp;
        private readonly Variable store;

        public RandomGenerator(Instruction instruction)
            : base(instruction)
        {
            this.rangeOp = instruction.Operands[0];
            this.store = instruction.StoreVariable;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            using (var range = il.NewLocal<short>())
            using (var result = il.NewLocal<ushort>())
            {
                var seed = il.NewLabel();
                var done = il.NewLabel();

                compiler.EmitLoadOperand(rangeOp);
                il.Convert.ToInt16();
                range.Store();

                range.Load();
                il.Load(0);
                seed.BranchIf(Condition.AtMost, @short: true);

                il.Arguments.LoadMachine();
                range.Load();
                il.Call(Reflection<CompiledZMachine>.GetMethod("NextRandom", Types.Array<short>(), @public: false));

                done.Branch(@short: true);

                seed.Mark();

                il.Arguments.LoadMachine();
                range.Load();
                il.Call(Reflection<CompiledZMachine>.GetMethod("SeedRandom", Types.Array<short>(), @public: false));

                il.Load(0);

                done.Mark();

                il.Convert.ToUInt16();

                result.Store();
                compiler.EmitStoreVariable(store, result);
            }
        }
    }
}
