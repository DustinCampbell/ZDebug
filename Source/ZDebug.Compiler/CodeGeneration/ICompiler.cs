using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration
{
    internal interface ICompiler
    {
        /// <summary>
        /// Retrieves a label for the given Z-code address.
        /// </summary>
        ILabel GetLabel(int address);

        /// <summary>
        /// Emits code to loads an operand onto the evaluation stack.
        /// </summary>
        void EmitOperandLoad(Operand operand);

        /// <summary>
        /// Emits code to execute the given branch using the value on top of the evaluation stack.
        /// </summary>
        void EmitBranch(Branch branch);

        /// <summary>
        /// Emits code to load the object parent of the specified operand value onto the evaluation stack.
        /// </summary>
        void EmitObjectParentLoad(Operand operand);
    }
}
