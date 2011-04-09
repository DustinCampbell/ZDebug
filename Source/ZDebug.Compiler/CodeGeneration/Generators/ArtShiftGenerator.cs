using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration
{
    internal class ArtShiftGenerator : OpcodeGenerator
    {
        private readonly Operand op1;
        private readonly Operand op2;
        private readonly Variable store;

        public ArtShiftGenerator(Operand op1, Operand op2, Variable store)
            : base(OpcodeGeneratorKind.ArtShift)
        {
            this.op1 = op1;
            this.op2 = op2;
            this.store = store;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            // OPTIMIZE: Use IL evaluation stack if first op is SP and last instruction stored to SP.

            using (var number = il.NewLocal<short>())
            using (var places = il.NewLocal<int>())
            {
                compiler.EmitOperandLoad(op1);
                il.Convert.ToInt16();
                number.Store();

                compiler.EmitOperandLoad(op2);
                il.Convert.ToInt16();
                places.Store();

                var positivePlaces = il.NewLabel();
                places.Load();
                il.Load(0);
                positivePlaces.BranchIf(Condition.GreaterThan, @short: true);

                number.Load();
                places.Load();
                il.Math.Negate();
                il.Math.And(0x1f);
                il.Math.Shr();
                il.Convert.ToUInt16();

                var done = il.NewLabel();
                done.Branch(@short: true);

                positivePlaces.Mark();

                number.Load();
                places.Load();
                il.Math.And(0x1f);
                il.Math.Shl();
                il.Convert.ToUInt16();

                done.Mark();

                compiler.EmitStore(store);
            }
        }
    }
}
