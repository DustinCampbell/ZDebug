using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using ZDebug.Core.Instructions;
using ZDebug.Compiler.Generate;

namespace ZDebug.Compiler
{
    public partial class ZCompiler
    {
        /// <summary>
        /// Reads a byte from memory at the address pushed onto the evaluation stack by the specified code generator.
        /// </summary>
        /// <param name="loadAddress">An Action delegate that pushes the address to be read from to the evaluation stack.
        /// This delegate should not rely upon the state of the stack when it is called.</param>
        private void ReadByte(CodeBuilder loadAddress)
        {
            memory.LoadElement(loadAddress);
        }

        /// <summary>
        /// Reads a byte from memory at the specified address.
        /// </summary>
        private void ReadByte(int address)
        {
            ReadByte(
                loadAddress: il.GenerateLoadConstant(address));
        }

        /// <summary>
        /// Reads a byte from memory at the address stored in the given local.
        /// </summary>
        private void ReadByte(ILocal addressLocal)
        {
            ReadByte(
                loadAddress: il.GenerateLoad(addressLocal));
        }

        /// <summary>
        /// Reads a byte from memory at the address on top of the evaluation stack.
        /// </summary>
        private void ReadByte()
        {
            using (var address = il.NewLocal<int>())
            {
                address.Store();
                ReadByte(address);
            }
        }

        /// <summary>
        /// Reads a word from memory at the address pushed onto the evaluation stack by the specified code generator.
        /// </summary>
        /// <param name="loadAddress">An Action delegate that pushes the address to be read from to the evaluation stack.
        /// This delegate should not rely upon the state of the stack when it is called.</param>
        /// <param name="loadAddressPlusOne">An Action delegate that pushes the address + 1 to be read from to the evaluation stack.
        /// This delegate should not rely upon the state of the stack when it is called.</param>
        private void ReadWord(CodeBuilder loadAddress, CodeBuilder loadAddressPlusOne)
        {
            // shift memory[address] left 8 bits
            memory.LoadElement(loadAddress);
            il.Shl(8);

            // read memory[address + 1]
            memory.LoadElement(loadAddressPlusOne);

            // or bytes together
            il.Or();
            il.ConvertToUInt16();
        }

        /// <summary>
        /// Reads a word from memory at the specified address.
        /// </summary>
        private void ReadWord(int address)
        {
            ReadWord(
                loadAddress: il.GenerateLoadConstant(address),
                loadAddressPlusOne: il.GenerateLoadConstant(address + 1));
        }

        /// <summary>
        /// Reads a word from memory at the address stored in the given local.
        /// </summary>
        private void ReadWord(ILocal address)
        {
            ReadWord(
                loadAddress: il.GenerateLoad(address),
                loadAddressPlusOne:
                    il.Combine(
                        il.GenerateLoad(address),
                        il.GenerateAdd(1)));
        }

        /// <summary>
        /// Reads a word from memory at the address on top of the evaluation stack.
        /// </summary>
        private void ReadWord()
        {
            using (var address = il.NewLocal<int>())
            {
                address.Store();
                ReadWord(address);
            }
        }

        /// <summary>
        /// Writes a word a memory at the address pushed onto the evaluation stack by the specified code generator.
        /// </summary>
        /// <param name="loadAddress">An Action delegate that pushes the address to be written to on the evaluation stack.
        /// This delegate should not rely upon the state of the stack when it is called.</param>
        /// <param name="loadValue">An Action delegate that pushes the value to be written to the evaluation stack.
        /// This delegate should not rely upon the state of the stack when it is called.</param>
        private void WriteByte(CodeBuilder loadAddress, CodeBuilder loadValue)
        {
            memory.StoreElement(loadAddress, loadValue);
        }

        /// <summary>
        /// Writes a byte to memory at the specified address.
        /// </summary>
        private void WriteByte(int address, ILocal value)
        {
            WriteByte(
                loadAddress: il.GenerateLoadConstant(address),
                loadValue: il.GenerateLoad(value));
        }

        /// <summary>
        /// Writes a byte to memory at the address stored in the given local.
        /// </summary>
        private void WriteByte(ILocal address, ILocal value)
        {
            WriteByte(
                loadAddress: il.GenerateLoad(address),
                loadValue: il.GenerateLoad(value));
        }

        /// <summary>
        /// Writes a word a memory at the address pushed onto the evaluation stack by the specified code generator.
        /// </summary>
        /// <param name="loadAddress">An Action delegate that loads the address to be written to onto the evaluation stack.
        /// This delegate should not rely upon the state of the stack when it is called.</param>
        /// <param name="loadAddressPlusOne">An Action delegate that loads the address + 1 to be written to onto the evaluation stack.
        /// This delegate should not rely upon the state of the stack when it is called.</param>
        /// <param name="loadValue">An Action delegate that loads the value to be written to onto the evaluation stack.
        /// This delegate should not rely upon the state of the stack when it is called.</param>
        private void WriteWord(CodeBuilder loadAddress, CodeBuilder loadAddressPlusOne, CodeBuilder loadValue)
        {
            // memory[address] = (byte)(value >> 8);
            memory.StoreElement(
                loadAddress,
                il.Combine(
                    loadValue,
                    il.GenerateShr(8),
                    il.GenerateConvertToUInt8()));

            // memory[address + 1] = (byte)(value & 0xff);
            memory.StoreElement(
                loadAddressPlusOne,
                il.Combine(
                    loadValue,
                    il.GenerateAnd(0xff),
                    il.GenerateConvertToUInt8()));
        }

        private void WriteWord(int address, ILocal value)
        {
            WriteWord(
                loadAddress: il.GenerateLoadConstant(address),
                loadAddressPlusOne: il.GenerateLoadConstant(address + 1),
                loadValue: il.GenerateLoad(value));
        }

        private void WriteWord(ILocal address, ILocal value)
        {
            WriteWord(
                loadAddress: il.GenerateLoad(address),
                loadAddressPlusOne: il.Combine(il.GenerateLoad(address), il.GenerateAdd(1)),
                loadValue: il.GenerateLoad(value));
        }

        private void CheckStackEmpty()
        {
            sp.Load();
            il.LoadConstant(0);
            il.CompareEqual();

            var ok = il.NewLabel();
            ok.BranchIf(Condition.False, @short: true);
            il.RuntimeError("Stack is empty.");

            ok.Mark();
        }

        private void CheckStackFull()
        {
            sp.Load();
            il.LoadConstant(STACK_SIZE);
            il.CompareEqual();

            var ok = il.NewLabel();
            ok.BranchIf(Condition.False, @short: true);
            il.RuntimeError("Stack is full.");

            ok.Mark();
        }

        private void PopStack()
        {
            CheckStackEmpty();

            stack.LoadElement(
                il.Combine(
                    il.GenerateLoad(sp),
                    il.GenerateSubtract(1)));

            // decrement sp
            sp.Load();
            il.Subtract(1);
            sp.Store();
        }

        private void PeekStack()
        {
            CheckStackEmpty();

            stack.LoadElement(
                il.Combine(
                    il.GenerateLoad(sp),
                    il.GenerateSubtract(1)));
        }

        private void PushStack(ILocal value)
        {
            CheckStackFull();

            stack.StoreElement(
                il.GenerateLoad(sp),
                il.GenerateLoad(value));

            // increment sp
            sp.Load();
            il.Add(1);
            sp.Store();
        }

        private void PushStack()
        {
            using (var value = il.NewLocal<ushort>())
            {
                value.Store();
                PushStack(value);
            }
        }

        private void SetStackTop(ILocal value)
        {
            CheckStackEmpty();

            stack.StoreElement(
                il.Combine(
                    il.GenerateLoad(sp),
                    il.GenerateSubtract(1)),
                il.GenerateLoad(value));
        }

        private void SetStackTop()
        {
            using (var value = il.NewLocal<ushort>())
            {
                value.Store();
                SetStackTop(value);
            }
        }

        /// <summary>
        /// Reads a value from the locals array
        /// </summary>
        private void ReadLocalVariable(int index)
        {
            locals.LoadElement(
                il.GenerateLoadConstant(index));
        }

        /// <summary>
        /// Reads a value from the locals array
        /// </summary>
        private void ReadLocalVariable(ILocal index)
        {
            locals.LoadElement(
                il.GenerateLoad(index));
        }

        private void ReadLocalVariable()
        {
            using (var index = il.NewLocal<int>())
            {
                index.Store();
                ReadLocalVariable(index);
            }
        }

        private void WriteLocalVariable(int index, ILocal value)
        {
            locals.StoreElement(
                loadIndex: il.GenerateLoadConstant(index),
                loadValue: il.GenerateLoad(value));
        }

        private void WriteLocalVariable(ILocal index, ILocal value)
        {
            locals.StoreElement(
                loadIndex: il.GenerateLoad(index),
                loadValue: il.GenerateLoad(value));
        }

        private void WriteLocalVariable(ILocal value)
        {
            using (var index = il.NewLocal<int>())
            {
                index.Store();
                WriteLocalVariable(index, value);
            }
        }

        private int CalculateGlobalVariableAddress(int index)
        {
            return machine.GlobalVariableTableAddress + (index * 2);
        }

        private void CalculateGlobalVariableAddress(ILocal index = null)
        {
            if (index != null)
            {
                index.Load();
            }

            il.Multiply(2);
            il.Add(machine.GlobalVariableTableAddress);
        }

        private void ReadGlobalVariable(int index)
        {
            var address = CalculateGlobalVariableAddress(index);
            ReadWord(address);
        }

        private void ReadGlobalVariable(ILocal index)
        {
            CalculateGlobalVariableAddress(index);
            ReadWord();
        }

        private void ReadGlobalVariable()
        {
            CalculateGlobalVariableAddress();
            ReadWord();
        }

        private void WriteGlobalVariable(int index, ILocal value)
        {
            var address = CalculateGlobalVariableAddress(index);
            WriteWord(address, value);
        }

        private void WriteGlobalVariable(ILocal index, ILocal value)
        {
            CalculateGlobalVariableAddress(index);

            using (var address = il.NewLocal<int>())
            {
                address.Store();
                WriteWord(address, value);
            }
        }

        private void WriteGlobalVariable(ILocal value)
        {
            CalculateGlobalVariableAddress();

            using (var address = il.NewLocal<int>())
            {
                address.Store();
                WriteWord(address, value);
            }
        }

        private void ReadVariable(ILocal variableIndex, bool indirect = false)
        {
            il.DebugIndent();

            var tryLocal = il.NewLabel();
            var tryGlobal = il.NewLabel();
            var done = il.NewLabel();

            // branch if this is not the stack (variableIndex > 0)
            variableIndex.Load();
            tryLocal.BranchIf(Condition.True, @short: true);

            // stack

            if (stack != null)
            {
                if (indirect)
                {
                    PeekStack();
                }
                else
                {
                    PopStack();
                }
            }

            done.Branch(@short: true);

            // local
            tryLocal.Mark();

            // branch if this is not a local (variableIndex >= 16)
            variableIndex.Load();
            il.LoadConstant(16);
            tryGlobal.BranchIf(Condition.AtLeast, @short: true);

            variableIndex.Load();
            il.Subtract(1);
            ReadLocalVariable();

            done.Branch(@short: true);

            // global
            tryGlobal.Mark();

            variableIndex.Load();
            il.Subtract(16);
            ReadGlobalVariable();

            done.Mark();

            il.DebugUnindent();
        }

        private void ReadVariable(byte variableIndex, bool indirect = false)
        {
            if (variableIndex == 0)
            {
                if (indirect)
                {
                    PeekStack();
                }
                else
                {
                    PopStack();
                }
            }
            else if (variableIndex < 16)
            {
                ReadLocalVariable(variableIndex - 1);
            }
            else
            {
                ReadGlobalVariable(variableIndex - 16);
            }
        }

        private void ReadVariable(Variable variable, bool indirect = false)
        {
            switch (variable.Kind)
            {
                case VariableKind.Stack:
                    if (indirect)
                    {
                        PeekStack();
                    }
                    else
                    {
                        PopStack();
                    }
                    break;

                case VariableKind.Local:
                    ReadLocalVariable(variable.Index);
                    break;

                default: // VariableKind.Global
                    ReadGlobalVariable(variable.Index);
                    break;
            }
        }

        private void WriteVariable(ILocal variableIndex, ILocal value, bool indirect = false)
        {
            il.DebugIndent();

            var tryLocal = il.NewLabel();
            var tryGlobal = il.NewLabel();
            var done = il.NewLabel();

            // branch if this is not the stack (variableIndex > 0)
            variableIndex.Load();
            tryLocal.BranchIf(Condition.True, @short: true);

            // stack

            if (stack != null)
            {
                if (indirect)
                {
                    SetStackTop(value);
                }
                else
                {
                    PushStack(value);
                }
            }

            done.Branch(@short: true);

            // local
            tryLocal.Mark();

            // branch if this is not a local (variableIndex >= 16)
            variableIndex.Load();
            il.LoadConstant(16);
            tryGlobal.BranchIf(Condition.AtLeast, @short: true);

            variableIndex.Load();
            il.Subtract(1);
            WriteLocalVariable(value);

            done.Branch(@short: true);

            // global
            tryGlobal.Mark();

            variableIndex.Load();
            il.Subtract(16);
            WriteGlobalVariable(value);

            done.Mark();

            il.DebugUnindent();
        }

        private void WriteVariable(byte variableIndex, ILocal value, bool indirect = false)
        {
            if (variableIndex == 0)
            {
                if (indirect)
                {
                    SetStackTop(value);
                }
                else
                {
                    PushStack(value);
                }
            }
            else if (variableIndex < 16)
            {
                WriteLocalVariable(variableIndex - 1, value);
            }
            else
            {
                WriteGlobalVariable(variableIndex - 16, value);
            }
        }

        private void WriteVariable(Variable variable, ILocal value, bool indirect = false)
        {
            switch (variable.Kind)
            {
                case VariableKind.Stack:
                    if (indirect)
                    {
                        SetStackTop(value);
                    }
                    else
                    {
                        PushStack(value);
                    }
                    break;

                case VariableKind.Local:
                    WriteLocalVariable(variable.Index, value);
                    break;

                default: // VariableKind.Global
                    WriteGlobalVariable(variable.Index, value);
                    break;
            }
        }

        private Operand GetOperand(int operandIndex)
        {
            if (operandIndex < 0 || operandIndex >= currentInstruction.OperandCount)
            {
                throw new ZCompilerException(
                    string.Format(
                        "Attempted to read operand {0}, but only 0 through {1} are valid.",
                        operandIndex,
                        currentInstruction.OperandCount - 1));
            }

            return currentInstruction.Operands[operandIndex];
        }

        /// <summary>
        /// Reads the specified operand and places the value on the stack.
        /// </summary>
        private void ReadOperand(int operandIndex)
        {
            var op = GetOperand(operandIndex);

            switch (op.Kind)
            {
                case OperandKind.LargeConstant:
                case OperandKind.SmallConstant:
                    il.LoadConstant(op.Value);
                    break;

                default: // OperandKind.Variable
                    ReadVariable((byte)op.Value);
                    break;
            }
        }

        /// <summary>
        /// Reads the specified operand as a small constant and places the value on the stack.
        /// </summary>
        private byte ReadSmallConstant(int operandIndex)
        {
            var op = GetOperand(operandIndex);

            if (op.Kind != OperandKind.SmallConstant)
            {
                throw new ZCompilerException(
                    string.Format(
                        "Expected a small constnat operand but found a {0}.",
                        op.Kind));
            }

            return (byte)op.Value;
        }

        /// <summary>
        /// Reads the first operand as a by ref variable.
        /// </summary>
        private Variable ReadByRefVariableOperand()
        {
            byte variableIndex = ReadSmallConstant(0);
            return Variable.FromByte(variableIndex);
        }

        private void UnpackRoutineAddress(Operand op)
        {
            switch (op.Kind)
            {
                case OperandKind.LargeConstant:
                case OperandKind.SmallConstant:
                    il.LoadConstant(machine.UnpackRoutineAddress(op.Value));
                    break;

                default: // OperandKind.Variable
                    ReadVariable((byte)op.Value);

                    byte version = machine.Version;
                    if (version < 4)
                    {
                        il.LoadConstant(2);
                    }
                    else if (version < 8)
                    {
                        il.LoadConstant(4);
                    }
                    else // 8
                    {
                        il.LoadConstant(8);
                    }

                    il.Multiply();

                    if (version >= 6 && version <= 7)
                    {
                        il.Add(machine.RoutinesOffset * 8);
                    }

                    break;
            }
        }

        private void UnpackStringAddress(Operand op)
        {
            switch (op.Kind)
            {
                case OperandKind.LargeConstant:
                case OperandKind.SmallConstant:
                    il.LoadConstant(machine.UnpackStringAddress(op.Value));
                    break;

                default: // OperandKind.Variable
                    ReadVariable((byte)op.Value);

                    byte version = machine.Version;
                    if (version < 4)
                    {
                        il.LoadConstant(2);
                    }
                    else if (version < 8)
                    {
                        il.LoadConstant(4);
                    }
                    else // 8
                    {
                        il.LoadConstant(8);
                    }

                    il.Multiply();

                    if (version >= 6 && version <= 7)
                    {
                        il.Add(machine.StringsOffset * 8);
                    }
                    break;
            }
        }
    }
}
