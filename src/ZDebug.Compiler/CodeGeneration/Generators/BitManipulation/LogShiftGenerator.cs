using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class LogShiftGenerator : OpcodeGenerator
    {
        private readonly Operand op1;
        private readonly Operand op2;
        private readonly Variable store;

        public LogShiftGenerator(Instruction instruction)
            : base(instruction)
        {
            this.op1 = instruction.Operands[0];
            this.op2 = instruction.Operands[1];
            this.store = instruction.StoreVariable;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            using (var number = il.NewLocal<ushort>())
            using (var places = il.NewLocal<int>())
            {
                if (!ReuseFirstOperand)
                {
                    compiler.EmitLoadOperand(op1);
                }

                number.Store();

                compiler.EmitLoadOperand(op2);
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

                using (var result = il.NewLocal<ushort>())
                {
                    result.Store();
                    compiler.EmitStoreVariable(store, result, reuse: ReuseStoreVariable);
                }
            }
        }

        public override bool CanReuseFirstOperand
        {
            get { return true; }
        }

        public override bool CanReuseStoreVariable
        {
            get { return true; }
        }
    }
}
