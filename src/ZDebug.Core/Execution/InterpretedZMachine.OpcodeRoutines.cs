using System;
using ZDebug.Core.Basics;
using ZDebug.Core.Extensions;
using ZDebug.Core.Instructions;
using ZDebug.Core.Text;

namespace ZDebug.Core.Execution;

public sealed partial class InterpretedZMachine
{
    ///////////////////////////////////////////////////////////////////////////////////////////
    // Arithmetic routines
    ///////////////////////////////////////////////////////////////////////////////////////////

    internal void op_add() => Store((ushort)((short)operandValues[0] + (short)operandValues[1]));

    internal void op_div() => Store((ushort)((short)operandValues[0] / (short)operandValues[1]));

    internal void op_mod() => Store((ushort)((short)operandValues[0] % (short)operandValues[1]));

    internal void op_mul() => Store((ushort)((short)operandValues[0] * (short)operandValues[1]));

    internal void op_sub() => Store((ushort)((short)operandValues[0] - (short)operandValues[1]));

    ///////////////////////////////////////////////////////////////////////////////////////////
    // Bit-level routines
    ///////////////////////////////////////////////////////////////////////////////////////////

    internal void op_and() => Store((ushort)(operandValues[0] & operandValues[1]));

    internal void op_art_shift()
    {
        var number = (short)operandValues[0];
        var places = (int)(short)operandValues[1];

        var result = places > 0
            ? (ushort)(number << places)
            : (ushort)(number >> -places);

        Store(result);
    }

    internal void op_log_shift()
    {
        var number = operandValues[0];
        var places = (int)(short)operandValues[1];

        var result = places > 0
            ? (ushort)(number << places)
            : (ushort)(number >> -places);

        Store((ushort)result);
    }

    internal void op_not() => Store((ushort)~operandValues[0]);

    internal void op_or() => Store((ushort)(operandValues[0] | operandValues[1]));

    internal void op_test()
    {
        var flags = operandValues[1];

        Branch((operandValues[0] & flags) == flags);
    }

    ///////////////////////////////////////////////////////////////////////////////////////////
    // Increment/decrement routines
    ///////////////////////////////////////////////////////////////////////////////////////////

    internal void op_dec()
    {
        var varIdx = (byte)operandValues[0];

        var value = (short)ReadVariableValueIndirectly(varIdx);
        value -= 1;

        WriteVariableValueIndirectly(varIdx, (ushort)value);
    }

    internal void op_dec_chk()
    {
        var varIdx = (byte)operandValues[0];

        var value = (short)ReadVariableValueIndirectly(varIdx);
        value -= 1;

        WriteVariableValueIndirectly(varIdx, (ushort)value);

        Branch(value < (short)operandValues[1]);
    }

    internal void op_inc()
    {
        var varIdx = (byte)operandValues[0];

        var value = (short)ReadVariableValueIndirectly(varIdx);
        value += 1;

        WriteVariableValueIndirectly(varIdx, (ushort)value);
    }

    internal void op_inc_chk()
    {
        var varIdx = (byte)operandValues[0];

        var value = (short)ReadVariableValueIndirectly(varIdx);
        value += 1;

        WriteVariableValueIndirectly(varIdx, (ushort)value);

        Branch(value > (short)operandValues[1]);
    }

    ///////////////////////////////////////////////////////////////////////////////////////////
    // Jump routines
    ///////////////////////////////////////////////////////////////////////////////////////////

    internal void op_je()
    {
        var x = operandValues[0];

        var result = false;
        for (var i = 1; i < operandCount; i++)
        {
            if (x == operandValues[i])
            {
                result = true;
                break;
            }
        }

        Branch(result);
    }

    internal void op_jg() => Branch((short)operandValues[0] > (short)operandValues[1]);

    internal void op_jin()
    {
        var obj1 = operandValues[0];
        var obj2 = operandValues[1];

        if (obj1 == 0)
        {
            MessageLog.SendWarning(opcode, startAddress, "called with object 0");
            Branch(obj2 == 0);
            return;
        }

        var obj1Parent = Story.ObjectTable.ReadParentNumberByObjectNumber(obj1);

        Branch(obj1Parent == obj2);
    }

    internal void op_jl() => Branch((short)operandValues[0] < (short)operandValues[1]);

    internal void op_jump() => pc += (short)operandValues[0] - 2;

    internal void op_jz() => Branch(operandValues[0] == 0);

    ///////////////////////////////////////////////////////////////////////////////////////////
    // Call routines
    ///////////////////////////////////////////////////////////////////////////////////////////

    internal void op_call_n() => Call(Story.UnpackRoutineAddress(operandValues[0]));

    internal void op_call_s()
    {
        var pc = this.pc;
        var storeVariable = Memory[pc];
        this.pc = pc + 1;
        Call(Story.UnpackRoutineAddress(operandValues[0]), storeVariable);
    }

    ///////////////////////////////////////////////////////////////////////////////////////////
    // Return routines
    ///////////////////////////////////////////////////////////////////////////////////////////

    internal void op_ret() => Return(operandValues[0]);

    internal void op_ret_popped() => Return(ReadVariableValue(0x00)); // read stack

    internal void op_rfalse() => Return(0);

    internal void op_rtrue() => Return(1);

    ///////////////////////////////////////////////////////////////////////////////////////////
    // Load/Store routines
    ///////////////////////////////////////////////////////////////////////////////////////////

    internal void op_load() => Store(ReadVariableValueIndirectly((byte)operandValues[0]));

    internal void op_loadb()
    {
        var address = operandValues[0] + operandValues[1];

        Store(Memory[address]);
    }

    internal void op_loadw()
    {
        var address = operandValues[0] + (operandValues[1] * 2);

        Store(Memory.ReadWord(address));
    }

    internal void op_store() => WriteVariableValueIndirectly((byte)operandValues[0], operandValues[1]);

    internal void op_storeb()
    {
        var address = operandValues[0] + operandValues[1];

        Memory[address] = (byte)operandValues[2];
    }

    internal void op_storew()
    {
        var address = operandValues[0] + (operandValues[1] * 2);

        Memory.WriteWord(address, operandValues[2]);
    }

    ///////////////////////////////////////////////////////////////////////////////////////////
    // Table routines
    ///////////////////////////////////////////////////////////////////////////////////////////

    internal void op_copy_table()
    {
        var first = operandValues[0];
        var second = operandValues[1];
        var size = operandValues[2];

        if (second == 0) // zero out first table
        {
            for (var j = 0; j < size; j++)
            {
                Memory.WriteByte(first + j, 0);
            }
        }
        else if ((short)size < 0 || first > second) // copy forwards
        {
            var copySize = size;
            if ((short)copySize < 0)
            {
                copySize = (ushort)-(short)size;
            }

            for (var j = 0; j < copySize; j++)
            {
                var value = Memory.ReadByte(first + j);
                Memory.WriteByte(second + j, value);
            }
        }
        else // copy backwards
        {
            for (var j = size - 1; j >= 0; j--)
            {
                var value = Memory.ReadByte(first + j);
                Memory.WriteByte(second + j, value);
            }
        }
    }

    internal void op_scan_table()
    {
        var x = operandValues[0];
        var table = operandValues[1];
        var len = operandValues[2];
        var form = operandCount > 3 ? operandValues[3] : (ushort)0x82;

        var address = table;

        for (var j = 0; j < len; j++)
        {
            if ((form & 0x80) != 0)
            {
                var value = Memory.ReadWord(address);
                if (value == x)
                {
                    Store(address);
                    Branch(true);
                    return;
                }
            }
            else
            {
                var value = Memory[address];
                if (value == x)
                {
                    Store(address);
                    Branch(true);
                    return;
                }
            }

            address += (ushort)(form & 0x7f);
        }

        Store(0);
        Branch(false);
    }

    ///////////////////////////////////////////////////////////////////////////////////////////
    // Stack routines
    ///////////////////////////////////////////////////////////////////////////////////////////

    internal void op_pull()
    {
        var varIdx = (byte)operandValues[0];

        var value = ReadVariableValue(0x00); // stack

        WriteVariableValueIndirectly(varIdx, value);
    }

    internal void op_push()
    {
        var value = operandValues[0];

        WriteVariableValue(0x00, value); // stack
    }

    ///////////////////////////////////////////////////////////////////////////////////////////
    // Object routines
    ///////////////////////////////////////////////////////////////////////////////////////////

    internal void op_clear_attr()
    {
        var objNum = operandValues[0];

        if (objNum == 0)
        {
            MessageLog.SendWarning(opcode, startAddress, "called with object 0");
            return;
        }

        var attrNum = (byte)operandValues[1];

        Story.ObjectTable.SetAttributeValueByObjectNumber(objNum, attrNum, false);
    }

    internal void op_get_child()
    {
        var objNum = operandValues[0];

        if (objNum == 0)
        {
            MessageLog.SendWarning(opcode, startAddress, "called with object 0");
            Store(0);
            Branch(false);
            return;
        }

        var childNum = Story.ObjectTable.ReadChildNumberByObjectNumber(objNum);

        Store(childNum);
        Branch(childNum > 0);
    }

    internal void op_get_next_prop()
    {
        var objNum = operandValues[0];

        if (objNum == 0)
        {
            MessageLog.SendWarning(opcode, startAddress, "called with object 0");
            Store(0);
            return;
        }

        var propNum = operandValues[1];

        var mask = (byte)(Version <= 3 ? 0x1f : 0x3f);
        byte value;

        var propAddress = GetFirstProperty(objNum);

        if (propNum != 0)
        {
            do
            {
                value = Memory[propAddress];
                propAddress = GetNextProperty(propAddress);
            }
            while ((value & mask) > propNum);

            if ((value & mask) != propNum)
            {
                throw new InvalidOperationException();
            }
        }

        value = Memory[propAddress];
        Store((ushort)(value & mask));
    }

    internal void op_get_parent()
    {
        var objNum = operandValues[0];

        if (objNum == 0)
        {
            MessageLog.SendWarning(opcode, startAddress, "called with object 0");
            Store(0);
            return;
        }

        var parentNum = Story.ObjectTable.ReadParentNumberByObjectNumber(objNum);

        Store(parentNum);
    }

    internal void op_get_prop()
    {
        var objNum = operandValues[0];

        if (objNum == 0)
        {
            MessageLog.SendWarning(opcode, startAddress, "called with object 0");
            Store(0);
            return;
        }

        var propNum = operandValues[1];

        var mask = (byte)(Version <= 3 ? 0x1f : 0x3f);
        byte value;

        var propAddress = GetFirstProperty(objNum);
        while (true)
        {
            value = Memory[propAddress];
            if ((value & mask) <= propNum)
            {
                break;
            }

            propAddress = GetNextProperty(propAddress);
        }

        ushort result;
        if ((value & mask) == propNum)
        {
            propAddress++;

            if ((Version <= 3 && (value & 0xe0) == 0) || (Version >= 4 && (value & 0xc0) == 0))
            {
                result = Memory[propAddress];
            }
            else
            {
                result = Memory.ReadWord(propAddress);
            }
        }
        else
        {
            propAddress = (ushort)(objectTableAddress + ((propNum - 1) * 2));
            result = Memory.ReadWord(propAddress);
        }

        Store(result);
    }

    internal void op_get_prop_addr()
    {
        var objNum = operandValues[0];

        if (objNum == 0)
        {
            MessageLog.SendWarning(opcode, startAddress, "called with object 0");
            Store(0);
            return;
        }

        var propNum = operandValues[1];

        var mask = (byte)(Version <= 3 ? 0x1f : 0x3f);
        byte value;

        var propAddress = GetFirstProperty(objNum);
        while (true)
        {
            value = Memory[propAddress];
            if ((value & mask) <= propNum)
            {
                break;
            }

            propAddress = GetNextProperty(propAddress);
        }

        if ((value & mask) == propNum)
        {
            if (Version >= 4 && (value & 0x80) != 0)
            {
                propAddress++;
            }

            Store(++propAddress);
        }
        else
        {
            Store(0);
        }
    }

    internal void op_get_prop_len()
    {
        var dataAddress = operandValues[0];

        if (dataAddress == 0)
        {
            Store(0);
            return;
        }

        dataAddress--;
        var value = Memory[dataAddress];

        if (Version <= 3)
        {
            value = (byte)((value >> 5) + 1);
        }
        else if ((value & 0x80) == 0)
        {
            value = (byte)((value >> 6) + 1);
        }
        else
        {
            value &= 0x3f;
        }

        if (value == 0)
        {
            value = 64;
        }

        Store(value);
    }

    internal void op_get_sibling()
    {
        var objNum = operandValues[0];

        if (objNum == 0)
        {
            MessageLog.SendWarning(opcode, startAddress, "called with object 0");
            Store(0);
            Branch(false);
            return;
        }

        var siblingNum = Story.ObjectTable.ReadSiblingNumberByObjectNumber(objNum);

        Store(siblingNum);
        Branch(siblingNum > 0);
    }

    internal void op_insert_obj()
    {
        var objNum = operandValues[0];

        if (objNum == 0)
        {
            MessageLog.SendWarning(opcode, startAddress, "called with object 0");
            return;
        }

        var destNum = operandValues[1];

        if (destNum == 0)
        {
            MessageLog.SendWarning(opcode, startAddress, "called with object 0");
            return;
        }

        Story.ObjectTable.MoveObjectToDestinationByNumber(objNum, destNum);
    }

    internal void op_put_prop()
    {
        var objNum = operandValues[0];

        if (objNum == 0)
        {
            MessageLog.SendWarning(opcode, startAddress, "called with object 0");
            return;
        }

        var propNum = operandValues[1];

        var mask = (byte)(Version <= 3 ? 0x1f : 0x3f);
        byte value;

        var propAddress = GetFirstProperty(objNum);
        while (true)
        {
            value = Memory[propAddress];
            if ((value & mask) <= propNum)
            {
                break;
            }

            propAddress = GetNextProperty(propAddress);
        }

        if ((value & mask) != propNum)
        {
            throw new InvalidOperationException();
        }

        propAddress++;

        if ((Version <= 3 && (value & 0xe0) == 0) || ((Version >= 4) && (value & 0xc0) == 0))
        {
            Memory[propAddress] = (byte)operandValues[2];
        }
        else
        {
            Memory.WriteWord(propAddress, operandValues[2]);
        }
    }

    internal void op_remove_obj()
    {
        var objNum = operandValues[0];

        if (objNum == 0)
        {
            MessageLog.SendWarning(opcode, startAddress, "called with object 0");
            return;
        }

        Story.ObjectTable.RemoveObjectFromParentByNumber(objNum);
    }

    internal void op_set_attr()
    {
        var objNum = operandValues[0];

        if (objNum == 0)
        {
            MessageLog.SendWarning(opcode, startAddress, "called with object 0");
            return;
        }

        var attrNum = (byte)operandValues[1];

        Story.ObjectTable.SetAttributeValueByObjectNumber(objNum, attrNum, true);
    }

    internal void op_test_attr()
    {
        var objNum = operandValues[0];

        if (objNum == 0)
        {
            MessageLog.SendWarning(opcode, startAddress, "called with object 0");
            Branch(false);
            return;
        }

        var attrNum = (byte)operandValues[1];

        var result = Story.ObjectTable.HasAttributeByObjectNumber(objNum, attrNum);

        Branch(result);
    }

    ///////////////////////////////////////////////////////////////////////////////////////////
    // Output routines
    ///////////////////////////////////////////////////////////////////////////////////////////

    internal void op_buffer_mode() =>
        // TODO: What should we do with buffer_mode? Does it have any meaning?
        MessageLog.SendWarning(opcode, startAddress, "Unsupported");

    internal void op_new_line() => OutputStreams.Print('\n');

    internal void op_output_stream()
    {
        switch ((short)operandValues[0])
        {
            case 1:
                OutputStreams.SelectScreenStream();
                break;

            case 2:
                OutputStreams.SelectTranscriptStream();
                break;

            case 3:
                var address = operandValues[1];
                OutputStreams.SelectMemoryStream(address);
                break;

            case -1:
                OutputStreams.DeselectScreenStream();
                break;

            case -2:
                OutputStreams.DeselectTranscriptStream();
                break;

            case -3:
                OutputStreams.DeselectMemoryStream();
                break;

            case 4:
            case -4:
                MessageLog.SendError(opcode, startAddress, "stream 4 is non supported");
                break;

            default:
                MessageLog.SendError(opcode, startAddress, "Illegal stream value: {0}", operandValues[0]);
                break;
        }
    }

    internal void op_print()
    {
        var text = DecodeEmbeddedText();

        OutputStreams.Print(text);
    }

    internal void op_print_addr()
    {
        var byteAddress = operandValues[0];

        var zwords = ZText.ReadZWords(Memory, byteAddress);
        var text = ZText.ZWordsAsString(zwords, ZTextFlags.All);

        OutputStreams.Print(text);
    }

    internal void op_print_char()
    {
        var ch = (char)operandValues[0];
        OutputStreams.Print(ch);
    }

    internal void op_print_num()
    {
        var number = (short)operandValues[0];
        OutputStreams.Print(number.ToString());
    }

    internal void op_print_obj()
    {
        var objNum = operandValues[0];

        var obj = Story.ObjectTable.GetByNumber(objNum);
        OutputStreams.Print(obj.ShortName);
    }

    internal void op_print_paddr()
    {
        var byteAddress = operandValues[0];
        var address = Story.UnpackStringAddress(byteAddress);

        var zwords = ZText.ReadZWords(Memory, address);
        var text = ZText.ZWordsAsString(zwords, ZTextFlags.All);

        OutputStreams.Print(text);
    }

    internal void op_print_ret()
    {
        var text = DecodeEmbeddedText();
        OutputStreams.Print(text + "\n");
        Return(1);
    }

    internal void op_print_table()
    {
        var address = operandValues[0];
        var width = operandValues[1];
        var height = operandCount > 2
            ? operandValues[2]
            : (ushort)1;
        var skip = operandCount > 3
            ? operandValues[3]
            : (ushort)0;

        var left = Screen.GetCursorColumn();

        for (var i = 0; i < height; i++)
        {
            if (i != 0)
            {
                var y = Screen.GetCursorLine() + 1;
                Screen.SetCursor(y, left);
            }

            for (var j = 0; j < width; j++)
            {
                var ch = (char)Memory.ReadByte(address);
                address++;
                Screen.Print(ch);
            }

            address += skip;
        }
    }

    internal void op_set_color()
    {
        var foreground = (ZColor)operandValues[0];
        var background = (ZColor)operandValues[1];

        if (foreground != 0)
        {
            Screen.SetForegroundColor(foreground);
        }

        if (background != 0)
        {
            Screen.SetBackgroundColor(background);
        }
    }

    internal void op_set_font()
    {
        var font = (ZFont)operandValues[0];

        var oldFont = (ushort)Screen.SetFont(font);

        Store(oldFont);
    }

    internal void op_set_text_style()
    {
        var textStyle = (ZTextStyle)operandValues[0];

        Screen.SetTextStyle(textStyle);
    }

    ///////////////////////////////////////////////////////////////////////////////////////////
    // Input routines
    ///////////////////////////////////////////////////////////////////////////////////////////

    internal void op_read_char()
    {
        if (operandCount > 0)
        {
            var inputStream = operandValues[0];

            if (inputStream != 1)
            {
                MessageLog.SendWarning(opcode, startAddress, "expected first operand to be 1 but was " + inputStream);
            }
        }
        else
        {
            MessageLog.SendWarning(opcode, startAddress, "expected at least 1 operand.");
        }

        Screen.ReadChar(ch =>
        {
            Store((ushort)ch);
        });
    }

    internal void op_aread()
    {
        var textBuffer = operandValues[0];

        ushort parseBuffer = 0;
        if (operandCount > 1)
        {
            parseBuffer = operandValues[1];
        }

        // TODO: Support timed input

        if (operandCount > 2)
        {
            MessageLog.SendWarning(opcode, startAddress, "timed input was attempted but it is unsupported");
        }

        var maxChars = Memory.ReadByte(textBuffer);

        Screen.ReadCommand(maxChars, s =>
        {
            var text = s.ToLower();

            var existingTextCount = Memory.ReadByte(textBuffer + 1);

            Memory.WriteByte(textBuffer + existingTextCount + 1, (byte)text.Length);

            for (var i = 0; i < text.Length; i++)
            {
                Memory.WriteByte(textBuffer + existingTextCount + 2 + i, (byte)text[i]);
            }

            if (parseBuffer > 0)
            {
                // TODO: Use ztext.TokenizeLine.

                var dictionary = Header.ReadDictionaryAddress(Memory);

                var tokens = ZText.TokenizeCommand(text, dictionary);

                var maxWords = Memory.ReadByte(parseBuffer);
                var parsedWords = Math.Min(maxWords, (byte)tokens.Length);

                Memory.WriteByte(parseBuffer + 1, parsedWords);

                for (var i = 0; i < parsedWords; i++)
                {
                    var token = tokens[i];

                    var address = ZText.LookupWord(token.Text, dictionary);
                    if (address > 0)
                    {
                        Memory.WriteWord(parseBuffer + 2 + (i * 4), address);
                    }
                    else
                    {
                        Memory.WriteWord(parseBuffer + 2 + (i * 4), 0);
                    }

                    Memory.WriteByte(parseBuffer + 2 + (i * 4) + 2, (byte)token.Length);
                    Memory.WriteByte(parseBuffer + 2 + (i * 4) + 3, (byte)(token.Start + 2));
                }
            }

            // TODO: Update this when timed input is supported
            Store(10);
        });
    }

    internal void op_sread1()
    {
        var textBuffer = operandValues[0];
        var parseBuffer = operandValues[1];

        Screen.ShowStatus();

        var maxChars = Memory.ReadByte(textBuffer);

        Screen.ReadCommand(maxChars, s =>
        {
            var text = s.ToLower();

            for (var i = 0; i < text.Length; i++)
            {
                Memory.WriteByte(textBuffer + 1 + i, (byte)text[i]);
            }

            Memory.WriteByte(textBuffer + 1 + text.Length, 0);

            // TODO: Use ztext.TokenizeLine.

            var dictionary = Header.ReadDictionaryAddress(Memory);

            var tokens = ZText.TokenizeCommand(text, dictionary);

            var maxWords = Memory.ReadByte(parseBuffer);
            var parsedWords = Math.Min(maxWords, (byte)tokens.Length);

            Memory.WriteByte(parseBuffer + 1, parsedWords);

            for (var i = 0; i < parsedWords; i++)
            {
                var token = tokens[i];

                var address = ZText.LookupWord(token.Text, dictionary);
                if (address > 0)
                {
                    Memory.WriteWord(parseBuffer + 2 + (i * 4), address);
                }
                else
                {
                    Memory.WriteWord(parseBuffer + 2 + (i * 4), 0);
                }

                Memory.WriteByte(parseBuffer + 2 + (i * 4) + 2, (byte)token.Length);
                Memory.WriteByte(parseBuffer + 2 + (i * 4) + 3, (byte)(token.Start + 1));
            }
        });
    }

    internal void op_sread2()
    {
        var textBuffer = operandValues[0];
        var parseBuffer = operandValues[1];

        // TODO: Support timed input

        if (operandCount > 2)
        {
            MessageLog.SendWarning(opcode, startAddress, "timed input was attempted but it is unsupported");
        }

        // TODO: Do something with time and routine operands if provided.

        var maxChars = Memory.ReadByte(textBuffer);

        Screen.ReadCommand(maxChars, s =>
        {
            var text = s.ToLower();

            for (var i = 0; i < text.Length; i++)
            {
                Memory.WriteByte(textBuffer + 1 + i, (byte)text[i]);
            }

            Memory.WriteByte(textBuffer + 1 + text.Length, 0);

            // TODO: Use ztext.TokenizeLine.

            var dictionary = Header.ReadDictionaryAddress(Memory);

            var tokens = ZText.TokenizeCommand(text, dictionary);

            var maxWords = Memory.ReadByte(parseBuffer);
            var parsedWords = Math.Min(maxWords, (byte)tokens.Length);

            Memory.WriteByte(parseBuffer + 1, parsedWords);

            for (var i = 0; i < parsedWords; i++)
            {
                var token = tokens[i];

                var address = ZText.LookupWord(token.Text, dictionary);
                if (address > 0)
                {
                    Memory.WriteWord(parseBuffer + 2 + (i * 4), address);
                }
                else
                {
                    Memory.WriteWord(parseBuffer + 2 + (i * 4), 0);
                }

                Memory.WriteByte(parseBuffer + 2 + (i * 4) + 2, (byte)token.Length);
                Memory.WriteByte(parseBuffer + 2 + (i * 4) + 3, (byte)(token.Start + 1));
            }
        });
    }

    ///////////////////////////////////////////////////////////////////////////////////////////
    // Window routines
    ///////////////////////////////////////////////////////////////////////////////////////////

    internal void op_erase_window()
    {
        var window = (short)operandValues[0];

        if (window == -1 || window == -2)
        {
            Screen.ClearAll(unsplit: window == -1);
        }
        else
        {
            Screen.Clear(window);
        }
    }

    internal void op_set_cursor()
    {
        var line = operandValues[0];
        var column = operandValues[1];

        Screen.SetCursor(line - 1, column - 1);
    }

    internal void op_set_window()
    {
        var window = operandValues[0];

        Screen.SetWindow(window);
    }

    internal void op_split_window()
    {
        var height = operandValues[0];

        if (height > 0)
        {
            Screen.Split(height);
        }
        else
        {
            Screen.Unsplit();
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////
    // Miscellaneous routines
    ///////////////////////////////////////////////////////////////////////////////////////////

    internal void op_check_arg_count()
    {
        var argNumber = operandValues[0];

        Branch(argNumber <= argumentCount);
    }

    internal void op_piracy() => Branch(true);

    internal void op_quit()
    {
        var handler = Quit;
        if (handler != null)
        {
            handler(this, EventArgs.Empty);
        }
    }

    internal void op_random()
    {
        var range = (short)operandValues[0];

        if (range > 0)
        {
            // range should be inclusive, so we need to subtract 1 since System.Random.Next makes it exclusive
            const ushort minValue = 1;
            var maxValue = Math.Max(minValue, (ushort)(range - 1));
            var result = GenerateRandomNumber(minValue, maxValue);

            Store(result);
        }
        else if (range < 0)
        {
            SetRandomSeed(+range);
            Store(0);
        }
        else // range = 0s
        {
            SetRandomSeed((int)DateTime.Now.Ticks);
            Store(0);
        }
    }

    internal void op_restore_undo()
    {
        MessageLog.SendWarning(opcode, startAddress, "Undo is not supported.");

        Store(unchecked((ushort)-1));
    }

    internal void op_save_undo()
    {
        MessageLog.SendWarning(opcode, startAddress, "Undo is not supported.");

        Store(unchecked((ushort)-1));
    }

    internal void op_show_status() => Screen.ShowStatus();

    internal void op_sound_effect()
    {
        if (operandCount == 0)
        {
            MessageLog.SendError(opcode, startAddress, "Called without any operands.");
            SoundEngine.HighBeep();
        }
        else if (operandCount == 1)
        {
            var number = operandValues[0];
            if (number == 1)
            {
                SoundEngine.HighBeep();
            }
            else if (number == 2)
            {
                SoundEngine.LowBeep();
            }
            else
            {
                MessageLog.SendError(opcode, startAddress, "Sound effect {0} is not supported without additional operands", operandValues[0]);
            }
        }
        else
        {
            MessageLog.SendError(opcode, startAddress, "Sound effect {0} is not supported", operandValues[0]);
        }
    }

    internal void op_tokenize()
    {
        var textBuffer = operandValues[0];
        var parseBuffer = operandValues[1];

        var dictionary = operandCount > 2
            ? operandValues[2]
            : (ushort)0;

        var flag = operandCount > 3
            ? operandValues[3] != 0
            : false;

        ZText.TokenizeLine(textBuffer, parseBuffer, dictionary, flag);
    }

    internal void op_verify() => Branch(Story.ActualChecksum == Header.ReadChecksum(Memory));

    ///////////////////////////////////////////////////////////////////////////////////////////
    // Execute
    ///////////////////////////////////////////////////////////////////////////////////////////

    private void Execute()
    {
        // TODO: The logic below should really be generated

        switch (opcode.Kind)
        {
            case OpcodeKind.TwoOp:
                {
                    switch (opcode.Number)
                    {
                        case 0x01:
                            op_je();
                            return;

                        case 0x02:
                            op_jl();
                            return;

                        case 0x03:
                            op_jg();
                            return;

                        case 0x04:
                            op_dec_chk();
                            return;

                        case 0x05:
                            op_inc_chk();
                            return;

                        case 0x06:
                            op_jin();
                            return;

                        case 0x07:
                            op_test();
                            return;

                        case 0x08:
                            op_or();
                            return;

                        case 0x09:
                            op_and();
                            return;

                        case 0x0a:
                            op_test_attr();
                            return;

                        case 0x0b:
                            op_set_attr();
                            return;

                        case 0x0c:
                            op_clear_attr();
                            return;

                        case 0x0d:
                            op_store();
                            return;

                        case 0x0e:
                            op_insert_obj();
                            return;

                        case 0x0f:
                            op_loadw();
                            return;

                        case 0x10:
                            op_loadb();
                            return;

                        case 0x11:
                            op_get_prop();
                            return;

                        case 0x12:
                            op_get_prop_addr();
                            return;

                        case 0x13:
                            op_get_next_prop();
                            return;

                        case 0x14:
                            op_add();
                            return;

                        case 0x15:
                            op_sub();
                            return;

                        case 0x16:
                            op_mul();
                            return;

                        case 0x17:
                            op_div();
                            return;

                        case 0x18:
                            op_mod();
                            return;

                        case 0x19:
                            if (Version < 4)
                            {
                                break; // illegal
                            }

                            op_call_s();
                            return;

                        case 0x1a:
                            if (Version < 5)
                            {
                                break; // illegal
                            }

                            op_call_n();
                            return;

                        case 0x1b:
                            if (Version < 5 || Version == 6)
                            {
                                break; // illegal
                            }

                            op_set_color();
                            return;

                        case 0x1c:
                            break; // 'throw' unsupported
                    }
                }

                break;

            case OpcodeKind.OneOp:
                {
                    switch (opcode.Number)
                    {
                        case 0x00:
                            op_jz();
                            return;

                        case 0x01:
                            op_get_sibling();
                            return;

                        case 0x02:
                            op_get_child();
                            return;

                        case 0x03:
                            op_get_parent();
                            return;

                        case 0x04:
                            op_get_prop_len();
                            return;

                        case 0x05:
                            op_inc();
                            return;

                        case 0x06:
                            op_dec();
                            return;

                        case 0x07:
                            op_print_addr();
                            return;

                        case 0x08:
                            if (Version < 4)
                            {
                                break; // illegal
                            }

                            op_call_s();
                            return;

                        case 0x09:
                            op_remove_obj();
                            return;

                        case 0x0a:
                            op_print_obj();
                            return;

                        case 0x0b:
                            op_ret();
                            return;

                        case 0x0c:
                            op_jump();
                            return;

                        case 0x0d:
                            op_print_paddr();
                            return;

                        case 0x0e:
                            op_load();
                            return;

                        case 0x0f:
                            if (Version < 5)
                            {
                                break; // 'not' unsupported
                            }

                            op_call_n();
                            return;
                    }
                }

                break;

            case OpcodeKind.ZeroOp:
                {
                    switch (opcode.Number)
                    {
                        case 0x00:
                            op_rtrue();
                            return;

                        case 0x01:
                            op_rfalse();
                            return;

                        case 0x02:
                            op_print();
                            return;

                        case 0x03:
                            op_print_ret();
                            return;

                        case 0x04:
                            break; // 'nop' unsupported

                        case 0x05:
                            break; // 'save' unsupported

                        case 0x06:
                            break; // 'restore' unsupported

                        case 0x07:
                            break; // 'restart' unsupported

                        case 0x08:
                            op_ret_popped();
                            return;

                        case 0x09:
                            if (Version < 5)
                            {
                                break; // 'pop' unsupported
                            }
                            else
                            {
                                break; // 'catch' unsupported
                            }

                        case 0x0a:
                            op_quit();
                            return;

                        case 0x0b:
                            op_new_line();
                            return;

                        case 0x0c:
                            if (Version == 3)
                            {
                                op_show_status();
                                return;
                            }
                            else
                            {
                                break; // illegal
                            }

                        case 0x0d:
                            if (Version < 3)
                            {
                                break; // illegal
                            }

                            op_verify();
                            return;

                        case 0x0e:
                            break; // first byte of extended opcode -- should never hit this

                        case 0x0f:
                            if (Version < 5)
                            {
                                break; // illegal
                            }

                            op_piracy();
                            return;
                    }
                }

                break;

            case OpcodeKind.VarOp:
                {
                    switch (opcode.Number)
                    {
                        case 0x00:
                            op_call_s();
                            return;

                        case 0x01:
                            op_storew();
                            return;

                        case 0x02:
                            op_storeb();
                            return;

                        case 0x03:
                            op_put_prop();
                            return;

                        case 0x04:
                            if (Version < 4)
                            {
                                op_sread1();
                            }
                            else if (Version == 4)
                            {
                                op_sread2();
                            }
                            else
                            {
                                op_aread();
                            }

                            return;

                        case 0x05:
                            op_print_char();
                            return;

                        case 0x06:
                            op_print_num();
                            return;

                        case 0x07:
                            op_random();
                            return;

                        case 0x08:
                            op_push();
                            return;

                        case 0x09:
                            if (Version == 6)
                            {
                                break; // 'pull' stack unsupported
                            }

                            op_pull();
                            return;

                        case 0x0a:
                            if (Version < 3)
                            {
                                break; // 'split_window' illegal
                            }

                            op_split_window();
                            return;

                        case 0x0b:
                            if (Version < 3)
                            {
                                break; // 'set_window' illegal
                            }

                            op_set_window();
                            return;

                        case 0x0c:
                            if (Version < 4)
                            {
                                break; // 'call_vs2' illegal
                            }

                            op_call_s();
                            return;

                        case 0x0d:
                            if (Version < 4)
                            {
                                break; // 'erase_window' illegal
                            }

                            op_erase_window();
                            return;

                        case 0x0e:
                            if (Version < 4)
                            {
                                break; // 'erase_line' illegal
                            }
                            else
                            {
                                break; // 'erase_line' unsupported
                            }

                        case 0x0f:
                            if (Version < 4)
                            {
                                break; // 'set_cursor' illegal
                            }
                            else if (Version == 6)
                            {
                                break; // 'set_cursor' unsupported
                            }

                            op_set_cursor();
                            return;

                        case 0x10:
                            break; // 'get_cursor' unsupported;

                        case 0x11:
                            if (Version < 4)
                            {
                                break; // 'set_text_style' illegal
                            }

                            op_set_text_style();
                            return;

                        case 0x12:
                            if (Version < 4)
                            {
                                break; // 'buffer_mode' illegal
                            }

                            op_buffer_mode();
                            return;

                        case 0x13:
                            if (Version < 3)
                            {
                                break; // 'output_stream' illegal
                            }
                            else if (Version == 6)
                            {
                                break; // 'output_stream' unsupported
                            }

                            op_output_stream();
                            return;

                        case 0x14:
                            if (Version < 3)
                            {
                                break; // 'input_stream' illegal
                            }
                            else
                            {
                                break; // 'input_stream' unsupported
                            }

                        case 0x15:
                            if (Version < 3)
                            {
                                break; // 'sound_effect' illegal
                            }
                            else
                            {
                                op_sound_effect();
                                return;
                            }

                        case 0x16:
                            if (Version < 4)
                            {
                                break; // 'read_char' illegal
                            }

                            op_read_char();
                            return;

                        case 0x17:
                            if (Version < 4)
                            {
                                break; // 'scan_table' illegal
                            }

                            op_scan_table();
                            return;

                        case 0x18:
                            if (Version < 5)
                            {
                                break; // 'not' illegal
                            }

                            op_not();
                            return;

                        case 0x19:
                            if (Version < 5)
                            {
                                break; // 'call_vn' illegal
                            }

                            op_call_n();
                            return;

                        case 0x1a:
                            if (Version < 5)
                            {
                                break; // 'call_vn2' illegal
                            }

                            op_call_n();
                            return;

                        case 0x1b:
                            if (Version < 5)
                            {
                                break; // 'tokenize' illegal
                            }

                            op_tokenize();
                            return;

                        case 0x1c:
                            if (Version < 5)
                            {
                                break; // 'encode_text' illegal
                            }
                            else
                            {
                                break; // encode_text unsupported
                            }

                        case 0x1d:
                            if (Version < 5)
                            {
                                break; // 'copy_table' illegal
                            }

                            op_copy_table();
                            return;

                        case 0x1e:
                            if (Version < 5)
                            {
                                break; // 'print_table' illegal
                            }

                            op_print_table();
                            return;

                        case 0x1f:
                            if (Version < 5)
                            {
                                break; // 'check_arg_count' illegal
                            }

                            op_check_arg_count();
                            return;
                    }
                }

                break;

            case OpcodeKind.Ext:
                {
                    switch (opcode.Number)
                    {
                        case 0x00:
                            if (Version < 5)
                            {
                                break; // 'save' illegal
                            }
                            else
                            {
                                break; // 'save' unsupported
                            }

                        case 0x01:
                            if (Version < 5)
                            {
                                break; // 'restore' illegal
                            }
                            else
                            {
                                break; // 'restore' unsupported
                            }

                        case 0x02:
                            if (Version < 5)
                            {
                                break; // 'log_shift' unsupported
                            }

                            op_log_shift();
                            return;

                        case 0x03:
                            if (Version < 5)
                            {
                                break; // 'art_shift' unsupported
                            }

                            op_art_shift();
                            return;

                        case 0x04:
                            if (Version < 5)
                            {
                                break; // 'set_font' unsupported
                            }

                            op_set_font();
                            return;

                        case 0x05:
                            if (Version != 6)
                            {
                                break; // 'draw_picture' illegal
                            }
                            else
                            {
                                break; // 'draw_picture' unsupported
                            }

                        case 0x06:
                            if (Version != 6)
                            {
                                break; // 'picture_data' illegal
                            }
                            else
                            {
                                break; // 'picture_data' unsupported
                            }

                        case 0x07:
                            if (Version != 6)
                            {
                                break; // 'erase_picture' illegal
                            }
                            else
                            {
                                break; // 'erase_picture' unsupported
                            }

                        case 0x08:
                            if (Version != 6)
                            {
                                break; // 'set_margins' illegal
                            }
                            else
                            {
                                break; // 'set_margins' unsupported
                            }

                        case 0x09:
                            if (Version < 5)
                            {
                                break; // 'save_undo' unsupported
                            }

                            op_save_undo();
                            return;

                        case 0x0a:
                            if (Version < 5)
                            {
                                break; // 'restore_undo' unsupported
                            }

                            op_restore_undo();
                            return;

                        case 0x0b:
                            if (Version < 5)
                            {
                                break; // 'print_unicode' illegal
                            }
                            else
                            {
                                break; // 'print_unicode' unsupported
                            }

                        case 0x0c:
                            if (Version < 5)
                            {
                                break; // 'check_unicode' illegal
                            }
                            else
                            {
                                break; // 'check_unicode' unsupported
                            }

                        case 0x10:
                            if (Version != 6)
                            {
                                break; // 'move_window' illegal
                            }
                            else
                            {
                                break; // 'move_window' unsupported
                            }

                        case 0x11:
                            if (Version != 6)
                            {
                                break; // 'window_size' illegal
                            }
                            else
                            {
                                break; // 'window_size' unsupported
                            }

                        case 0x12:
                            if (Version != 6)
                            {
                                break; // 'window_style' illegal
                            }
                            else
                            {
                                break; // 'window_style' unsupported
                            }

                        case 0x13:
                            if (Version != 6)
                            {
                                break; // 'get_wind_prop' illegal
                            }
                            else
                            {
                                break; // 'get_wind_prop' unsupported
                            }

                        case 0x14:
                            if (Version != 6)
                            {
                                break; // 'scroll_window' illegal
                            }
                            else
                            {
                                break; // 'scroll_window' unsupported
                            }

                        case 0x15:
                            if (Version != 6)
                            {
                                break; // 'pop_stack' illegal
                            }
                            else
                            {
                                break; // 'pop_stack' unsupported
                            }

                        case 0x16:
                            if (Version != 6)
                            {
                                break; // 'read_mouse' illegal
                            }
                            else
                            {
                                break; // 'read_mouse' unsupported
                            }

                        case 0x17:
                            if (Version != 6)
                            {
                                break; // 'mouse_window' illegal
                            }
                            else
                            {
                                break; // 'mouse_window' unsupported
                            }

                        case 0x18:
                            if (Version != 6)
                            {
                                break; // 'push_stack' illegal
                            }
                            else
                            {
                                break; // 'push_stack' unsupported
                            }

                        case 0x19:
                            if (Version != 6)
                            {
                                break; // 'put_wind_prop' illegal
                            }
                            else
                            {
                                break; // 'put_wind_prop' unsupported
                            }

                        case 0x1a:
                            if (Version != 6)
                            {
                                break; // 'print_form' illegal
                            }
                            else
                            {
                                break; // 'print_form' unsupported
                            }

                        case 0x1b:
                            if (Version != 6)
                            {
                                break; // 'make_menu' illegal
                            }
                            else
                            {
                                break; // 'make_menu' unsupported
                            }

                        case 0x1c:
                            if (Version != 6)
                            {
                                break; // 'picture_table' illegal
                            }
                            else
                            {
                                break; // 'picture_table' unsupported
                            }
                    }
                }

                break;
        }

        throw new InvalidOperationException(
            string.Format(
@"Routine does not exist for opcode '{0}'.

Kind = {1}
Number = {2:x2} ({2})", opcode.Name, opcode.Kind, opcode.Number));

    }
}
