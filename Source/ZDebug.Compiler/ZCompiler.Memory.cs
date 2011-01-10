using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler
{
    public partial class ZCompiler
    {
        private void ReadByte(int address)
        {
            il.Emit(OpCodes.Ldloc, memory);
            il.Emit(OpCodes.Ldc_I4, address);
            il.Emit(OpCodes.Ldelem_U1);
        }

        private void ReadByte(LocalBuilder address)
        {
            il.Emit(OpCodes.Ldloc, memory);
            il.Emit(OpCodes.Ldloc, address);
            il.Emit(OpCodes.Ldelem_U1);
        }
        private void ReadWord(int address)
        {
            // shift memory[address] left 8 bits
            il.Emit(OpCodes.Ldloc, memory);
            il.Emit(OpCodes.Ldc_I4, address);
            il.Emit(OpCodes.Ldelem_U1);
            il.Emit(OpCodes.Ldc_I4_8);
            il.Emit(OpCodes.Shl);

            // read memory[address + 1]
            il.Emit(OpCodes.Ldloc, memory);
            il.Emit(OpCodes.Ldc_I4, address + 1);
            il.Emit(OpCodes.Ldelem_U1);

            // or bytes together
            il.Emit(OpCodes.Or);
            il.Emit(OpCodes.Conv_U2);
        }

        private void ReadWord(LocalBuilder address)
        {
            // shift memory[address] left 8 bits
            il.Emit(OpCodes.Ldloc, memory);
            il.Emit(OpCodes.Ldloc, address);
            il.Emit(OpCodes.Ldelem_U1);
            il.Emit(OpCodes.Ldc_I4_8);
            il.Emit(OpCodes.Shl);

            // read memory[address + 1]
            il.Emit(OpCodes.Ldloc, memory);
            il.Emit(OpCodes.Ldloc, address);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldelem_U1);

            // or bytes together
            il.Emit(OpCodes.Or);
            il.Emit(OpCodes.Conv_U2);
        }

        private void WriteByte(LocalBuilder address, int value)
        {
            il.Emit(OpCodes.Ldloc, memory);
            il.Emit(OpCodes.Ldc_I4, address);
            il.Emit(OpCodes.Ldloc, value);
            il.Emit(OpCodes.Stelem_I1);
        }

        private void WriteByte(LocalBuilder address, LocalBuilder value)
        {
            il.Emit(OpCodes.Ldloc, memory);
            il.Emit(OpCodes.Ldloc, address);
            il.Emit(OpCodes.Ldloc, value);
            il.Emit(OpCodes.Stelem_I1);
        }

        private void WriteWord(int address, LocalBuilder value)
        {
            // memory[address] = (byte)(value >> 8);
            il.Emit(OpCodes.Ldloc, memory);
            il.Emit(OpCodes.Ldc_I4, address);
            il.Emit(OpCodes.Ldloc, value);
            il.Emit(OpCodes.Ldc_I4_8);
            il.Emit(OpCodes.Shr);
            il.Emit(OpCodes.Conv_U1);
            il.Emit(OpCodes.Stelem_I1);

            // memory[address + 1] = (byte)(value & 0xff);
            il.Emit(OpCodes.Ldloc, memory);
            il.Emit(OpCodes.Ldc_I4, address + 1);
            il.Emit(OpCodes.Ldloc, value);
            il.Emit(OpCodes.Ldc_I4, 0xff);
            il.Emit(OpCodes.And);
            il.Emit(OpCodes.Conv_U1);
            il.Emit(OpCodes.Stelem_I1);
        }

        private void WriteWord(LocalBuilder address, LocalBuilder value)
        {
            // memory[address] = (byte)(value >> 8);
            il.Emit(OpCodes.Ldloc, memory);
            il.Emit(OpCodes.Ldloc, address);
            il.Emit(OpCodes.Ldloc, value);
            il.Emit(OpCodes.Ldc_I4_8);
            il.Emit(OpCodes.Shr);
            il.Emit(OpCodes.Conv_U1);
            il.Emit(OpCodes.Stelem_I1);

            // memory[address + 1] = (byte)(value & 0xff);
            il.Emit(OpCodes.Ldloc, memory);
            il.Emit(OpCodes.Ldloc, address);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldloc, value);
            il.Emit(OpCodes.Ldc_I4, 0xff);
            il.Emit(OpCodes.And);
            il.Emit(OpCodes.Conv_U1);
            il.Emit(OpCodes.Stelem_I1);
        }

        private void CheckStackEmpty()
        {
            il.Emit(OpCodes.Ldloc, sp);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ceq);

            var ok = il.DefineLabel();
            il.Emit(OpCodes.Brfalse_S, ok);
            il.ThrowException("Stack is empty.");

            il.MarkLabel(ok);
        }

        private void CheckStackFull()
        {
            il.Emit(OpCodes.Ldloc, sp);
            il.Emit(OpCodes.Ldc_I4, STACK_SIZE);
            il.Emit(OpCodes.Ceq);

            var ok = il.DefineLabel();
            il.Emit(OpCodes.Brfalse_S, ok);
            il.ThrowException("Stack is full.");

            il.MarkLabel(ok);
        }

        private void PopStack()
        {
            il.CheckStackEmpty(sp);

            il.Emit(OpCodes.Ldloc, stack);

            // decrement sp
            il.Emit(OpCodes.Ldloc, sp);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Sub);
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Stloc, sp);

            il.Emit(OpCodes.Ldelem_U2);
        }

        private void PeekStack()
        {
            il.CheckStackEmpty(sp);

            il.Emit(OpCodes.Ldloc, stack);
            il.Emit(OpCodes.Ldloc, sp);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Sub);
            il.Emit(OpCodes.Ldelem_U2);
        }

        private void PushStack(LocalBuilder value)
        {
            il.CheckStackFull(sp);

            // store value in stack
            il.Emit(OpCodes.Ldloc, stack);
            il.Emit(OpCodes.Ldloc, sp);
            il.Emit(OpCodes.Ldloc, value);
            il.Emit(OpCodes.Stelem_I2);

            // increment sp
            il.Emit(OpCodes.Ldloc, sp);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stloc, sp);
        }

        private void SetStackTop(LocalBuilder value)
        {
            il.CheckStackEmpty(sp);

            il.Emit(OpCodes.Ldloc, stack);
            il.Emit(OpCodes.Ldloc, sp);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Sub);
            il.Emit(OpCodes.Ldloc, value);
            il.Emit(OpCodes.Stelem_I2);
        }

        private void ReadLocalVariable(int index)
        {
            il.Emit(OpCodes.Ldloc, locals);
            il.Emit(OpCodes.Ldc_I4, index);
            il.Emit(OpCodes.Ldelem_U2);
        }

        private void WriteLocalVariable(int index, LocalBuilder value)
        {
            il.Emit(OpCodes.Ldloc, locals);
            il.Emit(OpCodes.Ldc_I4, index);
            il.Emit(OpCodes.Ldloc, value);
            il.Emit(OpCodes.Stelem_I2);
        }

        private int CalculateGlobalVariableAddress(int index)
        {
            return machine.GlobalVariableTableAddress + (index * 2);
        }

        private void ReadGlobalVariable(int index)
        {
            var address = CalculateGlobalVariableAddress(index);
            ReadWord(address);
        }

        private void WriteGlobalVariable(int index, LocalBuilder value)
        {
            var address = CalculateGlobalVariableAddress(index);
            WriteWord(address, value);
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

        private void WriteVariable(byte variableIndex, LocalBuilder value, bool indirect = false)
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

        private void WriteVariable(Variable variable, LocalBuilder value, bool indirect = false)
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

        private void ReadOperand(Operand op)
        {
            switch (op.Kind)
            {
                case OperandKind.LargeConstant:
                case OperandKind.SmallConstant:
                    il.Emit(OpCodes.Ldc_I4, op.Value);
                    break;

                default: // OperandKind.Variable
                    ReadVariable((byte)op.Value);
                    break;
            }
        }
    }
}
