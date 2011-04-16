using System.Reflection.Emit;
using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler
{
    internal partial class ZCompiler
    {
        /// <summary>
        /// Loads a byte from memory at the address retrieved by the <paramref name="addressLoader"/>
        /// and pushes it onto the evaluation stack.
        /// </summary>
        /// <param name="addressLoader">A delegate that pushes the address to be read from to the evaluation stack.
        /// This delegate should not rely upon the state of the stack when invoked.</param>
        private void LoadByte(CodeBuilder addressLoader)
        {
            memory.LoadElement(addressLoader);
        }

        /// <summary>
        /// Loads a byte from memory at the specified address and pushes it onto the evaluation stack.
        /// </summary>
        private void LoadByte(int address)
        {
            LoadByte(
                addressLoader: () => il.Load(address));
        }

        /// <summary>
        /// Loads a byte from memory at the address stored in the given local.
        /// </summary>
        private void LoadByte(ILocal address)
        {
            LoadByte(
                addressLoader: () => address.Load());
        }

        /// <summary>
        /// Loads a byte from memory at the address on top of the evaluation stack.
        /// </summary>
        private void LoadByte()
        {
            using (var address = il.NewLocal<int>())
            {
                address.Store();
                LoadByte(address);
            }
        }

        /// <summary>
        /// Loads a word from memory at the address retrieved by the <paramref name="addressLoader"/>
        /// and pushes it onto the evaluation stack.
        /// </summary>
        /// <param name="addressLoader">A delegate that pushes the address to be read from to the evaluation stack.
        /// This delegate should not rely upon the state of the stack when invoked.</param>
        /// <param name="addressPlusOneLoader">A delegate that pushes the address + 1 to be read from to the evaluation stack.
        /// This delegate should not rely upon the state of the stack when invoked.</param>
        private void LoadWord(CodeBuilder addressLoader, CodeBuilder addressPlusOneLoader)
        {
            // shift memory[address] left 8 bits
            memory.LoadElement(addressLoader);
            il.Math.Shl(8);

            // read memory[address + 1]
            memory.LoadElement(addressPlusOneLoader);

            // or bytes together
            il.Math.Or();
            il.Convert.ToUInt16();
        }

        /// <summary>
        /// Loads a word from memory at the specified address.
        /// </summary>
        private void LoadWord(int address)
        {
            LoadWord(
                addressLoader: () => il.Load(address),
                addressPlusOneLoader: () => il.Load(address + 1));
        }

        /// <summary>
        /// Loads a word from memory at the address stored in the given local.
        /// </summary>
        private void LoadWord(ILocal address)
        {
            LoadWord(
                addressLoader: () => address.Load(),
                addressPlusOneLoader: () =>
                {
                    address.Load();
                    il.Math.Add(1);
                });
        }

        /// <summary>
        /// Loads a word from memory at the address on top of the evaluation stack.
        /// </summary>
        private void LoadWord()
        {
            using (var address = il.NewLocal<int>())
            {
                address.Store();
                LoadWord(address);
            }
        }

        /// <summary>
        /// Stores a word in memory at the address retrieved by the <paramref name="addressLoader"/>.
        /// </summary>
        /// <param name="addressLoader">A delegate that pushes the address to be store to on the evaluation stack.
        /// This delegate should not rely upon the state of the stack when invoked.</param>
        /// <param name="valueLoader">A delegate that pushes the value to be written to the evaluation stack.
        /// This delegate should not rely upon the state of the stack when invoked.</param>
        private void StoreByte(CodeBuilder addressLoader, CodeBuilder valueLoader)
        {
            memory.StoreElement(addressLoader, valueLoader);
        }

        /// <summary>
        /// Stores a byte in memory at the specified address.
        /// </summary>
        private void StoreByte(int address, ushort value)
        {
            StoreByte(
                addressLoader: () => il.Load(address),
                valueLoader: () => il.Load(value));
        }

        /// <summary>
        /// Stores a byte in memory at the specified address.
        /// </summary>
        private void StoreByte(int address, ILocal value)
        {
            StoreByte(
                addressLoader: () => il.Load(address),
                valueLoader: () => value.Load());
        }

        /// <summary>
        /// Stores a byte in memory at the specified address.
        /// </summary>
        private void StoreByte(int address, CodeBuilder valueLoader)
        {
            StoreByte(
                addressLoader: () => il.Load(address),
                valueLoader: valueLoader);
        }

        /// <summary>
        /// Stores a byte in memory at the address stored in the given local.
        /// </summary>
        private void StoreByte(ILocal address, ILocal value)
        {
            StoreByte(
                addressLoader: () => address.Load(),
                valueLoader: () => value.Load());
        }

        /// <summary>
        /// Stores a byte in memory at the address stored in the given local.
        /// </summary>
        private void StoreByte(ILocal address, CodeBuilder valueLoader)
        {
            StoreByte(
                addressLoader: () => address.Load(),
                valueLoader: valueLoader);
        }

        /// <summary>
        /// Stores a byte in memory at the address stored in the given local.
        /// </summary>
        private void StoreByte(ILocal address, ushort value)
        {
            StoreByte(
                addressLoader: () => address.Load(),
                valueLoader: () => il.Load(value));
        }

        /// <summary>
        /// Stores a word in memory at the address retrieved by the <paramref name="addressLoader"/>.
        /// </summary>
        /// <param name="addressLoader">A delegate that pushes the address to be store to on the evaluation stack.
        /// This delegate should not rely upon the state of the stack when invoked.</param>
        /// <param name="addressPlusOneLoader">A delegate that pushes the address to be store to on the evaluation stack.
        /// This delegate should not rely upon the state of the stack when invoked.</param>
        /// <param name="valueLoader">A delegate that pushes the value to be written to the evaluation stack.
        /// This delegate should not rely upon the state of the stack when invoked.</param>
        private void StoreWord(CodeBuilder addressLoader, CodeBuilder addressPlusOneLoader, CodeBuilder valueLoader)
        {
            // memory[address] = (byte)(value >> 8);
            memory.StoreElement(
                addressLoader,
                () =>
                {
                    valueLoader();
                    il.Math.Shr(8);
                    il.Convert.ToUInt8();
                });

            // memory[address + 1] = (byte)(value & 0xff);
            memory.StoreElement(
                addressPlusOneLoader,
                () =>
                {
                    valueLoader();
                    il.Math.And(0xff);
                    il.Convert.ToUInt8();
                });
        }

        private void StoreWord(int address, ushort value)
        {
            StoreWord(
                addressLoader: () => il.Load(address),
                addressPlusOneLoader: () => il.Load(address + 1),
                valueLoader: () => il.Load(value));
        }

        private void StoreWord(int address, ILocal value)
        {
            StoreWord(
                addressLoader: () => il.Load(address),
                addressPlusOneLoader: () => il.Load(address + 1),
                valueLoader: () => value.Load());
        }

        private void StoreWord(int address, CodeBuilder valueLoader)
        {
            StoreWord(
                addressLoader: () => il.Load(address),
                addressPlusOneLoader: () => il.Load(address + 1),
                valueLoader: valueLoader);
        }

        private void StoreWord(ILocal address, ILocal value)
        {
            StoreWord(
                addressLoader: () => address.Load(),
                addressPlusOneLoader: () =>
                {
                    address.Load();
                    il.Math.Add(1);
                },
                valueLoader: () => value.Load());
        }

        private void StoreWord(ILocal address, CodeBuilder valueLoader)
        {
            StoreWord(
                addressLoader: () => address.Load(),
                addressPlusOneLoader: () =>
                {
                    address.Load();
                    il.Math.Add(1);
                },
                valueLoader: valueLoader);
        }

        private void StoreWord(ILocal address, ushort value)
        {
            StoreWord(
                addressLoader: () => address.Load(),
                addressPlusOneLoader: () =>
                {
                    address.Load();
                    il.Math.Add(1);
                },
                valueLoader: () => il.Load(value));
        }

        /// <summary>
        /// Loads a value from the locals array onto the evaluation stack.
        /// </summary>
        private void LoadLocalVariable(int index)
        {
            il.Arguments.LoadLocals();
            il.Load(index);
            il.Emit(OpCodes.Ldelem_U2);
        }

        /// <summary>
        /// Loads a value from the locals array onto the evaluation stack.
        /// </summary>
        private void LoadLocalVariable(ILocal index)
        {
            il.Arguments.LoadLocals();
            index.Load();
            il.Emit(OpCodes.Ldelem_U2);
        }

        /// <summary>
        /// Loads a value from the locals array onto the evaluation stack using the index on the evaluation stack.
        /// </summary>
        private void LoadLocalVariable()
        {
            using (var index = il.NewLocal<int>())
            {
                index.Store();
                LoadLocalVariable(index);
            }
        }

        private void StoreLocalVariable(int index, CodeBuilder valueLoader)
        {
            il.Arguments.LoadLocals();
            il.Load(index);
            valueLoader();
            il.Emit(OpCodes.Stelem_I2);
        }

        private void StoreLocalVariable(int index, ILocal value)
        {
            il.Arguments.LoadLocals();
            il.Load(index);
            value.Load();
            il.Emit(OpCodes.Stelem_I2);
        }

        private void StoreLocalVariable(ILocal index, ILocal value)
        {
            il.Arguments.LoadLocals();
            index.Load();
            value.Load();
            il.Emit(OpCodes.Stelem_I2);
        }

        private void StoreLocalVariable(ILocal value)
        {
            using (var index = il.NewLocal<int>())
            {
                index.Store();
                StoreLocalVariable(index, value);
            }
        }

        private int CalculateGlobalVariableAddress(int index)
        {
            return machine.GlobalVariableTableAddress + (index * 2);
        }

        /// <summary>
        /// Calulates the address of a global variable and loads it on the evaluation stack.
        /// </summary>
        private void LoadGlobalVariableAddress(ILocal index = null)
        {
            if (index != null)
            {
                index.Load();
            }

            il.Math.Multiply(2);
            il.Math.Add(machine.GlobalVariableTableAddress);
        }

        private void LoadGlobalVariable(int index)
        {
            var address = CalculateGlobalVariableAddress(index);
            LoadWord(address);
        }

        private void LoadGlobalVariable(ILocal index)
        {
            LoadGlobalVariableAddress(index);
            LoadWord();
        }

        private void LoadGlobalVariable()
        {
            LoadGlobalVariableAddress();
            LoadWord();
        }

        private void StoreGlobalVariable(int index, ILocal value)
        {
            var address = CalculateGlobalVariableAddress(index);
            StoreWord(address, value);
        }

        private void StoreGlobalVariable(int index, CodeBuilder valueLoader)
        {
            var address = CalculateGlobalVariableAddress(index);
            StoreWord(address, valueLoader);
        }

        private void StoreGlobalVariable(ILocal index, ILocal value)
        {
            LoadGlobalVariableAddress(index);

            using (var address = il.NewLocal<int>())
            {
                address.Store();
                StoreWord(address, value);
            }
        }

        private void StoreGlobalVariable(ILocal value)
        {
            LoadGlobalVariableAddress();

            using (var address = il.NewLocal<int>())
            {
                address.Store();
                StoreWord(address, value);
            }
        }

        private void CalculatedLoadVariable(ILocal variableIndex, bool indirect = false)
        {
            il.DebugIndent();

            var tryLocal = il.NewLabel();
            var tryGlobal = il.NewLabel();
            var done = il.NewLabel();

            // branch if this is not the stack (variableIndex > 0)
            variableIndex.Load();
            tryLocal.BranchIf(Condition.True, @short: true);

            // stack
            if (usesStack)
            {
                EmitPopStack(indirect);
                done.Branch(@short: true);
            }
            else
            {
                il.RuntimeError("Unexpected stack access.");
            }

            // local
            tryLocal.Mark();

            // branch if this is not a local (variableIndex >= 16)
            variableIndex.Load();
            il.Load(16);
            tryGlobal.BranchIf(Condition.AtLeast, @short: true);

            if (routine.Locals.Length > 0)
            {
                variableIndex.Load();
                il.Math.Subtract(1);
                LoadLocalVariable();

                done.Branch(@short: true);
            }
            else
            {
                il.RuntimeError("Unexpected read from local variable {0}.", variableIndex);
            }

            // global
            tryGlobal.Mark();

            if (memory != null)
            {
                variableIndex.Load();
                il.Math.Subtract(16);
                LoadGlobalVariable();
            }
            else
            {
                il.RuntimeError("Unexpected global variable access.");
            }

            done.Mark();

            il.DebugUnindent();

            calculatedLoadVariableCount++;
        }

        private void LoadVariable(byte variableIndex, bool indirect = false)
        {
            if (variableIndex == 0)
            {
                EmitPopStack(indirect);
            }
            else if (variableIndex < 16)
            {
                LoadLocalVariable(variableIndex - 1);
            }
            else
            {
                LoadGlobalVariable(variableIndex - 16);
            }
        }

        private void LoadVariable(Variable variable, bool indirect = false)
        {
            switch (variable.Kind)
            {
                case VariableKind.Stack:
                    EmitPopStack(indirect);
                    break;

                case VariableKind.Local:
                    LoadLocalVariable(variable.Index);
                    break;

                default: // VariableKind.Global
                    LoadGlobalVariable(variable.Index);
                    break;
            }
        }

        private void CalculatedStoreVariable(ILocal variableIndex, ILocal value, bool indirect = false)
        {
            il.DebugIndent();

            var tryLocal = il.NewLabel();
            var tryGlobal = il.NewLabel();
            var done = il.NewLabel();

            // branch if this is not the stack (variableIndex > 0)
            variableIndex.Load();
            tryLocal.BranchIf(Condition.True, @short: true);

            // stack
            if (usesStack)
            {
                EmitPushStack(value, indirect);
                done.Branch(@short: true);
            }
            else
            {
                il.RuntimeError("Unexpected stack access.");
            }

            // local
            tryLocal.Mark();

            // branch if this is not a local (variableIndex >= 16)
            variableIndex.Load();
            il.Load(16);
            tryGlobal.BranchIf(Condition.AtLeast, @short: true);

            if (routine.Locals.Length > 0)
            {
                variableIndex.Load();
                il.Math.Subtract(1);
                StoreLocalVariable(value);

                done.Branch(@short: true);
            }
            else
            {
                il.RuntimeError("Unexpected write to local variable {0}.", variableIndex);
            }

            // global
            tryGlobal.Mark();

            if (memory != null)
            {
                variableIndex.Load();
                il.Math.Subtract(16);
                StoreGlobalVariable(value);
            }
            else
            {
                il.RuntimeError("Unexpected global variable access.");
            }

            done.Mark();

            il.DebugUnindent();

            calculatedStoreVariableCount++;
        }

        private void StoreVariable(byte variableIndex, ILocal value, bool indirect = false)
        {
            if (variableIndex == 0)
            {
                EmitPushStack(value, indirect);
            }
            else if (variableIndex < 16)
            {
                StoreLocalVariable(variableIndex - 1, value);
            }
            else
            {
                StoreGlobalVariable(variableIndex - 16, value);
            }
        }

        private void StoreVariable(Variable variable, CodeBuilder valueLoader)
        {
            switch (variable.Kind)
            {
                case VariableKind.Stack:
                    using (var value = il.NewLocal<ushort>())
                    {
                        valueLoader();
                        value.Store();

                        EmitPushStack(value);
                    }

                    break;

                case VariableKind.Local:
                    StoreLocalVariable(variable.Index, valueLoader);
                    break;

                default: // VariableKind.Global
                    StoreGlobalVariable(variable.Index, valueLoader);
                    break;
            }
        }

        private Operand GetOperand(int operandIndex)
        {
            if (operandIndex < 0 || operandIndex >= OperandCount)
            {
                throw new ZCompilerException(
                    string.Format(
                        "Attempted to read operand {0}, but only 0 through {1} are valid.",
                        operandIndex,
                        OperandCount - 1));
            }

            return this.current.Value.Operands[operandIndex];
        }

        /// <summary>
        /// Loads the specified operand onto the evaluation stack.
        /// </summary>
        private void LoadOperand(int operandIndex)
        {
            var op = GetOperand(operandIndex);

            switch (op.Kind)
            {
                case OperandKind.LargeConstant:
                case OperandKind.SmallConstant:
                    il.Load(op.Value);
                    break;

                default: // OperandKind.Variable
                    LoadVariable((byte)op.Value);
                    break;
            }
        }

        /// <summary>
        /// Loads the first operand as a by ref variable onto the evaluation stack.
        /// </summary>
        private void LoadByRefVariableOperand()
        {
            var op = GetOperand(0);

            switch (op.Kind)
            {
                case OperandKind.SmallConstant:
                    il.Load((byte)op.Value);
                    break;

                case OperandKind.Variable:
                    LoadVariable((byte)op.Value);

                    break;

                default:
                    throw new ZCompilerException("Expected small constant or variable, but was " + op.Kind);
            }
        }

        /// <summary>
        /// Unpacks the byte address on the evaluation stack as a routine address.
        /// </summary>
        private void UnpackRoutineAddress()
        {
            byte version = machine.Version;
            if (version < 4)
            {
                il.Math.Multiply(2);
            }
            else if (version < 8)
            {
                il.Math.Multiply(4);
            }
            else // 8
            {
                il.Math.Multiply(8);
            }

            if (version >= 6 && version <= 7)
            {
                il.Math.Add(machine.RoutinesOffset * 8);
            }
        }

        private void LoadUnpackedRoutineAddress(Operand op)
        {
            switch (op.Kind)
            {
                case OperandKind.LargeConstant:
                case OperandKind.SmallConstant:
                    il.Load(machine.UnpackRoutineAddress(op.Value));
                    break;

                default: // OperandKind.Variable
                    LoadVariable((byte)op.Value);
                    UnpackRoutineAddress();
                    break;
            }
        }

        private void LoadUnpackedStringAddress(Operand op)
        {
            switch (op.Kind)
            {
                case OperandKind.LargeConstant:
                case OperandKind.SmallConstant:
                    il.Load(machine.UnpackStringAddress(op.Value));
                    break;

                default: // OperandKind.Variable
                    LoadVariable((byte)op.Value);

                    byte version = machine.Version;
                    if (version < 4)
                    {
                        il.Math.Multiply(2);
                    }
                    else if (version < 8)
                    {
                        il.Math.Multiply(4);
                    }
                    else // 8
                    {
                        il.Math.Multiply(8);
                    }

                    if (version >= 6 && version <= 7)
                    {
                        il.Math.Add(machine.StringsOffset * 8);
                    }
                    break;
            }
        }
    }
}
