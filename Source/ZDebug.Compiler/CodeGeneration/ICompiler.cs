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
        /// Emits code to store the top of the evaluation stack in the specified variable.
        /// </summary>
        void EmitStore(Variable variable);

        /// <summary>
        /// Emits code to return from the current routine.
        /// </summary>
        void EmitReturn();

        /// <summary>
        /// Emits code to call a routine.
        /// </summary>
        void EmitCall();

        /// <summary>
        /// Emits code to pop the Z-machine stack.
        /// </summary>
        void EmitPopStack();

        /// <summary>
        /// Emits code to load the Z-machine local variable at the given index.
        /// </summary>
        void EmitLocalVariableLoad(byte variableIndex, bool indirect = false);

        /// <summary>
        /// Emits code to load the Z-machine local variable at the index stored in the given IL local.
        /// </summary>
        void EmitLocalVariableLoad(ILocal variableIndex, bool indirect = false);

        /// <summary>
        /// Emits code to store an IL local to the Z-machine local variable at the given index.
        /// </summary>
        void EmitLocalVariableStore(byte variableIndex, ILocal value, bool indirect = false);

        /// <summary>
        /// Emits code to store an IL local to the Z-machine local variable at the index stored in the given IL local.
        /// </summary>
        void EmitLocalVariableStore(ILocal variableIndex, ILocal value, bool indirect = false);

        /// <summary>
        /// Emits code to load the object parent of the specified operand value onto the evaluation stack.
        /// </summary>
        void EmitObjectParentLoad(Operand operand);
    }
}
