using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;
using ZDebug.Core.Utilities;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class ReadCharGenerator : OpcodeGenerator
    {
        private readonly Variable store;

        public ReadCharGenerator(Instruction instruction)
            : base(instruction)
        {
            if (instruction.OperandCount > 0)
            {
                var inputStreamOp = instruction.Operands[0];
                if (inputStreamOp.Kind == OperandKind.Variable)
                {
                    throw new ZCompilerException("op_read_char: Expected a single non-variable operand");
                }

                if (inputStreamOp.Value != 1)
                {
                    throw new ZCompilerException("op_read_char: Expected a single non-variable operand with value 1, but was " + inputStreamOp.Value);
                }
            }
            else
            {
                throw new ZCompilerException("op_read_char: Expected a single non-variable operand");
            }

            this.store = instruction.StoreVariable;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            using (var result = il.NewLocal<ushort>())
            {
                il.Arguments.LoadMachine();
                il.Call(Reflection<CompiledZMachine>.GetMethod("ReadChar", Types.None, @public: false));

                result.Store();

                compiler.EmitStoreVariable(store, result);
            }
        }
    }
}
