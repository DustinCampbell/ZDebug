using ZDebug.Compiler.Generate;
using ZDebug.Core.Collections;
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
        void EmitLoadOperand(Operand operand);

        /// <summary>
        /// Emits code to execute the given branch using the value on top of the evaluation stack.
        /// </summary>
        void EmitBranch(Branch branch);

        /// <summary>
        /// Emits code to return from the current routine.
        /// </summary>
        void EmitReturn();

        /// <summary>
        /// Emits code to call a routine.
        /// </summary>
        void EmitCall(Operand address, ReadOnlyArray<Operand> args);

        /// <summary>
        /// Emits code to load a byte from Z-machine memory at the given address.
        /// </summary>
        void EmitLoadMemoryByte(int address);

        /// <summary>
        /// Emits code to load a byte from Z-machine memory at the address stored in the given IL local.
        /// </summary>
        void EmitLoadMemoryByte(ILocal address);

        /// <summary>
        /// Emits code to load a word from Z-machine memory at the given address.
        /// </summary>
        void EmitLoadMemoryWord(int address);

        /// <summary>
        /// Emits code to load a word from Z-machine memory at the address stored in the given IL local.
        /// </summary>
        void EmitLoadMemoryWord(ILocal address);

        /// <summary>
        /// Emits code to store an IL local as a byte in Z-machine memory at the given address.
        /// </summary>
        void EmitStoreMemoryByte(int address, ILocal value);

        /// <summary>
        /// Emits code to store an IL local as a byte in Z-machine memory at the address stored in the given IL local.
        /// </summary>
        void EmitStoreMemoryByte(ILocal address, ILocal value);

        /// <summary>
        /// Emits code to store an IL local as a word in Z-machine memory at the given address.
        /// </summary>
        void EmitStoreMemoryWord(int address, ILocal value);

        /// <summary>
        /// Emits code to store an IL local as a word in Z-machine memory at the address stored in the given IL local.
        /// </summary>
        void EmitStoreMemoryWord(ILocal address, ILocal value);

        /// <summary>
        /// Emits code to pop the Z-machine stack.
        /// </summary>
        void EmitPopStack(bool indirect = false);

        /// <summary>
        /// Emits code to push an IL local onto the Z-machine stack.
        /// </summary>
        void EmitPushStack(ILocal value, bool indirect = false);

        /// <summary>
        /// Emits code to load the Z-machine variable at the given index.
        /// </summary>
        void EmitLoadVariable(byte variableIndex, bool indirect = false);

        /// <summary>
        /// Emits code to load the Z-machine variable at the index stored in the given IL local.
        /// </summary>
        void EmitLoadVariable(ILocal variableIndex, bool indirect = false);

        /// <summary>
        /// Emits code to store an IL local to the Z-machine variable at the given index.
        /// </summary>
        void EmitStoreVariable(byte variableIndex, ILocal value, bool indirect = false);

        /// <summary>
        /// Emits code to store an IL local to the specified Z-machine variable.
        /// </summary>
        void EmitStoreVariable(Variable variable, ILocal value, bool indirect = false);

        /// <summary>
        /// Emits code to store an IL local to the Z-machine variable at the index stored in the given IL local.
        /// </summary>
        void EmitStoreVariable(ILocal variableIndex, ILocal value, bool indirect = false);

        /// <summary>
        /// Emits code to load the unpacked string address in the specified operand onto the evaluation stack.
        /// </summary>
        void EmitLoadUnpackedStringAddress(Operand operand);

        /// <summary>
        /// Emits code to load the object parent of the specified operand value onto the evaluation stack.
        /// </summary>
        void EmitLoadObjectParent(Operand operand);

        /// <summary>
        /// Emits code to print the specified Z-Words to the screen.
        /// </summary>
        void EmitPrintZWords(ushort[] zwords);

        /// <summary>
        /// Emits code to print the text on the evaluation stack to the screen.
        /// </summary>
        void EmitPrintText();

        /// <summary>
        /// Returns the Z-Machine version.
        /// </summary>
        byte Version { get; }
    }
}
