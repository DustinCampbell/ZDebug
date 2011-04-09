using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class LoadGenerator : OpcodeGenerator
    {
        private readonly Operand op;
        private readonly Variable store;

        public LoadGenerator(Operand op, Variable store)
            : base(OpcodeGeneratorKind.Load)
        {
            if (op.Kind == OperandKind.LargeConstant)
            {
                throw new ZCompilerException("Expected variable or small constant operand.");
            }

            if (op.Value > 255)
            {
                throw new ZCompilerException("Expected operand value from 0-255.");
            }

            this.op = op;
            this.store = store;
        }

        private void GenerateWithCalculatedVariable(byte variableIndex, ILBuilder il, ICompiler compiler)
        {
            using (var calculatedVariableIndex = il.NewLocal<byte>())
            {
                compiler.EmitLocalVariableLoad(variableIndex);
                calculatedVariableIndex.Store();

                compiler.EmitLocalVariableLoad(calculatedVariableIndex, indirect: true);

                compiler.EmitStore(store);
            }
        }

        private void GenerateWithVariable(byte variableIndex, ILBuilder il, ICompiler compiler)
        {
            compiler.EmitLocalVariableLoad(variableIndex, indirect: true);
            compiler.EmitStore(store);
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
