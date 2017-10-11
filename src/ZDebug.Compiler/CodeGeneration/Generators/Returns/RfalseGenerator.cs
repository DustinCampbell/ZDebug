using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class RfalseGenerator : OpcodeGenerator
    {
        public RfalseGenerator(Instruction instruction)
            : base(instruction)
        {
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            il.Load(0);
            compiler.EmitReturn();
        }
    }
}
