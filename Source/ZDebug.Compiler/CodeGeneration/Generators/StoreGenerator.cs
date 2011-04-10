using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class StoreGenerator : OpcodeGenerator
    {
        private readonly Operand op1;
        private readonly Operand op2;

        public StoreGenerator(Operand op1, Operand op2)
            : base(OpcodeGeneratorKind.Store)
        {
            if (op1.Kind == OperandKind.LargeConstant)
            {
                throw new ZCompilerException("Expected variable or small constant operand.");
            }

            if (op1.Value > 255)
            {
                throw new ZCompilerException("Expected operand value from 0-255.");
            }

            this.op1 = op1;
            this.op2 = op2;
        }

        private void GenerateWithCalculatedVariable(byte variableIndex, ILBuilder il, ICompiler compiler)
        {
            using (var calculatedVariableIndex = il.NewLocal<byte>())
            using (var value = il.NewLocal<ushort>())
            {
                compiler.EmitLoadVariable(variableIndex);
                calculatedVariableIndex.Store();

                compiler.EmitOperandLoad(op2);
                value.Store();

                compiler.EmitStoreVariable(calculatedVariableIndex, value, indirect: true);

            }
        }

        private void GenerateWithVariable(byte variableIndex, ILBuilder il, ICompiler compiler)
        {
            using (var value = il.NewLocal<ushort>())
            {
                compiler.EmitOperandLoad(op2);
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

    }
}
