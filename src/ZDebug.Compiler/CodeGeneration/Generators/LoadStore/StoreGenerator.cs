using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class StoreGenerator : OpcodeGenerator
    {
        private readonly Operand op1;
        private readonly Operand op2;

        public StoreGenerator(Instruction instruction)
            : base(instruction)
        {
            this.op1 = instruction.Operands[0];
            this.op2 = instruction.Operands[1];

            if (op1.Kind == OperandKind.LargeConstant)
            {
                throw new ZCompilerException("Expected variable or small constant operand.");
            }

            if (op1.Value > 255)
            {
                throw new ZCompilerException("Expected operand value from 0-255.");
            }
        }

        private void GenerateWithCalculatedVariable(byte variableIndex, ILBuilder il, ICompiler compiler)
        {
            using (var calculatedVariableIndex = il.NewLocal<byte>())
            using (var value = il.NewLocal<ushort>())
            {
                compiler.EmitLoadVariable(variableIndex);
                calculatedVariableIndex.Store();

                compiler.EmitLoadOperand(op2);
                value.Store();

                compiler.EmitStoreVariable(calculatedVariableIndex, value, indirect: true);
            }
        }

        private void GenerateWithVariable(byte variableIndex, ILBuilder il, ICompiler compiler)
        {
            using (var value = il.NewLocal<ushort>())
            {
                compiler.EmitLoadOperand(op2);

                if (ReuseByRefOperand)
                {
                    il.Duplicate();
                }

                value.Store();

                compiler.EmitStoreVariable(variableIndex, value, indirect: true);
            }
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            if (op1.IsVariable)
            {
                GenerateWithCalculatedVariable((byte)op1.Value, il, compiler);
            }
            else
            {
                GenerateWithVariable((byte)op1.Value, il, compiler);
            }
        }

        public override bool CanReuseByRefOperand
        {
            get { return op1.IsConstant; }
        }
    }
}
