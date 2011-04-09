using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal abstract class UnaryOpGenerator : OpcodeGenerator
    {
        private readonly Operand op;

        public UnaryOpGenerator(OpcodeGeneratorKind kind, Operand op)
            : base(kind)
        {
            if (op.Value > 255)
            {
                throw new ZCompilerException("Expected operand value from 0-255.");
            }

            this.op = op;
        }

        protected abstract void Operation(ILBuilder il);

        protected virtual void PostOperation(ILocal result, ILBuilder il, ICompiler compiler)
        {
        }

        private void GenerateWithCalculatedVariable(byte variableIndex, ILBuilder il, ICompiler compiler)
        {
            using (var calculatedVariableIndex = il.NewLocal<byte>())
            using (var result = il.NewLocal<short>())
            {
                compiler.EmitLocalVariableLoad(variableIndex);
                calculatedVariableIndex.Store();

                compiler.EmitLocalVariableLoad(calculatedVariableIndex, indirect: true);
                il.Convert.ToInt16();

                Operation(il);
                result.Store();

                compiler.EmitLocalVariableStore(calculatedVariableIndex, result, indirect: true);

                PostOperation(result, il, compiler);
            }
        }

        private void GenerateWithVariable(byte variableIndex, ILBuilder il, ICompiler compiler)
        {
            using (var result = il.NewLocal<short>())
            {
                compiler.EmitLocalVariableLoad(variableIndex, indirect: true);
                il.Convert.ToInt16();

                Operation(il);
                result.Store();

                compiler.EmitLocalVariableStore(variableIndex, result, indirect: true);

                PostOperation(result, il, compiler);
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
