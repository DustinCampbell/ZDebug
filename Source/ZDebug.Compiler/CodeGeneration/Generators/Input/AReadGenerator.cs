using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;
using ZDebug.Core.Utilities;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class AReadGenerator : OpcodeGenerator
    {
        private readonly Operand textBufferOp;
        private readonly Operand? parseBufferOp;
        private readonly Variable store;

        public AReadGenerator(Instruction instruction)
            : base(instruction)
        {
            this.textBufferOp = instruction.Operands[0];

            if (instruction.OperandCount > 1)
            {
                this.parseBufferOp = instruction.Operands[1];
            }

            this.store = instruction.StoreVariable;

            if (instruction.OperandCount > 2)
            {
                new ZCompilerException("Timed input not supported");
            }
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            il.Arguments.LoadMachine();
            compiler.EmitLoadOperand(textBufferOp);

            if (parseBufferOp != null)
            {
                compiler.EmitLoadOperand(parseBufferOp.Value);
            }
            else
            {
                il.Load(0);
            }

            using (var result = il.NewLocal<ushort>())
            {
                il.Call(Reflection<CompiledZMachine>.GetMethod("Read_Z5", Types.Array<ushort, ushort>(), @public: false));

                result.Store();
                compiler.EmitStoreVariable(store, result);
            }
        }
    }
}
