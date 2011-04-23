using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;
using ZDebug.Core.Utilities;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class TokenizeGenerator : OpcodeGenerator
    {
        private readonly Operand textBufferOp;
        private readonly Operand parseBufferOp;
        private readonly Operand? dictionaryOp;
        private readonly Operand? flagOp;

        public TokenizeGenerator(Instruction instruction)
            : base(instruction)
        {
            this.textBufferOp = instruction.Operands[0];
            this.parseBufferOp = instruction.Operands[1];

            if (instruction.OperandCount > 2)
            {
                this.dictionaryOp = instruction.Operands[2];
            }

            if (instruction.OperandCount > 3)
            {
                this.flagOp = instruction.Operands[3];
            }
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            il.Arguments.LoadMachine();

            compiler.EmitLoadOperand(textBufferOp);
            compiler.EmitLoadOperand(parseBufferOp);

            if (dictionaryOp != null)
            {
                compiler.EmitLoadOperand(dictionaryOp.Value);
            }
            else
            {
                il.Load(0);
            }

            if (flagOp != null)
            {
                compiler.EmitLoadOperand(flagOp.Value);

                il.Load(0);
                il.Compare.Equal();
                il.Load(0);
                il.Compare.Equal();
            }
            else
            {
                il.Load(0);
            }

            il.Call(Reflection<CompiledZMachine>.GetMethod("Tokenize", Types.Array<ushort, ushort, ushort, bool>(), @public: false));
        }
    }
}
