using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class PrintRetGenerator : OpcodeGenerator
    {
        private readonly ushort[] zwords;

        public PrintRetGenerator(Instruction instruction)
            : base(instruction)
        {
            this.zwords = instruction.ZText;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            compiler.EmitPrintZWords(zwords);
            il.Load(1);
            compiler.EmitReturn();
        }
    }
}
