using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration
{
    internal interface ICompiler
    {
        void LoadOperand(Operand operand);

        void Branch();
    }
}
