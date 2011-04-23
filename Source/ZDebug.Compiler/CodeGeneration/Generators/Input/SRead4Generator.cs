using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;
using ZDebug.Core.Utilities;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class SRead4Generator : OpcodeGenerator
    {
        private readonly Operand textBufferOp;
        private readonly Operand parseBufferOp;

        public SRead4Generator(Instruction instruction)
            : base(instruction)
        {
            this.textBufferOp = instruction.Operands[0];
            this.parseBufferOp = instruction.Operands[1];

            if (instruction.OperandCount > 2)
            {
                new ZCompilerException("Timed input not supported");
            }
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            il.Arguments.LoadMachine();
            compiler.EmitLoadOperand(textBufferOp);
            compiler.EmitLoadOperand(parseBufferOp);

            il.Call(Reflection<CompiledZMachine>.GetMethod("Read_Z4", Types.Array<ushort, ushort>(), @public: false));
        }
    }
}
