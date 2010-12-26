using System;
using ZDebug.Core.Basics;
using ZDebug.Core.Instructions;
using ZDebug.Core.Objects;
using ZDebug.Core.Text;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Execution
{
    public sealed partial class Processor
    {
        ///////////////////////////////////////////////////////////////////////////////////////////
        // Arithmetic routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal void op_add()
        {
            short x = (short)operandValues[0];
            short y = (short)operandValues[1];

            ushort result = (ushort)(x + y);

            Store(result);
        }

        internal void op_div()
        {
            short x = (short)operandValues[0];
            short y = (short)operandValues[1];

            ushort result = (ushort)(x / y);

            Store(result);
        }

        internal void op_mod()
        {
            short x = (short)operandValues[0];
            short y = (short)operandValues[1];

            ushort result = (ushort)(x % y);

            Store(result);
        }

        internal void op_mul()
        {
            short x = (short)operandValues[0];
            short y = (short)operandValues[1];

            ushort result = (ushort)(x * y);

            Store(result);
        }

        internal void op_sub()
        {
            short x = (short)operandValues[0];
            short y = (short)operandValues[1];

            ushort result = (ushort)(x - y);

            Store(result);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Bit-level routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal void op_and()
        {
            ushort x = operandValues[0];
            ushort y = operandValues[1];

            ushort result = (ushort)(x & y);

            Store(result);
        }

        internal void op_art_shift()
        {
            short number = (short)operandValues[0];
            int places = (int)(short)operandValues[1];

            ushort result = places > 0
                ? (ushort)(number << places)
                : (ushort)(number >> -places);

            Store(result);
        }

        internal void op_log_shift()
        {
            ushort number = operandValues[0];
            int places = (int)(short)operandValues[1];

            ushort result = places > 0
                ? (ushort)(number << places)
                : (ushort)(number >> -places);

            Store((ushort)result);
        }

        internal void op_not()
        {
            ushort x = operandValues[0];

            ushort result = (ushort)(~x);

            Store(result);
        }

        internal void op_or()
        {
            ushort x = operandValues[0];
            ushort y = operandValues[1];

            ushort result = (ushort)(x | y);

            Store(result);
        }

        internal void op_test()
        {
            ushort bitmap = operandValues[0];
            ushort flags = operandValues[1];

            bool result = (bitmap & flags) == flags;

            Branch(result);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Increment/decrement routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal void op_dec()
        {
            byte varIdx = (byte)operandValues[0];

            short value = (short)ReadVariableValueIndirectly(varIdx);
            value -= 1;

            WriteVariableValueIndirectly(varIdx, (ushort)value);
        }

        internal void op_dec_chk()
        {
            byte varIdx = (byte)operandValues[0];

            var test = (short)operandValues[1];

            short value = (short)ReadVariableValueIndirectly(varIdx);
            value -= 1;

            WriteVariableValueIndirectly(varIdx, (ushort)value);

            Branch(value < test);
        }

        internal void op_inc()
        {
            byte varIdx = (byte)operandValues[0];

            short value = (short)ReadVariableValueIndirectly(varIdx);
            value += 1;

            WriteVariableValueIndirectly(varIdx, (ushort)value);
        }

        internal void op_inc_chk()
        {
            byte varIdx = (byte)operandValues[0];

            var test = (short)operandValues[1];

            short value = (short)ReadVariableValueIndirectly(varIdx);
            value += 1;

            WriteVariableValueIndirectly(varIdx, (ushort)value);

            Branch(value > test);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Jump routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal void op_je()
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

            Branch(result);
        }

        internal void op_jg()
        {
            short x = (short)operandValues[0];
            short y = (short)operandValues[1];

            Branch(x > y);
        }

        internal void op_jin()
        {
            ushort obj1 = operandValues[0];
            ushort obj2 = operandValues[1];

            if (obj1 == 0)
            {
                messageLog.SendWarning(opcode, startAddress, "called with object 0");
                Branch(obj2 == 0);
                return;
            }

            ushort obj1Parent = objectTable.ReadParentNumberByObjectNumber(obj1);

            Branch(obj1Parent == obj2);
        }

        internal void op_jl()
        {
            short x = (short)operandValues[0];
            short y = (short)operandValues[1];

            Branch(x < y);
        }

        internal void op_jump()
        {
            short offset = (short)operandValues[0];

            Jump(offset);
        }

        internal void op_jz()
        {
            ushort x = operandValues[0];

            Branch(x == 0);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Call routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal void op_call_n()
        {
            ushort byteAddress = operandValues[0];
            int address = story.UnpackRoutineAddress(byteAddress);

            Call(address);
        }

        internal void op_call_s()
        {
            ushort byteAddress = operandValues[0];
            int address = story.UnpackRoutineAddress(byteAddress);

            var storeVariable = bytes[pc++];

            Call(address, storeVariable);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Return routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal void op_ret()
        {
            ushort value = operandValues[0];

            Return(value);
        }

        internal void op_ret_popped()
        {
            ushort value = ReadVariableValue(0x00); // read stack

            Return(value);
        }

        internal void op_rfalse()
        {
            Return(0);
        }

        internal void op_rtrue()
        {
            Return(1);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Load/Store routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal void op_load()
        {
            byte varIdx = (byte)operandValues[0];

            ushort value = ReadVariableValueIndirectly(varIdx);

            Store(value);
        }

        internal void op_loadb()
        {
            ushort array = operandValues[0];
            ushort byteIndex = operandValues[1];

            int address = array + byteIndex;
            byte value = bytes[address];

            Store(value);
        }

        internal void op_loadw()
        {
            ushort array = operandValues[0];
            ushort wordIndex = operandValues[1];

            int address = array + (wordIndex * 2);
            ushort value = bytes.ReadWord(address);

            Store(value);
        }

        internal void op_store()
        {
            byte varIdx = (byte)operandValues[0];

            ushort value = operandValues[1];

            WriteVariableValueIndirectly(varIdx, value);
        }

        internal void op_storeb()
        {
            ushort array = operandValues[0];
            ushort byteIndex = operandValues[1];
            byte value = (byte)operandValues[2];

            int address = array + byteIndex;

            memory.WriteByte(address, value);
        }

        internal void op_storew()
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

        internal void op_copy_table()
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

        internal void op_scan_table()
        {
            // TODO: Rewrite to scan byte array directly

            ushort x = operandValues[0];
            ushort table = operandValues[1];
            ushort len = operandValues[2];
            ushort form = operandCount > 3 ? operandValues[3] : (ushort)0x82;

            ushort address = table;

            for (int j = 0; j < len; j++)
            {
                if ((form & 0x80) != 0)
                {
                    var value = bytes.ReadWord(address);
                    if (value == x)
                    {
                        Store(address);
                        Branch(true);
                        return;
                    }
                }
                else
                {
                    var value = bytes[address];
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
            byte varIdx = (byte)operandValues[0];

            ushort value = ReadVariableValue(0x00); // stack

            WriteVariableValueIndirectly(varIdx, value);
        }

        internal void op_push()
        {
            ushort value = operandValues[0];

            WriteVariableValue(0x00, value); // stack
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Object routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal void op_clear_attr()
        {
            ushort objNum = operandValues[0];

            if (objNum == 0)
            {
                messageLog.SendWarning(opcode, startAddress, "called with object 0");
                return;
            }

            byte attrNum = (byte)operandValues[1];

            objectTable.SetAttributeValueByObjectNumber(objNum, attrNum, false);
        }

        internal void op_get_child()
        {
            ushort objNum = operandValues[0];

            if (objNum == 0)
            {
                messageLog.SendWarning(opcode, startAddress, "called with object 0");
                Store(0);
                Branch(false);
                return;
            }

            ushort childNum = objectTable.ReadChildNumberByObjectNumber(objNum);

            Store(childNum);
            Branch(childNum > 0);
        }

        internal void op_get_next_prop()
        {
            ushort objNum = operandValues[0];

            if (objNum == 0)
            {
                messageLog.SendWarning(opcode, startAddress, "called with object 0");
                Store(0);
                return;
            }

            ushort propNum = operandValues[1];

            // TODO: Call into memory directly to find next property

            ZObject obj = objectTable.GetByNumber(objNum);

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
                Store(nextPropNum);
            }
            else
            {
                Store(0);
            }
        }

        internal void op_get_parent()
        {
            ushort objNum = operandValues[0];

            if (objNum == 0)
            {
                messageLog.SendWarning(opcode, startAddress, "called with object 0");
                Store(0);
                return;
            }

            ushort parentNum = objectTable.ReadParentNumberByObjectNumber(objNum);

            Store(parentNum);
        }

        internal void op_get_prop()
        {
            ushort objNum = operandValues[0];
            ushort propNum = operandValues[1];

            if (objNum == 0)
            {
                messageLog.SendWarning(opcode, startAddress, "called with object 0");
                Store(0);
                return;
            }

            ZObject obj = objectTable.GetByNumber(objNum);
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

            Store(value);
        }

        internal void op_get_prop_addr()
        {
            ushort objNum = operandValues[0];

            if (objNum == 0)
            {
                messageLog.SendWarning(opcode, startAddress, "called with object 0");
                Store(0);
                return;
            }

            ushort propNum = operandValues[1];

            ZObject obj = objectTable.GetByNumber(objNum);
            ZProperty prop = obj.PropertyTable.GetByNumber(propNum);

            ushort propAddress = prop != null
                ? (ushort)prop.DataAddress
                : (ushort)0;

            Store(propAddress);
        }

        internal void op_get_prop_len()
        {
            ushort dataAddress = operandValues[0];

            ushort propLen = objectTable.ReadPropertyDataLength(dataAddress);

            Store(propLen);
        }

        internal void op_get_sibling()
        {
            ushort objNum = operandValues[0];

            if (objNum == 0)
            {
                messageLog.SendWarning(opcode, startAddress, "called with object 0");
                Store(0);
                Branch(false);
                return;
            }

            ushort siblingNum = objectTable.ReadSiblingNumberByObjectNumber(objNum);

            Store(siblingNum);
            Branch(siblingNum > 0);
        }

        internal void op_insert_obj()
        {
            ushort objNum = operandValues[0];

            if (objNum == 0)
            {
                messageLog.SendWarning(opcode, startAddress, "called with object 0");
                return;
            }

            ushort destNum = operandValues[1];

            if (destNum == 0)
            {
                messageLog.SendWarning(opcode, startAddress, "called with object 0");
                return;
            }

            objectTable.MoveObjectToDestinationByNumber(objNum, destNum);
        }

        internal void op_put_prop()
        {
            ushort objNum = operandValues[0];

            if (objNum == 0)
            {
                messageLog.SendWarning(opcode, startAddress, "called with object 0");
                return;
            }

            ushort propNum = operandValues[1];
            ushort value = operandValues[2];

            var obj = objectTable.GetByNumber(objNum);
            var prop = obj.PropertyTable.GetByNumber(propNum);

            if (prop.DataLength == 2)
            {
                story.Memory.WriteWord(prop.DataAddress, value);
            }
            else if (prop.DataLength == 1)
            {
                story.Memory.WriteByte(prop.DataAddress, (byte)(value & 0x00ff));
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        internal void op_remove_obj()
        {
            ushort objNum = operandValues[0];

            if (objNum == 0)
            {
                messageLog.SendWarning(opcode, startAddress, "called with object 0");
                return;
            }

            objectTable.RemoveObjectFromParentByNumber(objNum);
        }

        internal void op_set_attr()
        {
            ushort objNum = operandValues[0];

            if (objNum == 0)
            {
                messageLog.SendWarning(opcode, startAddress, "called with object 0");
                return;
            }

            byte attrNum = (byte)operandValues[1];

            objectTable.SetAttributeValueByObjectNumber(objNum, attrNum, true);
        }

        internal void op_test_attr()
        {
            ushort objNum = operandValues[0];

            if (objNum == 0)
            {
                messageLog.SendWarning(opcode, startAddress, "called with object 0");
                Branch(false);
                return;
            }
            
            byte attrNum = (byte)operandValues[1];

            bool result = objectTable.HasAttributeByObjectNumber(objNum, attrNum);

            Branch(result);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Output routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal void op_buffer_mode()
        {
            // TODO: What should we do with buffer_mode? Does it have any meaning?
            messageLog.SendWarning(opcode, startAddress, "Unsupported");
        }

        internal void op_new_line()
        {
            outputStreams.Print('\n');
        }

        internal void op_output_stream()
        {
            switch ((short)operandValues[0])
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

                default:
                    messageLog.SendError(opcode, startAddress, "Illegal stream value: {0}", operandValues[0]);
                    break;
            }
        }

        internal void op_print()
        {
            string text = DecodeEmbeddedText();

            outputStreams.Print(text);
        }

        internal void op_print_addr()
        {
            ushort byteAddress = operandValues[0];

            ushort[] zwords = memory.ReadZWords(byteAddress);
            string text = ztext.ZWordsAsString(zwords, ZTextFlags.All);

            outputStreams.Print(text);
        }

        internal void op_print_char()
        {
            char ch = (char)operandValues[0];
            outputStreams.Print(ch);
        }

        internal void op_print_num()
        {
            short number = (short)operandValues[0];
            outputStreams.Print(number.ToString());
        }

        internal void op_print_obj()
        {
            ushort objNum = operandValues[0];

            ZObject obj = objectTable.GetByNumber(objNum);
            outputStreams.Print(obj.ShortName);
        }

        internal void op_print_paddr()
        {
            ushort byteAddress = operandValues[0];
            int address = story.UnpackStringAddress(byteAddress);

            ushort[] zwords = memory.ReadZWords(address);
            string text = ztext.ZWordsAsString(zwords, ZTextFlags.All);

            outputStreams.Print(text);
        }

        internal void op_print_ret()
        {
            string text = DecodeEmbeddedText();
            outputStreams.Print(text + "\n");
            Return(1);
        }

        internal void op_print_table()
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

        internal void op_set_color()
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

        internal void op_set_font()
        {
            ZFont font = (ZFont)operandValues[0];

            ushort oldFont = (ushort)screen.SetFont(font);

            Store(oldFont);
        }

        internal void op_set_text_style()
        {
            ZTextStyle textStyle = (ZTextStyle)operandValues[0];

            screen.SetTextStyle(textStyle);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Input routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal void op_read_char()
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
                Store((ushort)ch);
            });
        }

        internal void op_aread()
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
                Store(10);
            });
        }

        internal void op_sread1()
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
                    memory.WriteByte(parseBuffer + 2 + (i * 4) + 3, (byte)(token.Start + 1));
                }
            });
        }

        internal void op_sread2()
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
                    memory.WriteByte(parseBuffer + 2 + (i * 4) + 3, (byte)(token.Start + 1));
                }
            });
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Window routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal void op_erase_window()
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

        internal void op_set_cursor()
        {
            ushort line = operandValues[0];
            ushort column = operandValues[1];

            screen.SetCursor(line - 1, column - 1);
        }

        internal void op_set_window()
        {
            ushort window = operandValues[0];

            screen.SetWindow(window);
        }

        internal void op_split_window()
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

        internal void op_check_arg_count()
        {
            ushort argNumber = operandValues[0];

            Branch(argNumber <= argumentCount);
        }

        internal void op_piracy()
        {
            Branch(true);
        }

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
            short range = (short)operandValues[0];

            if (range > 0)
            {
                // range should be inclusive, so we need to subtract 1 since System.Random.Next makes it exclusive
                const int minValue = 1;
                int maxValue = Math.Max(minValue, range - 1);
                int result = random.Next(minValue, maxValue);

                Store((ushort)result);
            }
            else if (range < 0)
            {
                random = new Random(+range);
                Store(0);
            }
            else // range = 0s
            {
                random = new Random((int)DateTime.Now.Ticks);
                Store(0);
            }
        }

        internal void op_restore_undo()
        {
            messageLog.SendWarning(opcode, startAddress, "Undo is not supported.");

            Store(unchecked((ushort)-1));
        }

        internal void op_save_undo()
        {
            messageLog.SendWarning(opcode, startAddress, "Undo is not supported.");

            Store(unchecked((ushort)-1));
        }

        internal void op_show_status()
        {
            screen.ShowStatus();
        }

        internal void op_tokenize()
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

        internal void op_verify()
        {
            Branch(story.ActualChecksum == memory.ReadChecksum());
        }

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
                                if (version < 4)
                                {
                                    break; // illegal
                                }

                                op_call_s();
                                return;

                            case 0x1a:
                                if (version < 5)
                                {
                                    break; // illegal
                                }

                                op_call_n();
                                return;

                            case 0x1b:
                                if (version < 5 || version == 6)
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
                                if (version < 4)
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
                                if (version < 5)
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
                                if (version < 5)
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
                                if (version == 3)
                                {
                                    op_show_status();
                                    return;
                                }
                                else
                                {
                                    break; // illegal
                                }

                            case 0x0d:
                                if (version < 3)
                                {
                                    break; // illegal
                                }

                                op_verify();
                                return;

                            case 0x0e:
                                break; // first byte of extended opcode -- should never hit this

                            case 0x0f:
                                if (version < 5)
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
                                if (version < 4)
                                {
                                    op_sread1();
                                }
                                else if (version == 4)
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
                                if (version == 6)
                                {
                                    break; // 'pull' stack unsupported
                                }

                                op_pull();
                                return;

                            case 0x0a:
                                if (version < 3)
                                {
                                    break; // 'split_window' illegal
                                }

                                op_split_window();
                                return;

                            case 0x0b:
                                if (version < 3)
                                {
                                    break; // 'set_window' illegal
                                }

                                op_set_window();
                                return;

                            case 0x0c:
                                if (version < 4)
                                {
                                    break; // 'call_vs2' illegal
                                }

                                op_call_s();
                                return;

                            case 0x0d:
                                if (version < 4)
                                {
                                    break; // 'erase_window' illegal
                                }

                                op_erase_window();
                                return;

                            case 0x0e:
                                if (version < 4)
                                {
                                    break; // 'erase_line' illegal
                                }
                                else
                                {
                                    break; // 'erase_line' unsupported
                                }

                            case 0x0f:
                                if (version < 4)
                                {
                                    break; // 'set_cursor' illegal
                                }
                                else if (version == 6)
                                {
                                    break; // 'set_cursor' unsupported
                                }

                                op_set_cursor();
                                return;

                            case 0x10:
                                break; // 'get_cursor' unsupported;

                            case 0x11:
                                if (version < 4)
                                {
                                    break; // 'set_text_style' illegal
                                }

                                op_set_text_style();
                                return;

                            case 0x12:
                                if (version < 4)
                                {
                                    break; // 'buffer_mode' illegal
                                }

                                op_buffer_mode();
                                return;

                            case 0x13:
                                if (version < 3)
                                {
                                    break; // 'output_stream' illegal
                                }
                                else if (version == 6)
                                {
                                    break; // 'output_stream' unsupported
                                }

                                op_output_stream();
                                return;

                            case 0x14:
                                if (version < 3)
                                {
                                    break; // 'input_stream' illegal
                                }
                                else
                                {
                                    break; // 'input_stream' unsupported
                                }

                            case 0x15:
                                if (version < 3)
                                {
                                    break; // 'sound_effect' illegal
                                }
                                else
                                {
                                    break; // 'sound_effect' unsupported
                                }

                            case 0x16:
                                if (version < 4)
                                {
                                    break; // 'read_char' illegal
                                }

                                op_read_char();
                                return;

                            case 0x17:
                                if (version < 4)
                                {
                                    break; // 'scan_table' illegal
                                }

                                op_scan_table();
                                return;

                            case 0x18:
                                if (version < 5)
                                {
                                    break; // 'not' illegal
                                }

                                op_not();
                                return;

                            case 0x19:
                                if (version < 5)
                                {
                                    break; // 'call_vn' illegal
                                }

                                op_call_n();
                                return;

                            case 0x1a:
                                if (version < 5)
                                {
                                    break; // 'call_vn2' illegal
                                }

                                op_call_n();
                                return;

                            case 0x1b:
                                if (version < 5)
                                {
                                    break; // 'tokenize' illegal
                                }

                                op_tokenize();
                                return;

                            case 0x1c:
                                if (version < 5)
                                {
                                    break; // 'encode_text' illegal
                                }
                                else
                                {
                                    break; // encode_text unsupported
                                }

                            case 0x1d:
                                if (version < 5)
                                {
                                    break; // 'copy_table' illegal
                                }

                                op_copy_table();
                                return;

                            case 0x1e:
                                if (version < 5)
                                {
                                    break; // 'print_table' illegal
                                }

                                op_print_table();
                                return;

                            case 0x1f:
                                if (version < 5)
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
                                if (version < 5)
                                {
                                    break; // 'save' illegal
                                }
                                else
                                {
                                    break; // 'save' unsupported
                                }

                            case 0x01:
                                if (version < 5)
                                {
                                    break; // 'restore' illegal
                                }
                                else
                                {
                                    break; // 'restore' unsupported
                                }

                            case 0x02:
                                if (version < 5)
                                {
                                    break; // 'log_shift' unsupported
                                }

                                op_log_shift();
                                return;

                            case 0x03:
                                if (version < 5)
                                {
                                    break; // 'art_shift' unsupported
                                }

                                op_art_shift();
                                return;

                            case 0x04:
                                if (version < 5)
                                {
                                    break; // 'set_font' unsupported
                                }

                                op_set_font();
                                return;

                            case 0x05:
                                if (version != 6)
                                {
                                    break; // 'draw_picture' illegal
                                }
                                else
                                {
                                    break; // 'draw_picture' unsupported
                                }

                            case 0x06:
                                if (version != 6)
                                {
                                    break; // 'picture_data' illegal
                                }
                                else
                                {
                                    break; // 'picture_data' unsupported
                                }

                            case 0x07:
                                if (version != 6)
                                {
                                    break; // 'erase_picture' illegal
                                }
                                else
                                {
                                    break; // 'erase_picture' unsupported
                                }

                            case 0x08:
                                if (version != 6)
                                {
                                    break; // 'set_margins' illegal
                                }
                                else
                                {
                                    break; // 'set_margins' unsupported
                                }

                            case 0x09:
                                if (version < 5)
                                {
                                    break; // 'save_undo' unsupported
                                }

                                op_save_undo();
                                return;

                            case 0x0a:
                                if (version < 5)
                                {
                                    break; // 'restore_undo' unsupported
                                }

                                op_restore_undo();
                                return;

                            case 0x0b:
                                if (version < 5)
                                {
                                    break; // 'print_unicode' illegal
                                }
                                else
                                {
                                    break; // 'print_unicode' unsupported
                                }

                            case 0x0c:
                                if (version < 5)
                                {
                                    break; // 'check_unicode' illegal
                                }
                                else
                                {
                                    break; // 'check_unicode' unsupported
                                }

                            case 0x10:
                                if (version != 6)
                                {
                                    break; // 'move_window' illegal
                                }
                                else
                                {
                                    break; // 'move_window' unsupported
                                }

                            case 0x11:
                                if (version != 6)
                                {
                                    break; // 'window_size' illegal
                                }
                                else
                                {
                                    break; // 'window_size' unsupported
                                }

                            case 0x12:
                                if (version != 6)
                                {
                                    break; // 'window_style' illegal
                                }
                                else
                                {
                                    break; // 'window_style' unsupported
                                }

                            case 0x13:
                                if (version != 6)
                                {
                                    break; // 'get_wind_prop' illegal
                                }
                                else
                                {
                                    break; // 'get_wind_prop' unsupported
                                }

                            case 0x14:
                                if (version != 6)
                                {
                                    break; // 'scroll_window' illegal
                                }
                                else
                                {
                                    break; // 'scroll_window' unsupported
                                }

                            case 0x15:
                                if (version != 6)
                                {
                                    break; // 'pop_stack' illegal
                                }
                                else
                                {
                                    break; // 'pop_stack' unsupported
                                }

                            case 0x16:
                                if (version != 6)
                                {
                                    break; // 'read_mouse' illegal
                                }
                                else
                                {
                                    break; // 'read_mouse' unsupported
                                }

                            case 0x17:
                                if (version != 6)
                                {
                                    break; // 'mouse_window' illegal
                                }
                                else
                                {
                                    break; // 'mouse_window' unsupported
                                }

                            case 0x18:
                                if (version != 6)
                                {
                                    break; // 'push_stack' illegal
                                }
                                else
                                {
                                    break; // 'push_stack' unsupported
                                }

                            case 0x19:
                                if (version != 6)
                                {
                                    break; // 'put_wind_prop' illegal
                                }
                                else
                                {
                                    break; // 'put_wind_prop' unsupported
                                }

                            case 0x1a:
                                if (version != 6)
                                {
                                    break; // 'print_form' illegal
                                }
                                else
                                {
                                    break; // 'print_form' unsupported
                                }

                            case 0x1b:
                                if (version != 6)
                                {
                                    break; // 'make_menu' illegal
                                }
                                else
                                {
                                    break; // 'make_menu' unsupported
                                }

                            case 0x1c:
                                if (version != 6)
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
}
