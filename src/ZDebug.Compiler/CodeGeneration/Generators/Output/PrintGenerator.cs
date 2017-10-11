using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class PrintGenerator : OpcodeGenerator
    {
        private readonly ushort[] zwords;

        public PrintGenerator(Instruction instruction)
            : base(instruction)
        {
            this.zwords = instruction.ZText;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            compiler.EmitPrintZWords(zwords);
        }
    }
}
