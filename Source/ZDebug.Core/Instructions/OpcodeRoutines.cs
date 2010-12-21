using System;
using ZDebug.Core.Execution;

namespace ZDebug.Core.Instructions
{
    internal static class OpcodeRoutines
    {
        ///////////////////////////////////////////////////////////////////////////////////////////
        // Arithmetic routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine add = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var x = (short)opValues[0];
            var y = (short)opValues[1];

            var result = (ushort)(x + y);

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine div = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var x = (short)opValues[0];
            var y = (short)opValues[1];

            var result = (ushort)(x / y);

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine mod = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var x = (short)opValues[0];
            var y = (short)opValues[1];

            var result = (ushort)(x % y);

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine mul = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var x = (short)opValues[0];
            var y = (short)opValues[1];

            var result = (ushort)(x * y);

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine sub = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var x = (short)opValues[0];
            var y = (short)opValues[1];

            var result = (ushort)(x - y);

            context.WriteVariable(i.StoreVariable, result);
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Bit-level routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine and = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var x = opValues[0];
            var y = opValues[1];

            var result = (ushort)(x & y);

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine art_shift = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var number = (short)opValues[0];
            var places = (int)(short)opValues[1];

            var result = places > 0
                ? number << places
                : number >> -places;

            context.WriteVariable(i.StoreVariable, (ushort)result);
        };

        public static readonly OpcodeRoutine log_shift = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var number = opValues[0];
            var places = (int)(short)opValues[1];

            var result = places > 0
                ? number << places
                : number >> -places;

            context.WriteVariable(i.StoreVariable, (ushort)result);
        };

        public static readonly OpcodeRoutine not = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 1);
            Strict.HasStoreVariable(i);

            var x = opValues[0];

            var result = (ushort)(~x);

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine or = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var x = opValues[0];
            var y = opValues[1];

            var result = (ushort)(x | y);

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine test = (i, opValues, opCount, context) =>
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
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Increment/decrement routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine dec = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var varIdx = opValues[0];
            Strict.IsByte(i, varIdx);

            var variable = Variable.FromByte((byte)varIdx);

            var value = (short)context.ReadVariableIndirectly(variable);
            value -= 1;
            context.WriteVariableIndirectly(variable, (ushort)value);
        };

        public static readonly OpcodeRoutine dec_chk = (i, opValues, opCount, context) =>
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
        };

        public static readonly OpcodeRoutine inc = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var varIdx = opValues[0];
            Strict.IsByte(i, varIdx);

            var variable = Variable.FromByte((byte)varIdx);

            var value = (short)context.ReadVariableIndirectly(variable);
            value += 1;
            context.WriteVariableIndirectly(variable, (ushort)value);
        };

        public static readonly OpcodeRoutine inc_chk = (i, opValues, opCount, context) =>
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
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Jump routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine je = (i, opValues, opCount, context) =>
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
        };

        public static readonly OpcodeRoutine jg = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasBranch(i);

            var x = (short)opValues[0];
            var y = (short)opValues[1];

            if ((x > y) == i.Branch.Condition)
            {
                context.Jump(i.Branch);
            }
        };

        public static readonly OpcodeRoutine jin = (i, opValues, opCount, context) =>
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
        };

        public static readonly OpcodeRoutine jl = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasBranch(i);

            var x = (short)opValues[0];
            var y = (short)opValues[1];

            if ((x < y) == i.Branch.Condition)
            {
                context.Jump(i.Branch);
            }
        };

        public static readonly OpcodeRoutine jump = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var offset = (short)opValues[0];

            context.Jump(offset);
        };

        public static readonly OpcodeRoutine jz = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 1);
            Strict.HasBranch(i);

            var x = opValues[0];
            var result = x == 0;

            if (result == i.Branch.Condition)
            {
                context.Jump(i.Branch);
            }
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Call routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine call_1n = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var byteAddress = opValues[0];
            var address = context.UnpackRoutineAddress(byteAddress);

            context.Call(address, opValues, opCount);
        };

        public static readonly OpcodeRoutine call_1s = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 1);
            Strict.HasStoreVariable(i);

            var byteAddress = opValues[0];
            var address = context.UnpackRoutineAddress(byteAddress);

            context.Call(address, opValues, opCount, i.StoreVariable);
        };

        public static readonly OpcodeRoutine call_2n = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 2);

            var byteAddress = opValues[0];
            var address = context.UnpackRoutineAddress(byteAddress);

            context.Call(address, opValues, opCount);
        };

        public static readonly OpcodeRoutine call_2s = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var byteAddress = opValues[0];
            var address = context.UnpackRoutineAddress(byteAddress);

            context.Call(address, opValues, opCount, i.StoreVariable);
        };

        public static readonly OpcodeRoutine call_vn = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountInRange(i, 1, 4);

            var byteAddress = opValues[0];
            var address = context.UnpackRoutineAddress(byteAddress);

            context.Call(address, opValues, opCount);
        };

        public static readonly OpcodeRoutine call_vs = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountInRange(i, 1, 4);
            Strict.HasStoreVariable(i);

            var byteAddress = opValues[0];
            var address = context.UnpackRoutineAddress(byteAddress);

            context.Call(address, opValues, opCount, i.StoreVariable);
        };

        public static readonly OpcodeRoutine call_vn2 = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountInRange(i, 1, 8);

            var byteAddress = opValues[0];
            var address = context.UnpackRoutineAddress(byteAddress);

            context.Call(address, opValues, opCount);
        };

        public static readonly OpcodeRoutine call_vs2 = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountInRange(i, 1, 8);
            Strict.HasStoreVariable(i);

            var byteAddress = opValues[0];
            var address = context.UnpackRoutineAddress(byteAddress);

            context.Call(address, opValues, opCount, i.StoreVariable);
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Return routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine ret = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var value = opValues[0];

            context.Return(value);
        };

        public static readonly OpcodeRoutine ret_popped = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 0);

            var value = context.ReadVariable(Variable.Stack);

            context.Return(value);
        };

        public static readonly OpcodeRoutine rfalse = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 0);

            context.Return(0);
        };

        public static readonly OpcodeRoutine rtrue = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 0);

            context.Return(1);
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Load/Store routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine load = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 1);
            Strict.HasStoreVariable(i);

            var varIdx = opValues[0];
            Strict.IsByte(i, varIdx);

            var variable = Variable.FromByte((byte)varIdx);

            var value = context.ReadVariableIndirectly(variable);

            context.WriteVariable(i.StoreVariable, value);
        };

        public static readonly OpcodeRoutine loadb = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var array = opValues[0];
            var byteIndex = opValues[1];

            var address = array + byteIndex;
            var value = context.ReadByte(address);

            context.WriteVariable(i.StoreVariable, value);
        };

        public static readonly OpcodeRoutine loadw = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var array = opValues[0];
            var wordIndex = opValues[1];

            var address = array + (wordIndex * 2);
            var value = context.ReadWord(address);

            context.WriteVariable(i.StoreVariable, value);
        };

        public static readonly OpcodeRoutine store = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 2);

            var varIdx = opValues[0];
            Strict.IsByte(i, varIdx);

            var variable = Variable.FromByte((byte)varIdx);
            var value = opValues[1];

            context.WriteVariableIndirectly(variable, value);
        };

        public static readonly OpcodeRoutine storeb = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 3);

            var array = opValues[0];
            var byteIndex = opValues[1];
            var value = opValues[2];
            Strict.IsByte(i, value);

            var address = array + byteIndex;

            context.WriteByte(address, (byte)value);
        };

        public static readonly OpcodeRoutine storew = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 3);

            var array = opValues[0];
            var wordIndex = opValues[1];
            var value = opValues[2];

            var address = array + (wordIndex * 2);

            context.WriteWord(address, value);
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Table routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine copy_table = (i, opValues, opCount, context) =>
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
        };

        public static readonly OpcodeRoutine scan_table = (i, opValues, opCount, context) =>
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
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Stack routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine pull = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var varIdx = opValues[0];
            Strict.IsByte(i, varIdx);

            var variable = Variable.FromByte((byte)varIdx);
            var value = context.ReadVariable(Variable.Stack);

            context.WriteVariableIndirectly(variable, value);
        };

        public static readonly OpcodeRoutine push = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var value = opValues[0];

            context.WriteVariable(Variable.Stack, value);
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Object routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine clear_attr = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 2);

            var objNum = opValues[0];
            var attrNum = opValues[1];

            context.ClearAttribute(objNum, attrNum);
        };

        public static readonly OpcodeRoutine get_child = (i, opValues, opCount, context) =>
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
        };

        public static readonly OpcodeRoutine get_next_prop = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var objNum = opValues[0];
            var propNum = opValues[1];

            var nextPropNum = context.GetNextProperty(objNum, propNum);

            context.WriteVariable(i.StoreVariable, (ushort)nextPropNum);
        };

        public static readonly OpcodeRoutine get_parent = (i, opValues, opCount, context) =>
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
        };

        public static readonly OpcodeRoutine get_prop = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var objNum = opValues[0];
            var propNum = opValues[1];

            var value = context.GetPropertyData(objNum, propNum);

            context.WriteVariable(i.StoreVariable, (ushort)value);
        };

        public static readonly OpcodeRoutine get_prop_addr = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var objNum = opValues[0];
            var propNum = opValues[1];

            var propAddress = context.GetPropertyDataAddress(objNum, propNum);

            context.WriteVariable(i.StoreVariable, (ushort)propAddress);
        };

        public static readonly OpcodeRoutine get_prop_len = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 1);
            Strict.HasStoreVariable(i);

            var dataAddress = opValues[0];

            var propLen = context.GetPropertyDataLength(dataAddress);

            context.WriteVariable(i.StoreVariable, (ushort)propLen);
        };

        public static readonly OpcodeRoutine get_sibling = (i, opValues, opCount, context) =>
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
        };

        public static readonly OpcodeRoutine insert_obj = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 2);

            var objNum = opValues[0];
            var destNum = opValues[1];

            context.MoveTo(objNum, destNum);
        };

        public static readonly OpcodeRoutine put_prop = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 3);

            var objNum = opValues[0];
            var propNum = opValues[1];
            var value = opValues[2];

            context.WriteProperty(objNum, propNum, value);
        };

        public static readonly OpcodeRoutine remove_obj = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var objNum = opValues[0];

            context.RemoveFromParent(objNum);
        };

        public static readonly OpcodeRoutine set_attr = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 2);

            var objNum = opValues[0];
            var attrNum = opValues[1];

            context.SetAttribute(objNum, attrNum);
        };

        public static readonly OpcodeRoutine test_attr = (i, opValues, opCount, context) =>
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
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Output routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine buffer_mode = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var flag = opValues[0];

            // TODO: What should we do with buffer_mode? Does it have any meaning?
        };

        public static readonly OpcodeRoutine new_line = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 0);

            context.Print('\n');
        };

        public static readonly OpcodeRoutine output_stream = (i, opValues, opCount, context) =>
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
        };

        public static readonly OpcodeRoutine print = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 0);
            Strict.HasZText(i);

            var ztext = context.ParseZWords(i.ZText);
            context.Print(ztext);
        };

        public static readonly OpcodeRoutine print_addr = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var byteAddress = opValues[0];

            var zwords = context.ReadZWords(byteAddress);
            var ztext = context.ParseZWords(zwords);

            context.Print(ztext);
        };

        public static readonly OpcodeRoutine print_char = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var ch = (char)opValues[0];
            context.Print(ch);
        };

        public static readonly OpcodeRoutine print_num = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var number = (short)opValues[0];
            context.Print(number.ToString());
        };

        public static readonly OpcodeRoutine print_obj = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var objNum = opValues[0];

            var shortName = context.GetShortName(objNum);
            context.Print(shortName);
        };

        public static readonly OpcodeRoutine print_paddr = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var byteAddress = opValues[0];
            var address = context.UnpackStringAddress(byteAddress);

            var zwords = context.ReadZWords(address);
            var ztext = context.ParseZWords(zwords);

            context.Print(ztext);
        };

        public static readonly OpcodeRoutine print_ret = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 0);
            Strict.HasZText(i);

            var ztext = context.ParseZWords(i.ZText);
            context.Print(ztext + "\n");
            context.Return(1);
        };

        public static readonly OpcodeRoutine print_table = (i, opValues, opCount, context) =>
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

        };

        public static readonly OpcodeRoutine set_color = (i, opValues, opCount, context) =>
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
        };

        public static readonly OpcodeRoutine set_font = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 1);
            Strict.HasStoreVariable(i);

            var font = (ZFont)opValues[0];

            var oldFont = context.Screen.SetFont(font);
            context.WriteVariable(i.StoreVariable, (ushort)oldFont);
        };

        public static readonly OpcodeRoutine set_text_style = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var textStyle = (ZTextStyle)opValues[0];

            context.Screen.SetTextStyle(textStyle);
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Input routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine read_char = (i, opValues, opCount, context) =>
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
        };

        public static readonly OpcodeRoutine aread = (i, opValues, opCount, context) =>
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
        };

        public static readonly OpcodeRoutine sread1 = (i, opValues, opCount, context) =>
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
        };

        public static readonly OpcodeRoutine sread2 = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountInRange(i, 2, 4);

            int textBuffer = opValues[0];
            int parseBuffer = opValues[1];

            // TODO: Support timed input

            if (opCount > 2)
            {
                context.MessageLog.SendWarning(i, "timed input was attempted but it is unsupported");
                var time = context.GetOperandValue(i.Operands[2]);
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
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Window routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine erase_window = (i, opValues, opCount, context) =>
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
        };

        public static readonly OpcodeRoutine set_cursor = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 2);

            var line = opValues[0];
            var column = opValues[1];

            context.Screen.SetCursor(line - 1, column - 1);
        };

        public static readonly OpcodeRoutine set_window = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var window = opValues[0];

            context.Screen.SetWindow(window);
        };

        public static readonly OpcodeRoutine split_window = (i, opValues, opCount, context) =>
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
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Miscellaneous routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine check_arg_count = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 1);
            Strict.HasBranch(i);

            var argNumber = opValues[0];
            var argCount = context.GetArgumentCount();

            if ((argNumber <= argCount) == i.Branch.Condition)
            {
                context.Jump(i.Branch);
            }
        };

        public static readonly OpcodeRoutine piracy = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 0);
            Strict.HasBranch(i);

            context.Jump(i.Branch);
        };

        public static readonly OpcodeRoutine quit = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 0);

            context.Quit();
        };

        public static readonly OpcodeRoutine random = (i, opValues, opCount, context) =>
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
        };

        public static readonly OpcodeRoutine restore_undo = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 0);
            Strict.HasStoreVariable(i);

            context.MessageLog.SendWarning(i, "Undo is not supported.");

            context.WriteVariable(i.StoreVariable, unchecked((ushort)-1));
        };

        public static readonly OpcodeRoutine save_undo = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 0);
            Strict.HasStoreVariable(i);

            context.MessageLog.SendWarning(i, "Undo is not supported.");

            context.WriteVariable(i.StoreVariable, unchecked((ushort)-1));
        };

        public static readonly OpcodeRoutine show_status = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 0);

            context.Screen.ShowStatus();
        };

        public static readonly OpcodeRoutine tokenize = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountInRange(i, 2, 4);

            int textBuffer = opValues[0];
            int parseBuffer = opValues[1];

            int? dictionaryAddress = opCount > 2
                ? (int?)opValues[2]
                : null;

            bool flag = opCount > 3
                ? opValues[3] != 0
                : false;
        };

        public static readonly OpcodeRoutine verify = (i, opValues, opCount, context) =>
        {
            Strict.OperandCountIs(i, 0);
            Strict.HasBranch(i);

            if (context.VerifyChecksum() == i.Branch.Condition)
            {
                context.Jump(i.Branch);
            }
        };
    }
}
