using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class PullGenerator : OpcodeGenerator
    {
        private readonly Operand op;

        public PullGenerator(Instruction instruction)
            : base(instruction)
        {
            this.op = instruction.Operands[0];

            if (op.Kind == OperandKind.LargeConstant)
            {
                throw new ZCompilerException("Expected variable or small constant operand.");
            }

            if (op.Value > 255)
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

                compiler.EmitPopStack();
                value.Store();

                compiler.EmitStoreVariable(calculatedVariableIndex, value, indirect: true);
            }
        }

        private void GenerateWithVariable(byte variableIndex, ILBuilder il, ICompiler compiler)
        {
            using (var value = il.NewLocal<short>())
            {
                compiler.EmitPopStack();
                value.Store();

                compiler.EmitStoreVariable(variableIndex, value, indirect: true);
            }
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            if (op.IsVariable)
            {
                GenerateWithCalculatedVariable((byte)op.Value, il, compiler);
            }
            else
            {
                GenerateWithVariable((byte)op.Value, il, compiler);
            }
        }
    }
}
