using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class OutputStreamGenerator : OpcodeGenerator
    {
        private readonly Operand streamOp;
        private readonly Operand? addressOp;

        public OutputStreamGenerator(Instruction instruction)
            : base(instruction)
        {
            this.streamOp = instruction.Operands[0];

            if (streamOp.Kind == OperandKind.Variable)
            {
                throw new ZCompilerException("Expected non-variable operand.");
            }

            if (instruction.OperandCount > 1)
            {
                this.addressOp = instruction.Operands[1];
            }
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            switch ((short)streamOp.Value)
            {
                case 1:
                    compiler.EmitSelectScreenStream();
                    break;
                case 2:
                    compiler.EmitSelectTranscriptStream();
                    break;
                case 3:
                    compiler.EmitSelectMemoryStream(addressOp.Value);
                    break;
                case -1:
                    compiler.EmitDeselectScreenStream();
                    break;
                case -2:
                    compiler.EmitDeselectTranscriptStream();
                    break;
                case -3:
                    compiler.EmitDeselectMemoryStream();
                    break;
                case 4:
                case -4:
                    throw new ZCompilerException("Stream 4 is not supported.");
                default:
                    throw new ZCompilerException(string.Format("Illegal stream value: {0}", streamOp.Value));
            }
        }
    }
}
