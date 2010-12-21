using System;
using ZDebug.Core.Basics;
using ZDebug.Core.Instructions;
using ZDebug.Core.Objects;
using ZDebug.Core.Text;

namespace ZDebug.Core.Execution
{
    public sealed partial class Processor
    {
        ///////////////////////////////////////////////////////////////////////////////////////////
        // Arithmetic routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void op_add()
        {
            short x = (short)operandValues[0];
            short y = (short)operandValues[1];

            ushort result = (ushort)(x + y);

            WriteVariable(store, result);
        }

        private void op_div()
        {
            short x = (short)operandValues[0];
            short y = (short)operandValues[1];

            ushort result = (ushort)(x / y);

            WriteVariable(store, result);
        }

        private void op_mod()
        {
            short x = (short)operandValues[0];
            short y = (short)operandValues[1];

            ushort result = (ushort)(x % y);

            WriteVariable(store, result);
        }

        private void op_mul()
        {
            short x = (short)operandValues[0];
            short y = (short)operandValues[1];

            ushort result = (ushort)(x * y);

            WriteVariable(store, result);
        }

        private void op_sub()
        {
            short x = (short)operandValues[0];
            short y = (short)operandValues[1];

            ushort result = (ushort)(x - y);

            WriteVariable(store, result);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Bit-level routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void op_and()
        {
            ushort x = operandValues[0];
            ushort y = operandValues[1];

            ushort result = (ushort)(x & y);

            WriteVariable(store, result);
        }

        private void op_art_shift()
        {
            short number = (short)operandValues[0];
            int places = (int)(short)operandValues[1];

            ushort result = places > 0
                ? (ushort)(number << places)
                : (ushort)(number >> -places);

            WriteVariable(store, result);
        }

        private void op_log_shift()
        {
            ushort number = operandValues[0];
            int places = (int)(short)operandValues[1];

            ushort result = places > 0
                ? (ushort)(number << places)
                : (ushort)(number >> -places);

            WriteVariable(store, (ushort)result);
        }

        private void op_not()
        {
            ushort x = operandValues[0];

            ushort result = (ushort)(~x);

            WriteVariable(store, result);
        }

        private void op_or()
        {
            ushort x = operandValues[0];
            ushort y = operandValues[1];

            ushort result = (ushort)(x | y);

            WriteVariable(store, result);
        }

        private void op_test()
        {
            ushort bitmap = operandValues[0];
            ushort flags = operandValues[1];

            bool result = (bitmap & flags) == flags;

            if (result == branch.Condition)
            {
                Jump(branch);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Increment/decrement routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void op_dec()
        {
            byte varIdx = (byte)operandValues[0];
            Variable var = Variable.FromByte(varIdx);

            short value = (short)ReadVariable(var, indirect: true);
            value -= 1;

            WriteVariable(var, (ushort)value, indirect: true);
        }

        private void op_dec_chk()
        {
            byte varIdx = (byte)operandValues[0];
            Variable var = Variable.FromByte(varIdx);

            var test = (short)operandValues[1];

            short value = (short)ReadVariable(var, indirect: true);
            value -= 1;

            WriteVariable(var, (ushort)value, indirect: true);

            if ((value < test) == branch.Condition)
            {
                Jump(branch);
            }
        }

        private void op_inc()
        {
            byte varIdx = (byte)operandValues[0];
            Variable var = Variable.FromByte(varIdx);

            short value = (short)ReadVariable(var, indirect: true);
            value += 1;

            WriteVariable(var, (ushort)value, indirect: true);
        }

        private void op_inc_chk()
        {
            byte varIdx = (byte)operandValues[0];
            Variable var = Variable.FromByte(varIdx);

            var test = (short)operandValues[1];

            short value = (short)ReadVariable(var, indirect: true);
            value += 1;

            WriteVariable(var, (ushort)value, indirect: true);

            if ((value > test) == branch.Condition)
            {
                Jump(branch);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Jump routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void op_je()
        {
            ushort x = operandValues[0];

            bool result = false;
            for (int i = 1; i < operandCount; i++)
            {
                if (x == operandValues[i])
                {
                    result = true;
                    break;
                }
            }

            if (result == branch.Condition)
            {
                Jump(branch);
            }
        }

        private void op_jg()
        {
            short x = (short)operandValues[0];
            short y = (short)operandValues[1];

            if ((x > y) == branch.Condition)
            {
                Jump(branch);
            }
        }

        private void op_jin()
        {
            ushort obj1 = operandValues[0];
            ushort obj2 = operandValues[1];

            ushort obj1Parent = memory.ReadParentNumberByObjectNumber(obj1);

            if ((obj1Parent == obj2) == branch.Condition)
            {
                Jump(branch);
            }
        }

        private void op_jl()
        {
            short x = (short)operandValues[0];
            short y = (short)operandValues[1];

            if ((x < y) == branch.Condition)
            {
                Jump(branch);
            }
        }

        private void op_jump()
        {
            short offset = (short)operandValues[0];

            Jump(offset);
        }

        private void op_jz()
        {
            ushort x = operandValues[0];

            if ((x == 0) == branch.Condition)
            {
                Jump(branch);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Call routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void op_call_1n()
        {
            ushort byteAddress = operandValues[0];
            int address = story.UnpackRoutineAddress(byteAddress);

            Call(address, operandValues, operandCount);
        }

        private void op_call_1s()
        {
            ushort byteAddress = operandValues[0];
            int address = story.UnpackRoutineAddress(byteAddress);

            Call(address, operandValues, operandCount, store);
        }

        private void op_call_2n()
        {
            ushort byteAddress = operandValues[0];
            int address = story.UnpackRoutineAddress(byteAddress);

            Call(address, operandValues, operandCount);
        }

        private void op_call_2s()
        {
            ushort byteAddress = operandValues[0];
            int address = story.UnpackRoutineAddress(byteAddress);

            Call(address, operandValues, operandCount, store);
        }

        private void op_call_vn()
        {
            ushort byteAddress = operandValues[0];
            int address = story.UnpackRoutineAddress(byteAddress);

            Call(address, operandValues, operandCount);
        }

        private void op_call_vs()
        {
            ushort byteAddress = operandValues[0];
            int address = story.UnpackRoutineAddress(byteAddress);

            Call(address, operandValues, operandCount, store);
        }

        private void op_call_vn2()
        {
            ushort byteAddress = operandValues[0];
            int address = story.UnpackRoutineAddress(byteAddress);

            Call(address, operandValues, operandCount);
        }

        private void op_call_vs2()
        {
            ushort byteAddress = operandValues[0];
            int address = story.UnpackRoutineAddress(byteAddress);

            Call(address, operandValues, operandCount, store);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Return routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void op_ret()
        {
            ushort value = operandValues[0];

            Return(value);
        }

        private void op_ret_popped()
        {
            ushort value = ReadVariable(Variable.Stack);

            Return(value);
        }

        private void op_rfalse()
        {
            Return(0);
        }

        private void op_rtrue()
        {
            Return(1);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Load/Store routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void op_load()
        {
            byte varIdx = (byte)operandValues[0];
            Variable var = Variable.FromByte(varIdx);

            ushort value = ReadVariable(var, indirect: true);

            WriteVariable(store, value);
        }

        private void op_loadb()
        {
            ushort array = operandValues[0];
            ushort byteIndex = operandValues[1];

            int address = array + byteIndex;
            byte value = memory.ReadByte(address);

            WriteVariable(store, value);
        }

        private void op_loadw()
        {
            ushort array = operandValues[0];
            ushort wordIndex = operandValues[1];

            int address = array + (wordIndex * 2);
            ushort value = memory.ReadWord(address);

            WriteVariable(store, value);
        }

        private void op_store()
        {
            byte varIdx = (byte)operandValues[0];
            Variable var = Variable.FromByte(varIdx);

            ushort value = operandValues[1];

            WriteVariable(var, value, indirect: true);
        }

        private void op_storeb()
        {
            ushort array = operandValues[0];
            ushort byteIndex = operandValues[1];
            byte value = (byte)operandValues[2];

            int address = array + byteIndex;

            memory.WriteByte(address, value);
        }

        private void op_storew()
        {
            ushort array = operandValues[0];
            ushort wordIndex = operandValues[1];
            ushort value = operandValues[2];

            int address = array + (wordIndex * 2);

            memory.WriteWord(address, value);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Table routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void op_copy_table()
        {
            ushort first = operandValues[0];
            ushort second = operandValues[1];
            ushort size = operandValues[2];

            if (second == 0) // zero out first table
            {
                for (int j = 0; j < size; j++)
                {
                    memory.WriteByte(first + j, 0);
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
                    var value = memory.ReadByte(first + j);
                    memory.WriteByte(second + j, value);
                }
            }
            else // copy backwards
            {
                for (int j = size - 1; j >= 0; j--)
                {
                    var value = memory.ReadByte(first + j);
                    memory.WriteByte(second + j, value);
                }
            }
        }

        private void op_scan_table()
        {
            // TODO: Rewrite to scan byte array directly

            ushort x = operandValues[0];
            ushort table = operandValues[1];
            ushort len = operandValues[2];

            ushort form = 0x82;
            if (operandCount > 3)
            {
                form = operandValues[3];
            }

            ushort address = table;

            for (int j = 0; j < len; j++)
            {
                if ((form & 0x80) != 0)
                {
                    var value = memory.ReadWord(address);
                    if (value == x)
                    {
                        WriteVariable(store, address);

                        if (branch.Condition)
                        {
                            Jump(branch);
                        }

                        return;
                    }
                }
                else
                {
                    var value = memory.ReadByte(address);
                    if (value == x)
                    {
                        WriteVariable(store, address);

                        if (branch.Condition)
                        {
                            Jump(branch);
                        }

                        return;
                    }
                }

                address += (ushort)(form & 0x7f);
            }

            WriteVariable(store, 0);

            if (!branch.Condition)
            {
                Jump(branch);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Stack routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void op_pull()
        {
            byte varIdx = (byte)operandValues[0];
            Variable var = Variable.FromByte(varIdx);

            ushort value = ReadVariable(Variable.Stack);

            WriteVariable(var, value, indirect: true);
        }

        private void op_push()
        {
            ushort value = operandValues[0];

            WriteVariable(Variable.Stack, value);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Object routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void op_clear_attr()
        {
            ushort objNum = operandValues[0];
            ushort attrNum = operandValues[1];

            // TODO: Call into memory directly

            ZObject obj = objectTable.GetByNumber(objNum);
            obj.ClearAttribute(attrNum);
        }

        private void op_get_child()
        {
            ushort objNum = operandValues[0];

            ushort childNum;
            if (objNum > 0)
            {
                childNum = memory.ReadChildNumberByObjectNumber(objNum);
            }
            else
            {
                messageLog.SendWarning(opcode, startAddress, "called with object 0");
                childNum = 0;
            }

            WriteVariable(store, childNum);

            if ((childNum > 0) == branch.Condition)
            {
                Jump(branch);
            }
        }

        private void op_get_next_prop()
        {
            ushort objNum = operandValues[0];
            ushort propNum = operandValues[1];

            // TODO: Call into memory directly to find next property

            ZObject obj = this.objectTable.GetByNumber(objNum);

            int nextIndex = 0;
            if (propNum > 0)
            {
                var prop = obj.PropertyTable.GetByNumber(propNum);
                if (prop == null)
                {
                    throw new InvalidOperationException();
                }

                nextIndex = prop.Index + 1;
            }

            if (nextIndex < obj.PropertyTable.Count)
            {
                ushort nextPropNum = (ushort)obj.PropertyTable[nextIndex].Number;
                WriteVariable(store, nextPropNum);
            }
            else
            {
                WriteVariable(store, 0);
            }
        }

        private void op_get_parent()
        {
            ushort objNum = operandValues[0];

            ushort parentNum;
            if (objNum > 0)
            {
                parentNum = memory.ReadParentNumberByObjectNumber(objNum);
            }
            else
            {
                messageLog.SendWarning(opcode, startAddress, "called with object 0");
                parentNum = 0;
            }

            WriteVariable(store, parentNum);
        }

        private void op_get_prop()
        {
            ushort objNum = operandValues[0];
            ushort propNum = operandValues[1];

            ZObject obj = this.objectTable.GetByNumber(objNum);
            ZProperty prop = obj.PropertyTable.GetByNumber(propNum);

            ushort value;
            if (prop == null)
            {
                value = objectTable.GetPropertyDefault(propNum);
            }
            else if (prop.DataLength == 1)
            {
                value = memory.ReadByte(prop.DataAddress);
            }
            else if (prop.DataLength == 2)
            {
                value = memory.ReadWord(prop.DataAddress);
            }
            else
            {
                throw new InvalidOperationException();
            }

            WriteVariable(store, value);
        }

        private void op_get_prop_addr()
        {
            ushort objNum = operandValues[0];
            ushort propNum = operandValues[1];

            ZObject obj = this.objectTable.GetByNumber(objNum);
            ZProperty prop = obj.PropertyTable.GetByNumber(propNum);

            ushort propAddress = prop != null
                ? (ushort)prop.DataAddress
                : (ushort)0;

            WriteVariable(store, propAddress);
        }

        private void op_get_prop_len()
        {
            ushort dataAddress = operandValues[0];

            ushort propLen = (ushort)memory.ReadPropertyDataLength(dataAddress);

            WriteVariable(store, propLen);
        }

        private void op_get_sibling()
        {
            ushort objNum = operandValues[0];

            ushort siblingNum;
            if (objNum > 0)
            {
                siblingNum = memory.ReadSiblingNumberByObjectNumber(objNum);
            }
            else
            {
                messageLog.SendWarning(opcode, startAddress, "called with object 0");
                siblingNum = 0;
            }

            WriteVariable(store, siblingNum);

            if ((siblingNum > 0) == branch.Condition)
            {
                Jump(branch);
            }
        }

        private void op_insert_obj()
        {
            ushort objNum = operandValues[0];
            ushort destNum = operandValues[1];

            memory.MoveObjectToDestinationByNumber(objNum, destNum);
        }

        private void op_put_prop()
        {
            ushort objNum = operandValues[0];
            ushort propNum = operandValues[1];
            ushort value = operandValues[2];

            WriteProperty(objNum, propNum, value);
        }

        private void op_remove_obj()
        {
            ushort objNum = operandValues[0];

            memory.RemoveObjectFromParentByNumber(objNum);
        }

        private void op_set_attr()
        {
            ushort objNum = operandValues[0];
            ushort attrNum = operandValues[1];

            // TODO: Call into memory directly

            ZObject obj = objectTable.GetByNumber(objNum);
            obj.SetAttribute(attrNum);
        }

        private void op_test_attr()
        {
            ushort objNum = operandValues[0];
            ushort attrNum = operandValues[1];

            // TODO: Call into memory directly

            ZObject obj = this.objectTable.GetByNumber(objNum);
            bool result = obj.HasAttribute(attrNum);

            if (result == branch.Condition)
            {
                Jump(branch);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Output routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void op_buffer_mode()
        {
            // TODO: What should we do with buffer_mode? Does it have any meaning?
        }

        private void op_new_line()
        {
            outputStreams.Print('\n');
        }

        private void op_output_stream()
        {
            short number = (short)operandValues[0];

            switch (number)
            {
                case 1:
                    outputStreams.SelectScreenStream();
                    break;

                case 2:
                    outputStreams.SelectTranscriptStream();
                    break;

                case 3:
                    ushort address = operandValues[1];
                    outputStreams.SelectMemoryStream(memory, address);
                    break;

                case -1:
                    outputStreams.DeselectScreenStream();
                    break;

                case -2:
                    outputStreams.DeselectTranscriptStream();
                    break;

                case -3:
                    outputStreams.DeselectMemoryStream();
                    break;

                case 4:
                case -4:
                    messageLog.SendError(opcode, startAddress, "stream 4 is non supported");
                    break;
            }
        }

        private void op_print()
        {
            string text = ztext.ZWordsAsString(zwords, ZTextFlags.All);

            outputStreams.Print(text);
        }

        private void op_print_addr()
        {
            ushort byteAddress = operandValues[0];

            ushort[] zwords = memory.ReadZWords(byteAddress);
            string text = ztext.ZWordsAsString(zwords, ZTextFlags.All);

            outputStreams.Print(text);
        }

        private void op_print_char()
        {
            char ch = (char)operandValues[0];
            outputStreams.Print(ch);
        }

        private void op_print_num()
        {
            short number = (short)operandValues[0];
            outputStreams.Print(number.ToString());
        }

        private void op_print_obj()
        {
            ushort objNum = operandValues[0];

            ZObject obj = this.objectTable.GetByNumber(objNum);
            outputStreams.Print(obj.ShortName);
        }

        private void op_print_paddr()
        {
            ushort byteAddress = operandValues[0];
            int address = story.UnpackStringAddress(byteAddress);

            ushort[] zwords = memory.ReadZWords(byteAddress);
            string text = ztext.ZWordsAsString(zwords, ZTextFlags.All);

            outputStreams.Print(text);
        }

        private void op_print_ret()
        {
            string text = ztext.ZWordsAsString(zwords, ZTextFlags.All);
            outputStreams.Print(text + "\n");
            Return(1);
        }

        private void op_print_table()
        {
            ushort address = operandValues[0];
            ushort width = operandValues[1];
            ushort height = operandCount > 2
                ? operandValues[2]
                : (ushort)1;
            ushort skip = operandCount > 3
                ? operandValues[3]
                : (ushort)0;

            int left = screen.GetCursorColumn();

            for (int i = 0; i < height; i++)
            {
                if (i != 0)
                {
                    int y = screen.GetCursorLine() + 1;
                    screen.SetCursor(y, left);
                }

                for (int j = 0; j < width; j++)
                {
                    char ch = (char)memory.ReadByte(address);
                    address++;
                    screen.Print(ch);
                }

                address += skip;
            }

        }

        private void op_set_color()
        {
            ZColor foreground = (ZColor)operandValues[0];
            ZColor background = (ZColor)operandValues[1];

            if (foreground != 0)
            {
                screen.SetForegroundColor(foreground);
            }

            if (background != 0)
            {
                screen.SetBackgroundColor(background);
            }
        }

        private void op_set_font()
        {
            ZFont font = (ZFont)operandValues[0];

            ushort oldFont = (ushort)screen.SetFont(font);

            WriteVariable(store, oldFont);
        }

        private void op_set_text_style()
        {
            ZTextStyle textStyle = (ZTextStyle)operandValues[0];

            screen.SetTextStyle(textStyle);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Input routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void op_read_char()
        {
            if (operandCount > 0)
            {
                ushort inputStream = operandValues[0];

                if (inputStream != 1)
                {
                    messageLog.SendWarning(opcode, startAddress, "expected first operand to be 1 but was " + inputStream);
                }
            }
            else
            {
                messageLog.SendWarning(opcode, startAddress, "expected at least 1 operand.");
            }

            screen.ReadChar(ch =>
            {
                WriteVariable(store, (ushort)ch);
            });
        }

        private void op_aread()
        {
            ushort textBuffer = operandValues[0];

            ushort parseBuffer = 0;
            if (operandCount > 1)
            {
                parseBuffer = operandValues[1];
            }

            // TODO: Support timed input

            if (operandCount > 2)
            {
                messageLog.SendWarning(opcode, startAddress, "timed input was attempted but it is unsupported");
            }

            byte maxChars = memory.ReadByte(textBuffer);

            screen.ReadCommand(maxChars, s =>
            {
                string text = s.ToLower();

                byte existingTextCount = memory.ReadByte(textBuffer + 1);

                memory.WriteByte(textBuffer + existingTextCount + 1, (byte)text.Length);

                for (int i = 0; i < text.Length; i++)
                {
                    memory.WriteByte(textBuffer + existingTextCount + 2 + i, (byte)text[i]);
                }

                if (parseBuffer > 0)
                {
                    // TODO: Use ztext.TokenizeLine.

                    ushort dictionary = memory.ReadDictionaryAddress();

                    ZCommandToken[] tokens = ztext.TokenizeCommand(text, dictionary);

                    byte maxWords = memory.ReadByte(parseBuffer);
                    byte parsedWords = Math.Min(maxWords, (byte)tokens.Length);

                    memory.WriteByte(parseBuffer + 1, parsedWords);

                    for (int i = 0; i < parsedWords; i++)
                    {
                        ZCommandToken token = tokens[i];

                        ushort address = ztext.LookupWord(token.Text, dictionary);
                        if (address > 0)
                        {
                            memory.WriteWord(parseBuffer + 2 + (i * 4), address);
                        }
                        else
                        {
                            memory.WriteWord(parseBuffer + 2 + (i * 4), 0);
                        }

                        memory.WriteByte(parseBuffer + 2 + (i * 4) + 2, (byte)token.Length);
                        memory.WriteByte(parseBuffer + 2 + (i * 4) + 3, (byte)(token.Start + 2));
                    }
                }

                // TODO: Update this when timed input is supported
                WriteVariable(store, 10);
            });
        }

        private void op_sread1()
        {
            ushort textBuffer = operandValues[0];
            ushort parseBuffer = operandValues[1];

            screen.ShowStatus();

            byte maxChars = memory.ReadByte(textBuffer);

            screen.ReadCommand(maxChars, s =>
            {
                string text = s.ToLower();

                for (int i = 0; i < text.Length; i++)
                {
                    memory.WriteByte(textBuffer + 1 + i, (byte)text[i]);
                }

                memory.WriteByte(textBuffer + 1 + text.Length, 0);

                // TODO: Use ztext.TokenizeLine.

                ushort dictionary = memory.ReadDictionaryAddress();

                ZCommandToken[] tokens = ztext.TokenizeCommand(text, dictionary);

                byte maxWords = memory.ReadByte(parseBuffer);
                byte parsedWords = Math.Min(maxWords, (byte)tokens.Length);

                memory.WriteByte(parseBuffer + 1, parsedWords);

                for (int i = 0; i < parsedWords; i++)
                {
                    ZCommandToken token = tokens[i];

                    ushort address = ztext.LookupWord(token.Text, dictionary);
                    if (address > 0)
                    {
                        memory.WriteWord(parseBuffer + 2 + (i * 4), address);
                    }
                    else
                    {
                        memory.WriteWord(parseBuffer + 2 + (i * 4), 0);
                    }

                    memory.WriteByte(parseBuffer + 2 + (i * 4) + 2, (byte)token.Length);
                    memory.WriteByte(parseBuffer + 2 + (i * 4) + 3, (byte)(token.Start + 2));
                }
            });
        }

        private void op_sread2()
        {
            ushort textBuffer = operandValues[0];
            ushort parseBuffer = operandValues[1];

            // TODO: Support timed input

            if (operandCount > 2)
            {
                messageLog.SendWarning(opcode, startAddress, "timed input was attempted but it is unsupported");
            }

            // TODO: Do something with time and routine operands if provided.

            byte maxChars = memory.ReadByte(textBuffer);

            screen.ReadCommand(maxChars, s =>
            {
                string text = s.ToLower();

                for (int i = 0; i < text.Length; i++)
                {
                    memory.WriteByte(textBuffer + 1 + i, (byte)text[i]);
                }

                memory.WriteByte(textBuffer + 1 + text.Length, 0);

                // TODO: Use ztext.TokenizeLine.

                ushort dictionary = memory.ReadDictionaryAddress();

                ZCommandToken[] tokens = ztext.TokenizeCommand(text, dictionary);

                byte maxWords = memory.ReadByte(parseBuffer);
                byte parsedWords = Math.Min(maxWords, (byte)tokens.Length);

                memory.WriteByte(parseBuffer + 1, parsedWords);

                for (int i = 0; i < parsedWords; i++)
                {
                    ZCommandToken token = tokens[i];

                    ushort address = ztext.LookupWord(token.Text, dictionary);
                    if (address > 0)
                    {
                        memory.WriteWord(parseBuffer + 2 + (i * 4), address);
                    }
                    else
                    {
                        memory.WriteWord(parseBuffer + 2 + (i * 4), 0);
                    }

                    memory.WriteByte(parseBuffer + 2 + (i * 4) + 2, (byte)token.Length);
                    memory.WriteByte(parseBuffer + 2 + (i * 4) + 3, (byte)(token.Start + 2));
                }
            });
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Window routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void op_erase_window()
        {
            short window = (short)operandValues[0];

            if (window == -1 || window == -2)
            {
                screen.ClearAll(unsplit: window == -1);
            }
            else
            {
                screen.Clear(window);
            }
        }

        private void op_set_cursor()
        {
            ushort line = operandValues[0];
            ushort column = operandValues[1];

            screen.SetCursor(line - 1, column - 1);
        }

        private void op_set_window()
        {
            ushort window = operandValues[0];

            screen.SetWindow(window);
        }

        private void op_split_window()
        {
            ushort height = operandValues[0];

            if (height > 0)
            {
                screen.Split(height);
            }
            else
            {
                screen.Unsplit();
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Miscellaneous routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void op_check_arg_count()
        {
            ushort argNumber = operandValues[0];

            if ((argNumber <= argumentCount) == branch.Condition)
            {
                Jump(branch);
            }
        }

        private void op_piracy()
        {
            Jump(branch);
        }

        private void op_quit()
        {
            var handler = Quit;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void op_random()
        {
            short range = (short)operandValues[0];

            if (range > 0)
            {
                // range should be inclusive, so we need to subtract 1 since System.Random.Next makes it exclusive
                int minValue = 1;
                int maxValue = Math.Max(minValue, range - 1);
                int result = random.Next(minValue, maxValue);

                WriteVariable(store, (ushort)result);
            }
            else if (range < 0)
            {
                random = new Random(+range);
            }
            else // range = 0s
            {
                random = new Random((int)DateTime.Now.Ticks);
            }
        }

        private void op_restore_undo()
        {
            messageLog.SendWarning(opcode, startAddress, "Undo is not supported.");

            WriteVariable(store, unchecked((ushort)-1));
        }

        private void op_save_undo()
        {
            messageLog.SendWarning(opcode, startAddress, "Undo is not supported.");

            WriteVariable(store, unchecked((ushort)-1));
        }

        private void op_show_status()
        {
            screen.ShowStatus();
        }

        private void op_tokenize()
        {
            ushort textBuffer = operandValues[0];
            ushort parseBuffer = operandValues[1];

            ushort dictionary = operandCount > 2
                ? operandValues[2]
                : (ushort)0;

            bool flag = operandCount > 3
                ? operandValues[3] != 0
                : false;

            ztext.TokenizeLine(textBuffer, parseBuffer, dictionary, flag);
        }

        private void op_verify()
        {
            if ((story.ActualChecksum == story.Memory.ReadChecksum()) == branch.Condition)
            {
                Jump(branch);
            }
        }
    }
}
