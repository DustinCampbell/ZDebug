using System;
using System.Linq;
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

            var result = Value.Number((ushort)(x + y));

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine div = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var x = (short)context.GetOperandValue(i.Operands[0]);
            var y = (short)context.GetOperandValue(i.Operands[1]);

            var result = Value.Number((ushort)(x / y));

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine mod = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var x = (short)context.GetOperandValue(i.Operands[0]);
            var y = (short)context.GetOperandValue(i.Operands[1]);

            var result = Value.Number((ushort)(x % y));

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine mul = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var x = (short)context.GetOperandValue(i.Operands[0]);
            var y = (short)context.GetOperandValue(i.Operands[1]);

            var result = Value.Number((ushort)(x * y));

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine sub = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var x = (short)context.GetOperandValue(i.Operands[0]);
            var y = (short)context.GetOperandValue(i.Operands[1]);

            var result = Value.Number((ushort)(x - y));

            context.WriteVariable(i.StoreVariable, result);
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Bit-level routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine and = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var x = (ushort)context.GetOperandValue(i.Operands[0]);
            var y = (ushort)context.GetOperandValue(i.Operands[1]);

            var result = Value.Number((ushort)(x & y));

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

            context.WriteVariable(i.StoreVariable, Value.Number((ushort)result));
        };

        public static readonly OpcodeRoutine log_shift = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var number = (ushort)context.GetOperandValue(i.Operands[0]);
            var places = (int)(short)context.GetOperandValue(i.Operands[1]);

            var result = places > 0
                ? number << places
                : number >> -places;

            context.WriteVariable(i.StoreVariable, Value.Number((ushort)result));
        };

        public static readonly OpcodeRoutine not = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);
            Strict.HasStoreVariable(i);

            var x = (ushort)context.GetOperandValue(i.Operands[0]);

            var result = Value.Number((ushort)(~x));

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine or = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var x = (ushort)context.GetOperandValue(i.Operands[0]);
            var y = (ushort)context.GetOperandValue(i.Operands[1]);

            var result = Value.Number((ushort)(x | y));

            context.WriteVariable(i.StoreVariable, result);
        };

        public static readonly OpcodeRoutine test = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasBranch(i);

            var bitmap = (ushort)context.GetOperandValue(i.Operands[0]);
            var flags = (ushort)context.GetOperandValue(i.Operands[1]);

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

            var varIdx = (ushort)context.GetOperandValue(i.Operands[0]);
            Strict.IsByte(i, varIdx);

            var variable = Variable.FromByte((byte)varIdx);

            var value = (short)context.ReadVariableIndirectly(variable);
            value -= 1;
            context.WriteVariableIndirectly(variable, Value.Number((ushort)value));
        };

        public static readonly OpcodeRoutine dec_chk = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasBranch(i);

            var varIdx = (ushort)context.GetOperandValue(i.Operands[0]);
            Strict.IsByte(i, varIdx);

            var test = (short)context.GetOperandValue(i.Operands[1]);

            var variable = Variable.FromByte((byte)varIdx);

            var value = (short)context.ReadVariableIndirectly(variable);
            value -= 1;
            context.WriteVariableIndirectly(variable, Value.Number((ushort)value));

            if ((value < test) == i.Branch.Condition)
            {
                context.Jump(i.Branch);
            }
        };

        public static readonly OpcodeRoutine inc = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var varIdx = (ushort)context.GetOperandValue(i.Operands[0]);
            Strict.IsByte(i, varIdx);

            var variable = Variable.FromByte((byte)varIdx);

            var value = (short)context.ReadVariableIndirectly(variable);
            value += 1;
            context.WriteVariableIndirectly(variable, Value.Number((ushort)value));
        };

        public static readonly OpcodeRoutine inc_chk = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasBranch(i);

            var varIdx = (ushort)context.GetOperandValue(i.Operands[0]);
            Strict.IsByte(i, varIdx);

            var test = (short)context.GetOperandValue(i.Operands[1]);

            var variable = Variable.FromByte((byte)varIdx);

            var value = (short)context.ReadVariableIndirectly(variable);
            value += 1;
            context.WriteVariableIndirectly(variable, Value.Number((ushort)value));

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

            var x = (ushort)context.GetOperandValue(i.Operands[0]);
            var result = i.Operands.Skip(1).Any(op => x == (ushort)context.GetOperandValue(op));

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

            var obj1 = (ushort)context.GetOperandValue(i.Operands[0]);
            var obj2 = (ushort)context.GetOperandValue(i.Operands[1]);

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

            var x = (ushort)context.GetOperandValue(i.Operands[0]);
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

            var addressOpValue = context.GetOperandValue(i.Operands[0]);
            var address = context.UnpackRoutineAddress(addressOpValue.RawValue);

            context.Call(address);
        };

        public static readonly OpcodeRoutine call_1s = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);
            Strict.HasStoreVariable(i);

            var addressOpValue = context.GetOperandValue(i.Operands[0]);
            var address = context.UnpackRoutineAddress(addressOpValue.RawValue);

            context.Call(address, storeVariable: i.StoreVariable);
        };

        public static readonly OpcodeRoutine call_2n = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);

            var addressOpValue = context.GetOperandValue(i.Operands[0]);
            var address = context.UnpackRoutineAddress(addressOpValue.RawValue);

            var args = i.Operands.Skip(1);

            context.Call(address, args);
        };

        public static readonly OpcodeRoutine call_2s = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var addressOpValue = context.GetOperandValue(i.Operands[0]);
            var address = context.UnpackRoutineAddress(addressOpValue.RawValue);

            var args = i.Operands.Skip(1);

            context.Call(address, args, i.StoreVariable);
        };

        public static readonly OpcodeRoutine call_vn = (i, context) =>
        {
            Strict.OperandCountInRange(i, 1, 4);

            var addressOpValue = context.GetOperandValue(i.Operands[0]);
            var address = context.UnpackRoutineAddress(addressOpValue.RawValue);

            var args = i.Operands.Skip(1);

            context.Call(address, args);
        };

        public static readonly OpcodeRoutine call_vs = (i, context) =>
        {
            Strict.OperandCountInRange(i, 1, 4);
            Strict.HasStoreVariable(i);

            var addressOpValue = context.GetOperandValue(i.Operands[0]);
            var address = context.UnpackRoutineAddress(addressOpValue.RawValue);

            var args = i.Operands.Skip(1);

            context.Call(address, args, i.StoreVariable);
        };

        public static readonly OpcodeRoutine call_vn2 = (i, context) =>
        {
            Strict.OperandCountInRange(i, 1, 8);

            var addressOpValue = context.GetOperandValue(i.Operands[0]);
            var address = context.UnpackRoutineAddress(addressOpValue.RawValue);

            var args = i.Operands.Skip(1);

            context.Call(address, args);
        };

        public static readonly OpcodeRoutine call_vs2 = (i, context) =>
        {
            Strict.OperandCountInRange(i, 1, 8);
            Strict.HasStoreVariable(i);

            var addressOpValue = context.GetOperandValue(i.Operands[0]);
            var address = context.UnpackRoutineAddress(addressOpValue.RawValue);

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

            context.Return(Value.Zero);
        };

        public static readonly OpcodeRoutine rtrue = (i, context) =>
        {
            Strict.OperandCountIs(i, 0);

            context.Return(Value.One);
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Load/Store routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine load = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);
            Strict.HasStoreVariable(i);

            var varIdx = (ushort)context.GetOperandValue(i.Operands[0]);
            Strict.IsByte(i, varIdx);

            var variable = Variable.FromByte((byte)varIdx);

            var value = context.ReadVariableIndirectly(variable);

            context.WriteVariable(i.StoreVariable, value);
        };

        public static readonly OpcodeRoutine loadb = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var array = (ushort)context.GetOperandValue(i.Operands[0]);
            var byteIndex = (ushort)context.GetOperandValue(i.Operands[1]);

            var address = array + byteIndex;
            var value = context.ReadByte(address);

            context.WriteVariable(i.StoreVariable, value);
        };

        public static readonly OpcodeRoutine loadw = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var array = (ushort)context.GetOperandValue(i.Operands[0]);
            var wordIndex = (ushort)context.GetOperandValue(i.Operands[1]);

            var address = array + (wordIndex * 2);
            var value = context.ReadWord(address);

            context.WriteVariable(i.StoreVariable, value);
        };

        public static readonly OpcodeRoutine store = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);

            var varIdx = (ushort)context.GetOperandValue(i.Operands[0]);
            Strict.IsByte(i, varIdx);

            var variable = Variable.FromByte((byte)varIdx);
            var value = context.GetOperandValue(i.Operands[1]);

            context.WriteVariableIndirectly(variable, value);
        };

        public static readonly OpcodeRoutine storeb = (i, context) =>
        {
            Strict.OperandCountIs(i, 3);

            var array = (ushort)context.GetOperandValue(i.Operands[0]);
            var byteIndex = (ushort)context.GetOperandValue(i.Operands[1]);
            var value = (ushort)context.GetOperandValue(i.Operands[2]);
            Strict.IsByte(i, value);

            var address = array + byteIndex;

            context.WriteByte(address, (byte)value);
        };

        public static readonly OpcodeRoutine storew = (i, context) =>
        {
            Strict.OperandCountIs(i, 3);

            var array = (ushort)context.GetOperandValue(i.Operands[0]);
            var wordIndex = (ushort)context.GetOperandValue(i.Operands[1]);
            var value = (ushort)context.GetOperandValue(i.Operands[2]);

            var address = array + (wordIndex * 2);

            context.WriteWord(address, value);
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Stack routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine pull = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var varIdx = (ushort)context.GetOperandValue(i.Operands[0]);
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

            var objNum = (ushort)context.GetOperandValue(i.Operands[0]);
            var attrNum = (ushort)context.GetOperandValue(i.Operands[1]);

            context.ClearAttribute(objNum, attrNum);
        };

        public static readonly OpcodeRoutine get_child = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);
            Strict.HasBranch(i);
            Strict.HasStoreVariable(i);

            var objNum = (ushort)context.GetOperandValue(i.Operands[0]);

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

            context.WriteVariable(i.StoreVariable, Value.Number((ushort)childNum));

            if ((childNum > 0) == i.Branch.Condition)
            {
                context.Jump(i.Branch);
            }
        };

        public static readonly OpcodeRoutine get_next_prop = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var objNum = (ushort)context.GetOperandValue(i.Operands[0]);
            var propNum = (ushort)context.GetOperandValue(i.Operands[1]);

            var nextPropNum = context.GetNextProperty(objNum, propNum);

            context.WriteVariable(i.StoreVariable, Value.Number((ushort)nextPropNum));
        };

        public static readonly OpcodeRoutine get_parent = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);
            Strict.HasStoreVariable(i);

            var objNum = (ushort)context.GetOperandValue(i.Operands[0]);

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

            context.WriteVariable(i.StoreVariable, Value.Number((ushort)parentNum));
        };

        public static readonly OpcodeRoutine get_prop = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var objNum = (ushort)context.GetOperandValue(i.Operands[0]);
            var propNum = (ushort)context.GetOperandValue(i.Operands[1]);

            var value = context.GetPropertyData(objNum, propNum);

            context.WriteVariable(i.StoreVariable, Value.Number((ushort)value));
        };

        public static readonly OpcodeRoutine get_prop_addr = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasStoreVariable(i);

            var objNum = (ushort)context.GetOperandValue(i.Operands[0]);
            var propNum = (ushort)context.GetOperandValue(i.Operands[1]);

            var propAddress = context.GetPropertyDataAddress(objNum, propNum);

            context.WriteVariable(i.StoreVariable, Value.Number((ushort)propAddress));
        };

        public static readonly OpcodeRoutine get_prop_len = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);
            Strict.HasStoreVariable(i);

            var dataAddress = (ushort)context.GetOperandValue(i.Operands[0]);

            var propLen = context.GetPropertyDataLength(dataAddress);

            context.WriteVariable(i.StoreVariable, Value.Number((ushort)propLen));
        };

        public static readonly OpcodeRoutine get_sibling = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);
            Strict.HasBranch(i);
            Strict.HasStoreVariable(i);

            var objNum = (ushort)context.GetOperandValue(i.Operands[0]);

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

            context.WriteVariable(i.StoreVariable, Value.Number((ushort)siblingNum));

            if ((siblingNum > 0) == i.Branch.Condition)
            {
                context.Jump(i.Branch);
            }
        };

        public static readonly OpcodeRoutine insert_obj = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);

            var objNum = (ushort)context.GetOperandValue(i.Operands[0]);
            var destNum = (ushort)context.GetOperandValue(i.Operands[1]);

            context.MoveTo(objNum, destNum);
        };

        public static readonly OpcodeRoutine put_prop = (i, context) =>
        {
            Strict.OperandCountIs(i, 3);

            var objNum = (ushort)context.GetOperandValue(i.Operands[0]);
            var propNum = (ushort)context.GetOperandValue(i.Operands[1]);
            var value = (ushort)context.GetOperandValue(i.Operands[2]);

            context.WriteProperty(objNum, propNum, value);
        };

        public static readonly OpcodeRoutine remove_obj = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var objNum = (ushort)context.GetOperandValue(i.Operands[0]);

            context.RemoveFromParent(objNum);
        };

        public static readonly OpcodeRoutine set_attr = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);

            var objNum = (ushort)context.GetOperandValue(i.Operands[0]);
            var attrNum = (ushort)context.GetOperandValue(i.Operands[1]);

            context.SetAttribute(objNum, attrNum);
        };

        public static readonly OpcodeRoutine test_attr = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);
            Strict.HasBranch(i);

            var objNum = (ushort)context.GetOperandValue(i.Operands[0]);
            var attrNum = (ushort)context.GetOperandValue(i.Operands[1]);

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

            var flag = (ushort)context.GetOperandValue(i.Operands[0]);

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
                    var address = (ushort)context.GetOperandValue(i.Operands[1]);
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

            var byteAddress = (ushort)context.GetOperandValue(i.Operands[0]);

            var zwords = context.ReadZWords(byteAddress);
            var ztext = context.ParseZWords(zwords);

            context.Print(ztext);
        };

        public static readonly OpcodeRoutine print_char = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var ch = (char)(ushort)context.GetOperandValue(i.Operands[0]);
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

            var objNum = (ushort)context.GetOperandValue(i.Operands[0]);

            var shortName = context.GetShortName(objNum);
            context.Print(shortName);
        };

        public static readonly OpcodeRoutine print_paddr = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var byteAddress = (ushort)context.GetOperandValue(i.Operands[0]);
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
            context.Return(Value.One);
        };

        public static readonly OpcodeRoutine set_color = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);

            var foreground = (ZColor)(ushort)context.GetOperandValue(i.Operands[0]);
            var background = (ZColor)(ushort)context.GetOperandValue(i.Operands[1]);

            if (foreground != 0)
            {
                context.Screen.SetForegroundColor(foreground);
            }

            if (background != 0)
            {
                context.Screen.SetBackgroundColor(background);
            }
        };

        public static readonly OpcodeRoutine set_text_style = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var textStyle = (ZTextStyle)(ushort)context.GetOperandValue(i.Operands[0]);

            context.Screen.SetTextStyle(textStyle);
        };

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Input routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static readonly OpcodeRoutine read_char = (i, context) =>
        {
            Strict.OperandCountInRange(i, 1, 3);
            Strict.HasStoreVariable(i);

            var inputStream = (ushort)context.GetOperandValue(i.Operands[0]);

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
                context.WriteVariable(i.StoreVariable, Value.Number((ushort)ch));
            };

            context.ReadChar(callback);
        };

        public static readonly OpcodeRoutine sread1 = (i, context) =>
        {
            Strict.OperandCountIs(i, 2);

            var textBuffer = (ushort)context.GetOperandValue(i.Operands[0]);
            var parseBuffer = (ushort)context.GetOperandValue(i.Operands[1]);

            context.Screen.ShowStatus();

            throw new NotImplementedException();
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

            var line = (ushort)context.GetOperandValue(i.Operands[0]);
            var column = (ushort)context.GetOperandValue(i.Operands[1]);

            context.Screen.SetCursor(line - 1, column - 1);
        };

        public static readonly OpcodeRoutine set_window = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var window = (ushort)context.GetOperandValue(i.Operands[0]);

            context.Screen.SetWindow(window);
        };

        public static readonly OpcodeRoutine split_window = (i, context) =>
        {
            Strict.OperandCountIs(i, 1);

            var height = (ushort)context.GetOperandValue(i.Operands[0]);

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

            var argNumber = (ushort)context.GetOperandValue(i.Operands[0]);
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
                context.WriteVariable(i.StoreVariable, Value.Number((ushort)result));
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
