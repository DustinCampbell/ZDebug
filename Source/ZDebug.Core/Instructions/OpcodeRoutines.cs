using System;
using ZDebug.Core.Dictionary;
using ZDebug.Core.Execution;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Instructions
{
    internal static class OpcodeRoutines
    {
        ///////////////////////////////////////////////////////////////////////////////////////////
        // Arithmetic routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine add = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var x = (short)context.GetOperandValue(i.Operands[0]);
            var y = (short)context.GetOperandValue(i.Operands[1]);

            var result = (ushort)(x + y);

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine div = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var x = (short)context.GetOperandValue(i.Operands[0]);
            var y = (short)context.GetOperandValue(i.Operands[1]);

            var result = (ushort)(x / y);

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine mod = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var x = (short)context.GetOperandValue(i.Operands[0]);
            var y = (short)context.GetOperandValue(i.Operands[1]);

            var result = (ushort)(x % y);

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine mul = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var x = (short)context.GetOperandValue(i.Operands[0]);
            var y = (short)context.GetOperandValue(i.Operands[1]);

            var result = (ushort)(x * y);

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine sub = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var x = (short)context.GetOperandValue(i.Operands[0]);
            var y = (short)context.GetOperandValue(i.Operands[1]);

            var result = (ushort)(x - y);

            context.WriteVariable(i.StoreVariable, result);
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Bit-level routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine and = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var x = context.GetOperandValue(i.Operands[0]);
            var y = context.GetOperandValue(i.Operands[1]);

            var result = (ushort)(x & y);

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine art_shift = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var number = (short)context.GetOperandValue(i.Operands[0]);
            var places = (int)(short)context.GetOperandValue(i.Operands[1]);

            var result = places > 0
                ? number << places
                : number >> -places;

            context.WriteVariable(i.StoreVariable, (ushort)result);
        };

        public static readonly OpcodeRoutine log_shift = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var number = context.GetOperandValue(i.Operands[0]);
            var places = (int)(short)context.GetOperandValue(i.Operands[1]);

            var result = places > 0
                ? number << places
                : number >> -places;

            context.WriteVariable(i.StoreVariable, (ushort)result);
        };

        public static readonly OpcodeRoutine not = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);
            Strict.HasStoreVariable(i);

            var x = context.GetOperandValue(i.Operands[0]);

            var result = (ushort)(~x);

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine or = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var x = context.GetOperandValue(i.Operands[0]);
            var y = context.GetOperandValue(i.Operands[1]);

            var result = (ushort)(x | y);

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine test = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasBranch(i);

            var bitmap = context.GetOperandValue(i.Operands[0]);
            var flags = context.GetOperandValue(i.Operands[1]);

            var result = (bitmap & flags) == flags;

            if (result == i.Branch.Condition)
            {
                context.Jump(i.Branch);
            }
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Increment/decrement routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine dec = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var varIdx = context.GetOperandValue(i.Operands[0]);
            Strict.IsByte(i, varIdx);

            var variable = Variable.FromByte((byte)varIdx);

            var value = (short)context.ReadVariableIndirectly(variable);
            value -= 1;
            context.WriteVariableIndirectly(variable, (ushort)value);
        };

        public static readonly OpcodeRoutine dec_chk = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasBranch(i);

            var varIdx = context.GetOperandValue(i.Operands[0]);
            Strict.IsByte(i, varIdx);

            var test = (short)context.GetOperandValue(i.Operands[1]);

            var variable = Variable.FromByte((byte)varIdx);

            var value = (short)context.ReadVariableIndirectly(variable);
            value -= 1;
            context.WriteVariableIndirectly(variable, (ushort)value);

            if ((value < test) == i.Branch.Condition)
            {
                context.Jump(i.Branch);
            }
        };

        public static readonly OpcodeRoutine inc = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var varIdx = context.GetOperandValue(i.Operands[0]);
            Strict.IsByte(i, varIdx);

            var variable = Variable.FromByte((byte)varIdx);

            var value = (short)context.ReadVariableIndirectly(variable);
            value += 1;
            context.WriteVariableIndirectly(variable, (ushort)value);
        };

        public static readonly OpcodeRoutine inc_chk = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasBranch(i);

            var varIdx = context.GetOperandValue(i.Operands[0]);
            Strict.IsByte(i, varIdx);

            var test = (short)context.GetOperandValue(i.Operands[1]);

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

        public static readonly OpcodeRoutine je = (i, context) =>
        {
            Strict.OperandCountInRange(i, 2, 4);
            Strict.HasBranch(i);

            var ops = i.Operands;

            var x = context.GetOperandValue(ops[0]);

            bool result = false;
            for (int j = 1; j < ops.Length; j++)
            {
                if (x == context.GetOperandValue(ops[j]))
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

        public static readonly OpcodeRoutine jg = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasBranch(i);

            var x = (short)context.GetOperandValue(i.Operands[0]);
            var y = (short)context.GetOperandValue(i.Operands[1]);

            if ((x > y) == i.Branch.Condition)
            {
                context.Jump(i.Branch);
            }
        };

        public static readonly OpcodeRoutine jin = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasBranch(i);

            var obj1 = context.GetOperandValue(i.Operands[0]);
            var obj2 = context.GetOperandValue(i.Operands[1]);

            var obj1Parent = context.GetParent(obj1);

            if ((obj1Parent == obj2) == i.Branch.Condition)
            {
                context.Jump(i.Branch);
            }
        };

        public static readonly OpcodeRoutine jl = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasBranch(i);

            var x = (short)context.GetOperandValue(i.Operands[0]);
            var y = (short)context.GetOperandValue(i.Operands[1]);

            if ((x < y) == i.Branch.Condition)
            {
                context.Jump(i.Branch);
            }
        };

        public static readonly OpcodeRoutine jump = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var offset = (short)context.GetOperandValue(i.Operands[0]);

            context.Jump(offset);
        };

        public static readonly OpcodeRoutine jz = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);
            Strict.HasBranch(i);

            var x = context.GetOperandValue(i.Operands[0]);
            var result = x == 0;

            if (result == i.Branch.Condition)
            {
                context.Jump(i.Branch);
            }
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Call routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine call_1n = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var byteAddress = context.GetOperandValue(i.Operands[0]);
            var address = context.UnpackRoutineAddress(byteAddress);

            context.Call(address);
        };

        public static readonly OpcodeRoutine call_1s = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);
            Strict.HasStoreVariable(i);

            var byteAddress = context.GetOperandValue(i.Operands[0]);
            var address = context.UnpackRoutineAddress(byteAddress);

            context.Call(address, storeVariable: i.StoreVariable);
        };

        public static readonly OpcodeRoutine call_2n = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);

            var byteAddress = context.GetOperandValue(i.Operands[0]);
            var address = context.UnpackRoutineAddress(byteAddress);

            var args = i.Operands.Skip(1);

            context.Call(address, args);
        };

        public static readonly OpcodeRoutine call_2s = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var byteAddress = context.GetOperandValue(i.Operands[0]);
            var address = context.UnpackRoutineAddress(byteAddress);

            var args = i.Operands.Skip(1);

            context.Call(address, args, i.StoreVariable);
        };

        public static readonly OpcodeRoutine call_vn = (i, context) =>
        {
            Strict.OperandCountInRange(i, 1, 4);

            var byteAddress = context.GetOperandValue(i.Operands[0]);
            var address = context.UnpackRoutineAddress(byteAddress);

            var args = i.Operands.Skip(1);

            context.Call(address, args);
        };

        public static readonly OpcodeRoutine call_vs = (i, context) =>
        {
            Strict.OperandCountInRange(i, 1, 4);
            Strict.HasStoreVariable(i);

            var byteAddress = context.GetOperandValue(i.Operands[0]);
            var address = context.UnpackRoutineAddress(byteAddress);

            var args = i.Operands.Skip(1);

            context.Call(address, args, i.StoreVariable);
        };

        public static readonly OpcodeRoutine call_vn2 = (i, context) =>
        {
            Strict.OperandCountInRange(i, 1, 8);

            var byteAddress = context.GetOperandValue(i.Operands[0]);
            var address = context.UnpackRoutineAddress(byteAddress);

            var args = i.Operands.Skip(1);

            context.Call(address, args);
        };

        public static readonly OpcodeRoutine call_vs2 = (i, context) =>
        {
            Strict.OperandCountInRange(i, 1, 8);
            Strict.HasStoreVariable(i);

            var byteAddress = context.GetOperandValue(i.Operands[0]);
            var address = context.UnpackRoutineAddress(byteAddress);

            var args = i.Operands.Skip(1);

            context.Call(address, args, i.StoreVariable);
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Return routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine ret = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var value = context.GetOperandValue(i.Operands[0]);

            context.Return(value);
        };

        public static readonly OpcodeRoutine ret_popped = (i, context) =>
        {
            Strict.OperandCountIs(i, 0);

            var value = context.ReadVariable(Variable.Stack);

            context.Return(value);
        };

        public static readonly OpcodeRoutine rfalse = (i, context) =>
        {
            Strict.OperandCountIs(i, 0);

            context.Return(0);
        };

        public static readonly OpcodeRoutine rtrue = (i, context) =>
        {
            Strict.OperandCountIs(i, 0);

            context.Return(1);
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Load/Store routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine load = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);
            Strict.HasStoreVariable(i);

            var varIdx = context.GetOperandValue(i.Operands[0]);
            Strict.IsByte(i, varIdx);

            var variable = Variable.FromByte((byte)varIdx);

            var value = context.ReadVariableIndirectly(variable);

            context.WriteVariable(i.StoreVariable, value);
        };

        public static readonly OpcodeRoutine loadb = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var array = context.GetOperandValue(i.Operands[0]);
            var byteIndex = context.GetOperandValue(i.Operands[1]);

            var address = array + byteIndex;
            var value = context.ReadByte(address);

            context.WriteVariable(i.StoreVariable, value);
        };

        public static readonly OpcodeRoutine loadw = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var array = context.GetOperandValue(i.Operands[0]);
            var wordIndex = context.GetOperandValue(i.Operands[1]);

            var address = array + (wordIndex * 2);
            var value = context.ReadWord(address);

            context.WriteVariable(i.StoreVariable, value);
        };

        public static readonly OpcodeRoutine store = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);

            var varIdx = context.GetOperandValue(i.Operands[0]);
            Strict.IsByte(i, varIdx);

            var variable = Variable.FromByte((byte)varIdx);
            var value = context.GetOperandValue(i.Operands[1]);

            context.WriteVariableIndirectly(variable, value);
        };

        public static readonly OpcodeRoutine storeb = (i, context) =>
        {
            Strict.OperandCountIs(i, 3);

            var array = context.GetOperandValue(i.Operands[0]);
            var byteIndex = context.GetOperandValue(i.Operands[1]);
            var value = context.GetOperandValue(i.Operands[2]);
            Strict.IsByte(i, value);

            var address = array + byteIndex;

            context.WriteByte(address, (byte)value);
        };

        public static readonly OpcodeRoutine storew = (i, context) =>
        {
            Strict.OperandCountIs(i, 3);

            var array = context.GetOperandValue(i.Operands[0]);
            var wordIndex = context.GetOperandValue(i.Operands[1]);
            var value = context.GetOperandValue(i.Operands[2]);

            var address = array + (wordIndex * 2);

            context.WriteWord(address, value);
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Table routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine copy_table = (i, context) =>
        {
            Strict.OperandCountIs(i, 3);

            var first = context.GetOperandValue(i.Operands[0]);
            var second = context.GetOperandValue(i.Operands[1]);
            var size = context.GetOperandValue(i.Operands[2]);

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

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Stack routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine pull = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var varIdx = context.GetOperandValue(i.Operands[0]);
            Strict.IsByte(i, varIdx);

            var variable = Variable.FromByte((byte)varIdx);
            var value = context.ReadVariable(Variable.Stack);

            context.WriteVariableIndirectly(variable, value);
        };

        public static readonly OpcodeRoutine push = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var value = context.GetOperandValue(i.Operands[0]);

            context.WriteVariable(Variable.Stack, value);
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Object routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine clear_attr = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);

            var objNum = context.GetOperandValue(i.Operands[0]);
            var attrNum = context.GetOperandValue(i.Operands[1]);

            context.ClearAttribute(objNum, attrNum);
        };

        public static readonly OpcodeRoutine get_child = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);
            Strict.HasBranch(i);
            Strict.HasStoreVariable(i);

            var objNum = context.GetOperandValue(i.Operands[0]);

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

        public static readonly OpcodeRoutine get_next_prop = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var objNum = context.GetOperandValue(i.Operands[0]);
            var propNum = context.GetOperandValue(i.Operands[1]);

            var nextPropNum = context.GetNextProperty(objNum, propNum);

            context.WriteVariable(i.StoreVariable, (ushort)nextPropNum);
        };

        public static readonly OpcodeRoutine get_parent = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);
            Strict.HasStoreVariable(i);

            var objNum = context.GetOperandValue(i.Operands[0]);

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

        public static readonly OpcodeRoutine get_prop = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var objNum = context.GetOperandValue(i.Operands[0]);
            var propNum = context.GetOperandValue(i.Operands[1]);

            var value = context.GetPropertyData(objNum, propNum);

            context.WriteVariable(i.StoreVariable, (ushort)value);
        };

        public static readonly OpcodeRoutine get_prop_addr = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var objNum = context.GetOperandValue(i.Operands[0]);
            var propNum = context.GetOperandValue(i.Operands[1]);

            var propAddress = context.GetPropertyDataAddress(objNum, propNum);

            context.WriteVariable(i.StoreVariable, (ushort)propAddress);
        };

        public static readonly OpcodeRoutine get_prop_len = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);
            Strict.HasStoreVariable(i);

            var dataAddress = context.GetOperandValue(i.Operands[0]);

            var propLen = context.GetPropertyDataLength(dataAddress);

            context.WriteVariable(i.StoreVariable, (ushort)propLen);
        };

        public static readonly OpcodeRoutine get_sibling = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);
            Strict.HasBranch(i);
            Strict.HasStoreVariable(i);

            var objNum = context.GetOperandValue(i.Operands[0]);

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

        public static readonly OpcodeRoutine insert_obj = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);

            var objNum = context.GetOperandValue(i.Operands[0]);
            var destNum = context.GetOperandValue(i.Operands[1]);

            context.MoveTo(objNum, destNum);
        };

        public static readonly OpcodeRoutine put_prop = (i, context) =>
        {
            Strict.OperandCountIs(i, 3);

            var objNum = context.GetOperandValue(i.Operands[0]);
            var propNum = context.GetOperandValue(i.Operands[1]);
            var value = context.GetOperandValue(i.Operands[2]);

            context.WriteProperty(objNum, propNum, value);
        };

        public static readonly OpcodeRoutine remove_obj = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var objNum = context.GetOperandValue(i.Operands[0]);

            context.RemoveFromParent(objNum);
        };

        public static readonly OpcodeRoutine set_attr = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);

            var objNum = context.GetOperandValue(i.Operands[0]);
            var attrNum = context.GetOperandValue(i.Operands[1]);

            context.SetAttribute(objNum, attrNum);
        };

        public static readonly OpcodeRoutine test_attr = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasBranch(i);

            var objNum = context.GetOperandValue(i.Operands[0]);
            var attrNum = context.GetOperandValue(i.Operands[1]);

            var result = context.HasAttribute(objNum, attrNum);

            if (result == i.Branch.Condition)
            {
                context.Jump(i.Branch);
            }
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Output routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine buffer_mode = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var flag = context.GetOperandValue(i.Operands[0]);

            // TODO: What should we do with buffer_mode? Does it have any meaning?
        };

        public static readonly OpcodeRoutine new_line = (i, context) =>
        {
            Strict.OperandCountIs(i, 0);

            context.Print('\n');
        };

        public static readonly OpcodeRoutine output_stream = (i, context) =>
        {
            Strict.OperandCountInRange(i, 1, 2);

            var number = (short)context.GetOperandValue(i.Operands[0]);

            switch (number)
            {
                case 1:
                    context.SelectScreenStream();
                    break;

                case 2:
                    context.SelectTranscriptStream();
                    break;

                case 3:
                    var address = context.GetOperandValue(i.Operands[1]);
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

        public static readonly OpcodeRoutine print = (i, context) =>
        {
            Strict.OperandCountIs(i, 0);
            Strict.HasZText(i);

            var ztext = context.ParseZWords(i.ZText);
            context.Print(ztext);
        };

        public static readonly OpcodeRoutine print_addr = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var byteAddress = context.GetOperandValue(i.Operands[0]);

            var zwords = context.ReadZWords(byteAddress);
            var ztext = context.ParseZWords(zwords);

            context.Print(ztext);
        };

        public static readonly OpcodeRoutine print_char = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var ch = (char)context.GetOperandValue(i.Operands[0]);
            context.Print(ch);
        };

        public static readonly OpcodeRoutine print_num = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var number = (short)context.GetOperandValue(i.Operands[0]);
            context.Print(number.ToString());
        };

        public static readonly OpcodeRoutine print_obj = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var objNum = context.GetOperandValue(i.Operands[0]);

            var shortName = context.GetShortName(objNum);
            context.Print(shortName);
        };

        public static readonly OpcodeRoutine print_paddr = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var byteAddress = context.GetOperandValue(i.Operands[0]);
            var address = context.UnpackStringAddress(byteAddress);

            var zwords = context.ReadZWords(address);
            var ztext = context.ParseZWords(zwords);

            context.Print(ztext);
        };

        public static readonly OpcodeRoutine print_ret = (i, context) =>
        {
            Strict.OperandCountIs(i, 0);
            Strict.HasZText(i);

            var ztext = context.ParseZWords(i.ZText);
            context.Print(ztext + "\n");
            context.Return(1);
        };

        public static readonly OpcodeRoutine print_table = (i, context) =>
        {
            Strict.OperandCountInRange(i, 2, 4);

            var address = context.GetOperandValue(i.Operands[0]);
            var width = context.GetOperandValue(i.Operands[1]);
            var height = i.Operands.Length > 2
                ? (ushort)context.GetOperandValue(i.Operands[2])
                : (ushort)1;
            var skip = i.Operands.Length > 3
                ? (ushort)context.GetOperandValue(i.Operands[3])
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

        public static readonly OpcodeRoutine set_color = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);

            var foreground = (ZColor)context.GetOperandValue(i.Operands[0]);
            var background = (ZColor)context.GetOperandValue(i.Operands[1]);

            if (foreground != 0)
            {
                context.Screen.SetForegroundColor(foreground);
            }

            if (background != 0)
            {
                context.Screen.SetBackgroundColor(background);
            }
        };

        public static readonly OpcodeRoutine set_font = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);
            Strict.HasStoreVariable(i);

            var font = (ZFont)context.GetOperandValue(i.Operands[0]);

            var oldFont = context.Screen.SetFont(font);
            context.WriteVariable(i.StoreVariable, (ushort)oldFont);
        };

        public static readonly OpcodeRoutine set_text_style = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var textStyle = (ZTextStyle)context.GetOperandValue(i.Operands[0]);

            context.Screen.SetTextStyle(textStyle);
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Input routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine read_char = (i, context) =>
        {
            Strict.OperandCountInRange(i, 0, 3);
            Strict.HasStoreVariable(i);

            if (i.Operands.Length > 0)
            {
                var inputStream = context.GetOperandValue(i.Operands[0]);

                if (inputStream != 1)
                {
                    context.MessageLog.SendWarning(i, "expected first operand to be 1 but was " + inputStream);
                }
            }
            else
            {
                context.MessageLog.SendWarning(i, "expected at least 1 operand.");
            }

            if (i.Operands.Length > 1)
            {
                // TODO: Unsupported for the moment, but we need to read the operand to keep the stack consistent.
                context.GetOperandValue(i.Operands[1]);
            }

            if (i.Operands.Length > 2)
            {
                // TODO: Unsupported for the moment, but we need to read the operand to keep the stack consistent.
                context.GetOperandValue(i.Operands[2]);
            }

            Action<char> callback = ch =>
            {
                context.WriteVariable(i.StoreVariable, (ushort)ch);
            };

            context.Screen.ReadChar(callback);
        };

        public static readonly OpcodeRoutine aread = (i, context) =>
        {
            Strict.OperandCountInRange(i, 1, 4);
            Strict.HasStoreVariable(i);

            int textBuffer = context.GetOperandValue(i.Operands[0]);

            int parseBuffer = 0;
            if (i.Operands.Length > 1)
            {
                parseBuffer = context.GetOperandValue(i.Operands[1]);
            }

            // TODO: Support timed input

            if (i.Operands.Length > 2)
            {
                context.MessageLog.SendWarning(i, "timed input was attempted but it is unsupported");
                var time = context.GetOperandValue(i.Operands[2]);
            }

            if (i.Operands.Length > 3)
            {
                var routine = context.GetOperandValue(i.Operands[3]);
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

                        ZDictionaryEntry entry;
                        if (context.TryLookupWord(token.Text, out entry))
                        {
                            context.WriteWord(parseBuffer + 2 + (j * 4), (ushort)entry.Address);
                        }
                        else
                        {
                            context.WriteWord(parseBuffer + 2 + (j * 4), 0);
                        }

                        context.WriteByte(parseBuffer + 2 + (j * 4) + 2, (byte)token.Length);
                        context.WriteByte(parseBuffer + 2 + (j * 4) + 3, (byte)(token.Start + 1));
                    }
                }

                // TODO: Update this when timed input is supported
                context.WriteVariable(i.StoreVariable, 10);
            });
        };

        public static readonly OpcodeRoutine sread1 = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);

            int textBuffer = context.GetOperandValue(i.Operands[0]);
            int parseBuffer = context.GetOperandValue(i.Operands[1]);

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

                    ZDictionaryEntry entry;
                    if (context.TryLookupWord(token.Text, out entry))
                    {
                        context.WriteWord(parseBuffer + 2 + (j * 4), (ushort)entry.Address);
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

        public static readonly OpcodeRoutine sread2 = (i, context) =>
        {
            Strict.OperandCountInRange(i, 2, 4);

            int textBuffer = context.GetOperandValue(i.Operands[0]);
            int parseBuffer = context.GetOperandValue(i.Operands[1]);

            // TODO: Support timed input

            if (i.Operands.Length > 2)
            {
                context.MessageLog.SendWarning(i, "timed input was attempted but it is unsupported");
                var time = context.GetOperandValue(i.Operands[2]);
            }

            if (i.Operands.Length > 3)
            {
                var routine = context.GetOperandValue(i.Operands[3]);
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

                    ZDictionaryEntry entry;
                    if (context.TryLookupWord(token.Text, out entry))
                    {
                        context.WriteWord(parseBuffer + 2 + (j * 4), (ushort)entry.Address);
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

        public static readonly OpcodeRoutine erase_window = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var window = (short)context.GetOperandValue(i.Operands[0]);

            if (window == -1 || window == -2)
            {
                context.Screen.ClearAll(unsplit: window == -1);
            }
            else
            {
                context.Screen.Clear(window);
            }
        };

        public static readonly OpcodeRoutine set_cursor = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);

            var line = context.GetOperandValue(i.Operands[0]);
            var column = context.GetOperandValue(i.Operands[1]);

            context.Screen.SetCursor(line - 1, column - 1);
        };

        public static readonly OpcodeRoutine set_window = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var window = context.GetOperandValue(i.Operands[0]);

            context.Screen.SetWindow(window);
        };

        public static readonly OpcodeRoutine split_window = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var height = context.GetOperandValue(i.Operands[0]);

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

        public static readonly OpcodeRoutine check_arg_count = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);
            Strict.HasBranch(i);

            var argNumber = context.GetOperandValue(i.Operands[0]);
            var argCount = context.GetArgumentCount();

            if ((argNumber <= argCount) == i.Branch.Condition)
            {
                context.Jump(i.Branch);
            }
        };

        public static readonly OpcodeRoutine piracy = (i, context) =>
        {
            Strict.OperandCountIs(i, 0);
            Strict.HasBranch(i);

            context.Jump(i.Branch);
        };

        public static readonly OpcodeRoutine quit = (i, context) =>
        {
            Strict.OperandCountIs(i, 0);

            context.Quit();
        };

        public static readonly OpcodeRoutine random = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);
            Strict.HasStoreVariable(i);

            var range = (short)context.GetOperandValue(i.Operands[0]);

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

        public static readonly OpcodeRoutine restore_undo = (i, context) =>
        {
            Strict.OperandCountIs(i, 0);
            Strict.HasStoreVariable(i);

            context.MessageLog.SendWarning(i, "Undo is not supported.");

            context.WriteVariable(i.StoreVariable, unchecked((ushort)-1));
        };

        public static readonly OpcodeRoutine save_undo = (i, context) =>
        {
            Strict.OperandCountIs(i, 0);
            Strict.HasStoreVariable(i);

            context.MessageLog.SendWarning(i, "Undo is not supported.");

            context.WriteVariable(i.StoreVariable, unchecked((ushort)-1));
        };

        public static readonly OpcodeRoutine show_status = (i, context) =>
        {
            Strict.OperandCountIs(i, 0);

            context.Screen.ShowStatus();
        };

        public static readonly OpcodeRoutine verify = (i, context) =>
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
