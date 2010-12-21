using System;
using ZDebug.Core.Execution;

namespace ZDebug.Core.Instructions
{
    internal static class OpcodeRoutines
    {
        ///////////////////////////////////////////////////////////////////////////////////////////
        // Arithmetic routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public class add : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);
                Strict.HasStoreVariable(i);

                var x = (short)opValues[0];
                var y = (short)opValues[1];

                var result = (ushort)(x + y);

                context.WriteVariable(i.StoreVariable, result);
            }
        }

        public class div : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);
                Strict.HasStoreVariable(i);

                var x = (short)opValues[0];
                var y = (short)opValues[1];

                var result = (ushort)(x / y);

                context.WriteVariable(i.StoreVariable, result);
            }
        }

        public class mod : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);
                Strict.HasStoreVariable(i);

                var x = (short)opValues[0];
                var y = (short)opValues[1];

                var result = (ushort)(x % y);

                context.WriteVariable(i.StoreVariable, result);
            }
        }

        public class mul : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);
                Strict.HasStoreVariable(i);

                var x = (short)opValues[0];
                var y = (short)opValues[1];

                var result = (ushort)(x * y);

                context.WriteVariable(i.StoreVariable, result);
            }
        }

        public class sub : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);
                Strict.HasStoreVariable(i);

                var x = (short)opValues[0];
                var y = (short)opValues[1];

                var result = (ushort)(x - y);

                context.WriteVariable(i.StoreVariable, result);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Bit-level routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public class and : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);
                Strict.HasStoreVariable(i);

                var x = opValues[0];
                var y = opValues[1];

                var result = (ushort)(x & y);

                context.WriteVariable(i.StoreVariable, result);
            }
        }

        public class art_shift : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);
                Strict.HasStoreVariable(i);

                var number = (short)opValues[0];
                var places = (int)(short)opValues[1];

                var result = places > 0
                    ? number << places
                    : number >> -places;

                context.WriteVariable(i.StoreVariable, (ushort)result);
            }
        }

        public class log_shift : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);
                Strict.HasStoreVariable(i);

                var number = opValues[0];
                var places = (int)(short)opValues[1];

                var result = places > 0
                    ? number << places
                    : number >> -places;

                context.WriteVariable(i.StoreVariable, (ushort)result);
            }
        }

        public class not : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);
                Strict.HasStoreVariable(i);

                var x = opValues[0];

                var result = (ushort)(~x);

                context.WriteVariable(i.StoreVariable, result);
            }
        }

        public class or : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);
                Strict.HasStoreVariable(i);

                var x = opValues[0];
                var y = opValues[1];

                var result = (ushort)(x | y);

                context.WriteVariable(i.StoreVariable, result);
            }
        }

        public class test : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);
                Strict.HasBranch(i);

                var bitmap = opValues[0];
                var flags = opValues[1];

                var result = (bitmap & flags) == flags;

                if (result == i.Branch.Condition)
                {
                    context.Jump(i.Branch);
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Increment/decrement routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public class dec : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);

                var varIdx = opValues[0];
                Strict.IsByte(i, varIdx);

                var variable = Variable.FromByte((byte)varIdx);

                var value = (short)context.ReadVariableIndirectly(variable);
                value -= 1;
                context.WriteVariableIndirectly(variable, (ushort)value);
            }
        }

        public class dec_chk : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);
                Strict.HasBranch(i);

                var varIdx = opValues[0];
                Strict.IsByte(i, varIdx);

                var test = (short)opValues[1];

                var variable = Variable.FromByte((byte)varIdx);

                var value = (short)context.ReadVariableIndirectly(variable);
                value -= 1;
                context.WriteVariableIndirectly(variable, (ushort)value);

                if ((value < test) == i.Branch.Condition)
                {
                    context.Jump(i.Branch);
                }
            }
        }

        public class inc : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);

                var varIdx = opValues[0];
                Strict.IsByte(i, varIdx);

                var variable = Variable.FromByte((byte)varIdx);

                var value = (short)context.ReadVariableIndirectly(variable);
                value += 1;
                context.WriteVariableIndirectly(variable, (ushort)value);
            }
        }

        public class inc_chk : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);
                Strict.HasBranch(i);

                var varIdx = opValues[0];
                Strict.IsByte(i, varIdx);

                var test = (short)opValues[1];

                var variable = Variable.FromByte((byte)varIdx);

                var value = (short)context.ReadVariableIndirectly(variable);
                value += 1;
                context.WriteVariableIndirectly(variable, (ushort)value);

                if ((value > test) == i.Branch.Condition)
                {
                    context.Jump(i.Branch);
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Jump routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public class je : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountInRange(i, 2, 4);
                Strict.HasBranch(i);

                var x = opValues[0];

                bool result = false;
                for (int j = 1; j < opCount; j++)
                {
                    if (x == opValues[j])
                    {
                        result = true;
                        break;
                    }
                }

                if (result == i.Branch.Condition)
                {
                    context.Jump(i.Branch);
                }
            }
        }

        public class jg : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);
                Strict.HasBranch(i);

                var x = (short)opValues[0];
                var y = (short)opValues[1];

                if ((x > y) == i.Branch.Condition)
                {
                    context.Jump(i.Branch);
                }
            }
        }

        public class jin : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);
                Strict.HasBranch(i);

                var obj1 = opValues[0];
                var obj2 = opValues[1];

                var obj1Parent = context.GetParent(obj1);

                if ((obj1Parent == obj2) == i.Branch.Condition)
                {
                    context.Jump(i.Branch);
                }
            }
        }

        public class jl : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);
                Strict.HasBranch(i);

                var x = (short)opValues[0];
                var y = (short)opValues[1];

                if ((x < y) == i.Branch.Condition)
                {
                    context.Jump(i.Branch);
                }
            }
        }

        public class jump : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);

                var offset = (short)opValues[0];

                context.Jump(offset);
            }
        }

        public class jz : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);
                Strict.HasBranch(i);

                var x = opValues[0];
                var result = x == 0;

                if (result == i.Branch.Condition)
                {
                    context.Jump(i.Branch);
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Call routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public class call_1n : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);

                var byteAddress = opValues[0];
                var address = context.UnpackRoutineAddress(byteAddress);

                context.Call(address, opValues, opCount);
            }
        }

        public class call_1s : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);
                Strict.HasStoreVariable(i);

                var byteAddress = opValues[0];
                var address = context.UnpackRoutineAddress(byteAddress);

                context.Call(address, opValues, opCount, i.StoreVariable);
            }
        }

        public class call_2n : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);

                var byteAddress = opValues[0];
                var address = context.UnpackRoutineAddress(byteAddress);

                context.Call(address, opValues, opCount);
            }
        }

        public class call_2s : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);
                Strict.HasStoreVariable(i);

                var byteAddress = opValues[0];
                var address = context.UnpackRoutineAddress(byteAddress);

                context.Call(address, opValues, opCount, i.StoreVariable);
            }
        }

        public class call_vn : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountInRange(i, 1, 4);

                var byteAddress = opValues[0];
                var address = context.UnpackRoutineAddress(byteAddress);

                context.Call(address, opValues, opCount);
            }
        }

        public class call_vs : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountInRange(i, 1, 4);
                Strict.HasStoreVariable(i);

                var byteAddress = opValues[0];
                var address = context.UnpackRoutineAddress(byteAddress);

                context.Call(address, opValues, opCount, i.StoreVariable);
            }
        }

        public class call_vn2 : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountInRange(i, 1, 8);

                var byteAddress = opValues[0];
                var address = context.UnpackRoutineAddress(byteAddress);

                context.Call(address, opValues, opCount);
            }
        }

        public class call_vs2 : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountInRange(i, 1, 8);
                Strict.HasStoreVariable(i);

                var byteAddress = opValues[0];
                var address = context.UnpackRoutineAddress(byteAddress);

                context.Call(address, opValues, opCount, i.StoreVariable);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Return routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public class ret : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);

                var value = opValues[0];

                context.Return(value);
            }
        }

        public class ret_popped : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 0);

                var value = context.ReadVariable(Variable.Stack);

                context.Return(value);
            }
        }

        public class rfalse : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 0);

                context.Return(0);
            }
        }

        public class rtrue : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 0);

                context.Return(1);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Load/Store routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public class load : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);
                Strict.HasStoreVariable(i);

                var varIdx = opValues[0];
                Strict.IsByte(i, varIdx);

                var variable = Variable.FromByte((byte)varIdx);

                var value = context.ReadVariableIndirectly(variable);

                context.WriteVariable(i.StoreVariable, value);
            }
        }

        public class loadb : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);
                Strict.HasStoreVariable(i);

                var array = opValues[0];
                var byteIndex = opValues[1];

                var address = array + byteIndex;
                var value = context.ReadByte(address);

                context.WriteVariable(i.StoreVariable, value);
            }
        }

        public class loadw : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);
                Strict.HasStoreVariable(i);

                var array = opValues[0];
                var wordIndex = opValues[1];

                var address = array + (wordIndex * 2);
                var value = context.ReadWord(address);

                context.WriteVariable(i.StoreVariable, value);
            }
        }

        public class store : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);

                var varIdx = opValues[0];
                Strict.IsByte(i, varIdx);

                var variable = Variable.FromByte((byte)varIdx);
                var value = opValues[1];

                context.WriteVariableIndirectly(variable, value);
            }
        }

        public class storeb : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 3);

                var array = opValues[0];
                var byteIndex = opValues[1];
                var value = opValues[2];
                Strict.IsByte(i, value);

                var address = array + byteIndex;

                context.WriteByte(address, (byte)value);
            }
        }

        public class storew : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 3);

                var array = opValues[0];
                var wordIndex = opValues[1];
                var value = opValues[2];

                var address = array + (wordIndex * 2);

                context.WriteWord(address, value);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Table routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public class copy_table : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 3);

                var first = opValues[0];
                var second = opValues[1];
                var size = opValues[2];

                if (second == 0) // zero out first table
                {
                    for (int j = 0; j < size; j++)
                    {
                        context.WriteByte(first + j, 0);
                    }
                }
                else if ((short)size < 0 || first > second) // copy forwards
                {
                    var copySize = size;
                    if ((short)copySize < 0)
                    {
                        copySize = (ushort)(-((short)size));
                    }

                    for (int j = 0; j < copySize; j++)
                    {
                        var value = context.ReadByte(first + j);
                        context.WriteByte(second + j, value);
                    }
                }
                else // copy backwards
                {
                    for (int j = size - 1; j >= 0; j--)
                    {
                        var value = context.ReadByte(first + j);
                        context.WriteByte(second + j, value);
                    }
                }
            }
        }

        public class scan_table : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountInRange(i, 3, 4);
                Strict.HasStoreVariable(i);
                Strict.HasBranch(i);

                var x = opValues[0];
                var table = opValues[1];
                var len = opValues[2];

                var form = 0x82;
                if (opCount > 3)
                {
                    form = opValues[3];
                }

                ushort address = table;

                for (int j = 0; j < len; j++)
                {
                    if ((form & 0x80) != 0)
                    {
                        var value = context.ReadWord(address);
                        if (value == x)
                        {
                            context.WriteVariable(i.StoreVariable, address);

                            if (i.Branch.Condition)
                            {
                                context.Jump(i.Branch);
                            }
                            return;
                        }
                    }
                    else
                    {
                        var value = context.ReadByte(address);
                        if (value == x)
                        {
                            context.WriteVariable(i.StoreVariable, address);

                            if (i.Branch.Condition)
                            {
                                context.Jump(i.Branch);
                            }
                            return;
                        }
                    }

                    address += (ushort)(form & 0x7f);
                }

                context.WriteVariable(i.StoreVariable, 0);

                if (!i.Branch.Condition)
                {
                    context.Jump(i.Branch);
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Stack routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public class pull : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);

                var varIdx = opValues[0];
                Strict.IsByte(i, varIdx);

                var variable = Variable.FromByte((byte)varIdx);
                var value = context.ReadVariable(Variable.Stack);

                context.WriteVariableIndirectly(variable, value);
            }
        }

        public class push : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);

                var value = opValues[0];

                context.WriteVariable(Variable.Stack, value);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Object routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public class clear_attr : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);

                var objNum = opValues[0];
                var attrNum = opValues[1];

                context.ClearAttribute(objNum, attrNum);
            }
        }

        public class get_child : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);
                Strict.HasBranch(i);
                Strict.HasStoreVariable(i);

                var objNum = opValues[0];

                int childNum;
                if (objNum > 0)
                {
                    childNum = context.GetChild(objNum);
                }
                else
                {
                    context.MessageLog.SendWarning(i, "called with object 0");
                    childNum = 0;
                }

                context.WriteVariable(i.StoreVariable, (ushort)childNum);

                if ((childNum > 0) == i.Branch.Condition)
                {
                    context.Jump(i.Branch);
                }
            }
        }

        public class get_next_prop : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);
                Strict.HasStoreVariable(i);

                var objNum = opValues[0];
                var propNum = opValues[1];

                var nextPropNum = context.GetNextProperty(objNum, propNum);

                context.WriteVariable(i.StoreVariable, (ushort)nextPropNum);
            }
        }

        public class get_parent : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);
                Strict.HasStoreVariable(i);

                var objNum = opValues[0];

                int parentNum;
                if (objNum > 0)
                {
                    parentNum = context.GetParent(objNum);
                }
                else
                {
                    context.MessageLog.SendWarning(i, "called with object 0");
                    parentNum = 0;
                }

                context.WriteVariable(i.StoreVariable, (ushort)parentNum);
            }
        }

        public class get_prop : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);
                Strict.HasStoreVariable(i);

                var objNum = opValues[0];
                var propNum = opValues[1];

                var value = context.GetPropertyData(objNum, propNum);

                context.WriteVariable(i.StoreVariable, (ushort)value);
            }
        }

        public class get_prop_addr : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);
                Strict.HasStoreVariable(i);

                var objNum = opValues[0];
                var propNum = opValues[1];

                var propAddress = context.GetPropertyDataAddress(objNum, propNum);

                context.WriteVariable(i.StoreVariable, (ushort)propAddress);
            }
        }

        public class get_prop_len : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);
                Strict.HasStoreVariable(i);

                var dataAddress = opValues[0];

                var propLen = context.GetPropertyDataLength(dataAddress);

                context.WriteVariable(i.StoreVariable, (ushort)propLen);
            }
        }

        public class get_sibling : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);
                Strict.HasBranch(i);
                Strict.HasStoreVariable(i);

                var objNum = opValues[0];

                int siblingNum;
                if (objNum > 0)
                {
                    siblingNum = context.GetSibling(objNum);
                }
                else
                {
                    context.MessageLog.SendWarning(i, "called with object 0");
                    siblingNum = 0;
                }

                context.WriteVariable(i.StoreVariable, (ushort)siblingNum);

                if ((siblingNum > 0) == i.Branch.Condition)
                {
                    context.Jump(i.Branch);
                }
            }
        }

        public class insert_obj : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);

                var objNum = opValues[0];
                var destNum = opValues[1];

                context.MoveTo(objNum, destNum);
            }
        }

        public class put_prop : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 3);

                var objNum = opValues[0];
                var propNum = opValues[1];
                var value = opValues[2];

                context.WriteProperty(objNum, propNum, value);
            }
        }

        public class remove_obj : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);

                var objNum = opValues[0];

                context.RemoveFromParent(objNum);
            }
        }

        public class set_attr : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);

                var objNum = opValues[0];
                var attrNum = opValues[1];

                context.SetAttribute(objNum, attrNum);
            }
        }

        public class test_attr : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);
                Strict.HasBranch(i);

                var objNum = opValues[0];
                var attrNum = opValues[1];

                var result = context.HasAttribute(objNum, attrNum);

                if (result == i.Branch.Condition)
                {
                    context.Jump(i.Branch);
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Output routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public class buffer_mode : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);

                var flag = opValues[0];

                // TODO: What should we do with buffer_mode? Does it have any meaning?
            }
        }

        public class new_line : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 0);

                context.Print('\n');
            }
        }

        public class output_stream : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountInRange(i, 1, 2);

                var number = (short)opValues[0];

                switch (number)
                {
                    case 1:
                        context.SelectScreenStream();
                        break;

                    case 2:
                        context.SelectTranscriptStream();
                        break;

                    case 3:
                        var address = opValues[1];
                        context.SelectMemoryStream(address);
                        break;

                    case -1:
                        context.DeselectScreenStream();
                        break;

                    case -2:
                        context.DeselectTranscriptStream();
                        break;

                    case -3:
                        context.DeselectMemoryStream();
                        break;

                    case 4:
                    case -4:
                        context.MessageLog.SendError(i, "stream 4 is non supported");
                        break;
                }
            }
        }

        public class print : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 0);
                Strict.HasZText(i);

                var ztext = context.ParseZWords(i.ZText);
                context.Print(ztext);
            }
        }

        public class print_addr : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);

                var byteAddress = opValues[0];

                var zwords = context.ReadZWords(byteAddress);
                var ztext = context.ParseZWords(zwords);

                context.Print(ztext);
            }
        }

        public class print_char : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);

                var ch = (char)opValues[0];
                context.Print(ch);
            }
        }

        public class print_num : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);

                var number = (short)opValues[0];
                context.Print(number.ToString());
            }
        }

        public class print_obj : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);

                var objNum = opValues[0];

                var shortName = context.GetShortName(objNum);
                context.Print(shortName);
            }
        }

        public class print_paddr : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);

                var byteAddress = opValues[0];
                var address = context.UnpackStringAddress(byteAddress);

                var zwords = context.ReadZWords(address);
                var ztext = context.ParseZWords(zwords);

                context.Print(ztext);
            }
        }

        public class print_ret : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 0);
                Strict.HasZText(i);

                var ztext = context.ParseZWords(i.ZText);
                context.Print(ztext + "\n");
                context.Return(1);
            }
        }

        public class print_table : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountInRange(i, 2, 4);

                var address = opValues[0];
                var width = opValues[1];
                var height = opCount > 2
                    ? opValues[2]
                    : (ushort)1;
                var skip = opCount > 3
                    ? opValues[3]
                    : (ushort)0;

                var left = context.Screen.GetCursorColumn();

                for (int j = 0; j < height; j++)
                {
                    if (j != 0)
                    {
                        var y = context.Screen.GetCursorLine() + 1;
                        context.Screen.SetCursor(y, left);
                    }

                    for (int k = 0; k < width; k++)
                    {
                        var ch = (char)context.ReadByte(address);
                        address++;
                        context.Screen.Print(ch);
                    }

                    address += skip;
                }

            }
        }

        public class set_color : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);

                var foreground = (ZColor)opValues[0];
                var background = (ZColor)opValues[1];

                if (foreground != 0)
                {
                    context.Screen.SetForegroundColor(foreground);
                }

                if (background != 0)
                {
                    context.Screen.SetBackgroundColor(background);
                }
            }
        }

        public class set_font : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);
                Strict.HasStoreVariable(i);

                var font = (ZFont)opValues[0];

                var oldFont = context.Screen.SetFont(font);
                context.WriteVariable(i.StoreVariable, (ushort)oldFont);
            }
        }

        public class set_text_style : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);

                var textStyle = (ZTextStyle)opValues[0];

                context.Screen.SetTextStyle(textStyle);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Input routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public class read_char : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountInRange(i, 0, 3);
                Strict.HasStoreVariable(i);

                if (opCount > 0)
                {
                    var inputStream = opValues[0];

                    if (inputStream != 1)
                    {
                        context.MessageLog.SendWarning(i, "expected first operand to be 1 but was " + inputStream);
                    }
                }
                else
                {
                    context.MessageLog.SendWarning(i, "expected at least 1 operand.");
                }

                Action<char> callback = ch =>
                {
                    context.WriteVariable(i.StoreVariable, (ushort)ch);
                };

                context.Screen.ReadChar(callback);
            }
        }

        public class aread : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountInRange(i, 1, 4);
                Strict.HasStoreVariable(i);

                int textBuffer = opValues[0];

                int parseBuffer = 0;
                if (opCount > 1)
                {
                    parseBuffer = opValues[1];
                }

                // TODO: Support timed input

                if (opCount > 2)
                {
                    context.MessageLog.SendWarning(i, "timed input was attempted but it is unsupported");
                    var time = opValues[2];
                }

                if (opCount > 3)
                {
                    var routine = opValues[3];
                }

                context.Screen.ShowStatus();

                var maxChars = context.ReadByte(textBuffer);

                context.Screen.ReadCommand(maxChars, s =>
                {
                    var text = s.ToLower();

                    var existingTextCount = context.ReadByte(textBuffer + 1);

                    context.WriteByte(textBuffer + existingTextCount + 1, (byte)text.Length);

                    for (int j = 0; j < text.Length; j++)
                    {
                        context.WriteByte(textBuffer + existingTextCount + 2 + j, (byte)text[j]);
                    }

                    if (parseBuffer > 0)
                    {
                        var tokens = context.TokenizeCommand(text);

                        var maxWords = context.ReadByte(parseBuffer);
                        var parsedWords = Math.Min(maxWords, tokens.Length);

                        context.WriteByte(parseBuffer + 1, (byte)parsedWords);

                        for (int j = 0; j < parsedWords; j++)
                        {
                            var token = tokens[j];

                            ushort address;
                            if (context.TryLookupWord(token.Text, null, out address))
                            {
                                context.WriteWord(parseBuffer + 2 + (j * 4), address);
                            }
                            else
                            {
                                context.WriteWord(parseBuffer + 2 + (j * 4), 0);
                            }

                            context.WriteByte(parseBuffer + 2 + (j * 4) + 2, (byte)token.Length);
                            context.WriteByte(parseBuffer + 2 + (j * 4) + 3, (byte)(token.Start + 2));
                        }
                    }

                    // TODO: Update this when timed input is supported
                    context.WriteVariable(i.StoreVariable, 10);
                });
            }
        }

        public class sread1 : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);

                int textBuffer = opValues[0];
                int parseBuffer = opValues[1];

                context.Screen.ShowStatus();

                var maxChars = context.ReadByte(textBuffer);

                context.Screen.ReadCommand(maxChars, s =>
                {
                    var text = s.ToLower();

                    for (int j = 0; j < text.Length; j++)
                    {
                        context.WriteByte(textBuffer + 1 + j, (byte)text[j]);
                    }

                    context.WriteByte(textBuffer + 1 + text.Length, 0);

                    var tokens = context.TokenizeCommand(text);

                    var maxWords = context.ReadByte(parseBuffer);
                    var parsedWords = Math.Min(maxWords, tokens.Length);

                    context.WriteByte(parseBuffer + 1, (byte)parsedWords);

                    for (int j = 0; j < parsedWords; j++)
                    {
                        var token = tokens[j];

                        ushort address;
                        if (context.TryLookupWord(token.Text, null, out address))
                        {
                            context.WriteWord(parseBuffer + 2 + (j * 4), address);
                        }
                        else
                        {
                            context.WriteWord(parseBuffer + 2 + (j * 4), 0);
                        }

                        context.WriteByte(parseBuffer + 2 + (j * 4) + 2, (byte)token.Length);
                        context.WriteByte(parseBuffer + 2 + (j * 4) + 3, (byte)(token.Start + 1));
                    }
                });
            }
        }

        public class sread2 : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountInRange(i, 2, 4);

                int textBuffer = opValues[0];
                int parseBuffer = opValues[1];

                // TODO: Support timed input

                if (opCount > 2)
                {
                    context.MessageLog.SendWarning(i, "timed input was attempted but it is unsupported");
                }

                // TODO: Do something with time and routine operands if provided.

                context.Screen.ShowStatus();

                var maxChars = context.ReadByte(textBuffer);

                context.Screen.ReadCommand(maxChars, s =>
                {
                    var text = s.ToLower();

                    for (int j = 0; j < text.Length; j++)
                    {
                        context.WriteByte(textBuffer + 1 + j, (byte)text[j]);
                    }

                    context.WriteByte(textBuffer + 1 + text.Length, 0);

                    var tokens = context.TokenizeCommand(text);

                    var maxWords = context.ReadByte(parseBuffer);
                    var parsedWords = Math.Min(maxWords, tokens.Length);

                    context.WriteByte(parseBuffer + 1, (byte)parsedWords);

                    for (int j = 0; j < parsedWords; j++)
                    {
                        var token = tokens[j];

                        ushort address;
                        if (context.TryLookupWord(token.Text, null, out address))
                        {
                            context.WriteWord(parseBuffer + 2 + (j * 4), address);
                        }
                        else
                        {
                            context.WriteWord(parseBuffer + 2 + (j * 4), 0);
                        }

                        context.WriteByte(parseBuffer + 2 + (j * 4) + 2, (byte)token.Length);
                        context.WriteByte(parseBuffer + 2 + (j * 4) + 3, (byte)(token.Start + 1));
                    }
                });
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Window routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public class erase_window : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);

                var window = (short)opValues[0];

                if (window == -1 || window == -2)
                {
                    context.Screen.ClearAll(unsplit: window == -1);
                }
                else
                {
                    context.Screen.Clear(window);
                }
            }
        }

        public class set_cursor : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 2);

                var line = opValues[0];
                var column = opValues[1];

                context.Screen.SetCursor(line - 1, column - 1);
            }
        }

        public class set_window : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);

                var window = opValues[0];

                context.Screen.SetWindow(window);
            }
        }

        public class split_window : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);

                var height = opValues[0];

                if (height > 0)
                {
                    context.Screen.Split(height);
                }
                else
                {
                    context.Screen.Unsplit();
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Miscellaneous routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public class check_arg_count : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);
                Strict.HasBranch(i);

                var argNumber = opValues[0];
                var argCount = context.GetArgumentCount();

                if ((argNumber <= argCount) == i.Branch.Condition)
                {
                    context.Jump(i.Branch);
                }
            }
        }

        public class piracy : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 0);
                Strict.HasBranch(i);

                context.Jump(i.Branch);
            }
        }

        public class quit : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 0);

                context.Quit();
            }
        }

        public class random : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 1);
                Strict.HasStoreVariable(i);

                var range = (short)opValues[0];

                if (range > 0)
                {
                    var result = context.NextRandom(range);
                    context.WriteVariable(i.StoreVariable, (ushort)result);
                }
                else if (range < 0)
                {
                    context.Randomize(+range);
                }
                else // range = 0s
                {
                    context.Randomize((int)DateTime.Now.Ticks);
                }
            }
        }

        public class restore_undo : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 0);
                Strict.HasStoreVariable(i);

                context.MessageLog.SendWarning(i, "Undo is not supported.");

                context.WriteVariable(i.StoreVariable, unchecked((ushort)-1));
            }
        }

        public class save_undo : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 0);
                Strict.HasStoreVariable(i);

                context.MessageLog.SendWarning(i, "Undo is not supported.");

                context.WriteVariable(i.StoreVariable, unchecked((ushort)-1));
            }
        }

        public class show_status : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 0);

                context.Screen.ShowStatus();
            }
        }

        public class tokenize : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountInRange(i, 2, 4);

                ushort textBuffer = opValues[0];
                ushort parseBuffer = opValues[1];

                ushort dictionary = opCount > 2
                    ? opValues[2]
                    : (ushort)0;

                bool flag = opCount > 3
                    ? opValues[3] != 0
                    : false;

                context.TokenizeLine(textBuffer, parseBuffer, dictionary, flag);
            }
        }

        public class verify : OpcodeRoutine
        {
            public override void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context)
            {
                Strict.OperandCountIs(i, 0);
                Strict.HasBranch(i);

                if (context.VerifyChecksum() == i.Branch.Condition)
                {
                    context.Jump(i.Branch);
                }
            }
        }
    }
}
