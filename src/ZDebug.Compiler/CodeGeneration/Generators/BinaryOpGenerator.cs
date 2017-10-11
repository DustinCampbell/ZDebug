using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal abstract class BinaryOpGenerator : OpcodeGenerator
    {
        private readonly Operand op1;
        private readonly Operand op2;
        private readonly Variable store;
        private readonly bool signed;

        public BinaryOpGenerator(Instruction instruction, bool signed)
            : base(instruction)
        {
            this.op1 = instruction.Operands[0];
            this.op2 = instruction.Operands[1];
            this.store = instruction.StoreVariable;
            this.signed = signed;
        }

        protected abstract void Operation(ILBuilder il);

        protected virtual void LoadOperands(ILBuilder il, ICompiler compiler, Operand op1, Operand op2)
        {
            if (!ReuseFirstOperand)
            {
                compiler.EmitLoadOperand(op1, convertResult: !signed);
            }

            if (signed)
            {
                il.Convert.ToInt16();
            }

            compiler.EmitLoadOperand(op2, convertResult: !signed);
            if (signed)
            {
                il.Convert.ToInt16();
            }
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            LoadOperands(il, compiler, op1, op2);

            Operation(il);

            if (!LeaveStoreVariableSigned)
            {
                il.Convert.ToUInt16();
            }

            using (var result = il.NewLocal<ushort>())
            {
                result.Store();
                compiler.EmitStoreVariable(store, result, reuse: ReuseStoreVariable);
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

        public override bool SignsOperands
        {
            get { return signed; }
        }

        public override bool CanLeaveStoreVariableSigned
        {
            get { return true; }
        }
    }
}
