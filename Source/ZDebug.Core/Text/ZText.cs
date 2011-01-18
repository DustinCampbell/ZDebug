using System;
using System.Collections.Generic;
using System.Text;
using ZDebug.Core.Basics;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Text
{
    public sealed class ZText
    {
        private readonly Memory memory;
        private readonly AlphabetTable alphabetTable;
        private readonly byte version;
        private readonly StringBuilder builder;

        private readonly AlphabetTable abbreviationAlphabetTable;
        private readonly StringBuilder abbreviationBuilder;

        public ZText(Memory memory)
        {
            this.memory = memory;
            this.alphabetTable = new AlphabetTable(memory);
            this.version = memory.ReadVersion();
            this.builder = new StringBuilder();
            this.abbreviationAlphabetTable = new AlphabetTable(memory);
            this.abbreviationBuilder = new StringBuilder();
        }

        private byte[] ZWordsToZChars(ushort[] zwords)
        {
            var count = zwords.Length;
            var result = new byte[count * 3];
            for (int i = 0; i < count; i++)
            {
                var zword = zwords[i];
                var offset = i * 3;

                result[offset] = (byte)((zword & 0x7c00) >> 10);
                result[offset + 1] = (byte)((zword & 0x03e0) >> 5);
                result[offset + 2] = (byte)(zword & 0x001f);
            }

            return result;
        }

        private string ZWordsAsString(ushort[] zwords, ZTextFlags flags, AlphabetTable alphabetTable, StringBuilder builder)
        {
            var zchars = ZWordsToZChars(zwords);

            builder.Clear();
            if (builder.Capacity < zchars.Length)
            {
                builder.Capacity = zchars.Length;
            }

            alphabetTable.FullReset();

            var i = 0;
            while (i < zchars.Length)
            {
                var zchar = zchars[i];

                if (zchar == 0)
                {
                    builder.Append(' ');
                }
                else if (zchar >= 1 && zchar <= 3)
                {
                    if (version == 1 || (version == 2 && zchar >= 2))
                    {
                        switch (zchar)
                        {
                            case 1:
                                builder.Append('\n');
                                break;
                            case 2:
                                alphabetTable.Shift();
                                break;
                            case 3:
                                alphabetTable.DoubleShift();
                                break;
                        }
                    }
                    else // abbeviations
                    {
                        if ((flags & ZTextFlags.AllowAbbreviations) == 0)
                        {
                            throw new InvalidOperationException("Encountered ZSCII code for an illegal abbreviation.");
                        }

                        if (i + 1 < zchars.Length)
                        {
                            var abbreviationCode = zchars[++i];
                            var abbreviationIndex = (32 * (zchar - 1)) + abbreviationCode;
                            var abbreviationZWords = memory.ReadAbbreviation(abbreviationIndex);
                            var abbreviationText = ZWordsAsString(abbreviationZWords, ZTextFlags.None, abbreviationAlphabetTable, abbreviationBuilder);
                            builder.Append(abbreviationText);
                        }
                    }
                }
                else if (zchar == 4)
                {
                    if (version <= 2)
                    {
                        alphabetTable.ShiftLock();
                    }
                    else
                    {
                        alphabetTable.Shift();
                    }
                }
                else if (zchar == 5)
                {
                    if (version <= 2)
                    {
                        alphabetTable.DoubleShiftLock();
                    }
                    else
                    {
                        alphabetTable.DoubleShift();
                    }
                }
                else if (zchar == 6 && alphabetTable.CurrentAlphabet == 2)
                {
                    // If this is character 6 in A2, it's a multibyte ZSCII character
                    // Note that it can be legal for the stream to end in the middle of a 
                    // multi-byte ZSCII character (i.e. in the dictionary table). In that case,
                    // the value is discared or an exception is thrown if that behavior
                    // isn't allowed.

                    // The alphabet table must be reset to ensure that the next zcode
                    // after the multi-byte ZSCII character uses the correct alphabet.
                    alphabetTable.Reset();

                    if (i + 2 < zchars.Length)
                    {
                        var zscii1 = zchars[++i];
                        var zscii2 = zchars[++i];
                        var zscii = ((zscii1 & 0x1f) << 5) | zscii2;
                        builder.Append((char)zscii);
                    }
                    else
                    {
                        if ((flags & ZTextFlags.AllowIncompleteMultibyteChars) == 0)
                        {
                            throw new InvalidOperationException("Encountered illegal incomplete multi-byte ZSCII character.");
                        }
                    }
                }
                else if (zchar > 31)
                {
                    throw new InvalidOperationException("Unexpected ZSCII character value: " + zchar + ". Legal values are from 0 to 31.");
                }
                else
                {
                    builder.Append(alphabetTable.ReadChar(zchar));
                }

                i++;
            }

            return builder.ToString();
        }

        public string ZWordsAsString(ushort[] zwords, ZTextFlags flags)
        {
            return ZWordsAsString(zwords, flags, alphabetTable, builder);
        }


        /// <summary>
        /// Reads the Z-words at the specified <paramref name="address"/> from the given <see cref="Memory"/>.
        /// </summary>
        public ushort[] ReadZWords(int address)
        {
            int count = 0;
            while (true)
            {
                var zword = memory.ReadWord(address + (count++ * 2));
                if ((zword & 0x8000) != 0)
                {
                    break;
                }
            }

            return memory.ReadWords(address, count);
        }


        private string ExpandCommonAbbreviations(string text)
        {
            // Some older games don't define these common abbreviations
            if (text == "g")
            {
                return "again";
            }
            if (text == "x")
            {
                return "examine";
            }
            else if (text == "z")
            {
                return "wait";
            }
            else
            {
                return text;
            }
        }

        private ushort TranslateToZscii(char ch)
        {
            // TODO: Handle unicode, mouse clicks, etc.

            return (ushort)ch;
        }

        private ushort[] EncodeZText(string text)
        {
            var resolution = version <= 3 ? 2 : 3;
            text = ExpandCommonAbbreviations(text);

            byte[] zchars = new byte[resolution * 3];

            int i = 0;
            int textIndex = 0;
            while (i < 3 * resolution)
            {
                if (textIndex < text.Length)
                {
                    var ch = text[textIndex++];
                    if (ch == 0x20) // space
                    {
                        zchars[i++] = 0;
                        continue;
                    }

                    var setAndIndex = alphabetTable.FindSetAndIndexOfChar(ch);

                    if (setAndIndex != null)
                    {
                        if (setAndIndex.Item1 != 0)
                        {
                            zchars[i++] = (byte)((version <= 2 ? 1 : 3) + setAndIndex.Item1);
                        }

                        zchars[i++] = setAndIndex.Item2;
                    }
                    else
                    {
                        // character not found, store its ZSCII value
                        ushort zc = TranslateToZscii(ch);

                        zchars[i++] = 5;
                        zchars[i++] = 6;
                        zchars[i++] = (byte)(zc >> 5);
                        zchars[i++] = (byte)(zc & 0x1f);
                    }
                }
                else
                {
                    zchars[i++] = 5;
                }
            }

            var result = new ushort[resolution];

            for (i = 0; i < resolution; i++)
            {
                result[i] = (ushort)(
                    (ushort)(zchars[i * 3] << 10) |
                    (ushort)(zchars[i * 3 + 1] << 5) |
                    (ushort)(zchars[i * 3 + 2]));
            }

            result[resolution - 1] |= 0x8000;

            return result;
        }

        private void TokenizeWord(byte[] bytes, ushort text, ushort start, ushort length, ushort parse, ushort dictionary, bool flag)
        {
            byte tokenMax = bytes[parse++];
            byte tokenCount = bytes[parse];

            if (tokenCount < tokenMax)
            {
                bytes[parse++] = (byte)(tokenCount + 1);
            }

            char[] wordChars = new char[length];
            for (int i = 0; i < length; i++)
            {
                wordChars[i] = (char)bytes[text + start + i];
            }
            string word = new string(wordChars);

            ushort address = LookupWord(word, dictionary);

            if (address != 0 || !flag)
            {
                parse += (byte)(tokenCount * 4);

                bytes.WriteWord(parse, address);
                bytes[parse + 2] = (byte)length;
                bytes[parse + 3] = (byte)start;
            }
        }

        /// <summary>
        /// Performs Z-machine lexical analysis.
        /// </summary>
        /// <param name="text">Byte address of the text buffer.</param>
        /// <param name="parse">Byte address of the parse buffer.</param>
        /// <param name="dictionary">Byte address of the dictionary to use.</param>
        /// <param name="flag">If true, unrecognized words are not written into parse.</param>
        public void TokenizeLine(ushort text, ushort parse, ushort dictionary, bool flag)
        {
            // Use standard dictionary if none is provided.
            if (dictionary == 0)
            {
                dictionary = memory.ReadDictionaryAddress();
            }

            var bytes = memory.Bytes;

            // Set number of parse tokens to zero.
            bytes[parse + 1] = 0;

            int addr1 = text;
            int addr2 = 0;

            int length = 0;
            if (version >= 5)
            {
                length = bytes[++addr1];
            }

            byte zc;
            do
            {
                // Get next ZSCII character
                addr1++;

                if (version >= 5 && addr1 == text + 2 + length)
                {
                    zc = 0;
                }
                else
                {
                    zc = memory.ReadByte(addr1);
                }

                // Check for separator
                int sepAddr = dictionary;
                byte sepCount = bytes[sepAddr++];
                byte separator;
                do
                {
                    separator = bytes[sepAddr++];
                }
                while (zc != separator && --sepCount != 0);

                // This could be the start or end of a word
                if (sepCount == 0 && zc != 0x20 && zc != 0)
                {
                    if (addr2 == 0)
                    {
                        addr2 = addr1;
                    }
                }
                else if (addr2 != 0)
                {
                    TokenizeWord(bytes, text, (ushort)(addr2 - text), (ushort)(addr1 - addr2), parse, dictionary, flag);

                    addr2 = 0;
                }

                // Translate separator (which is a word in its own right)
                if (sepCount != 0)
                {
                    TokenizeWord(bytes, text, (ushort)(addr1 - text), (ushort)1, parse, dictionary, flag);
                }
            }
            while (zc != 0);
        }

        public ZCommandToken[] TokenizeCommand(string commandText, int dictionaryAddress)
        {
            var wordSepCount = memory.ReadByte(ref dictionaryAddress);
            var wordSeps = memory.ReadBytes(ref dictionaryAddress, wordSepCount).ConvertAll(b => (char)b);

            var tokens = new List<ZCommandToken>();

            int start = -1;
            int i = 0;
            while (i < commandText.Length)
            {
                var ch = commandText[i];
                if (start < 0)
                {
                    if (ch != ' ')
                    {
                        start = i;
                    }

                    if (wordSeps.Contains(ch))
                    {
                        tokens.Add(new ZCommandToken(i, 1, ch.ToString()));
                        start = -1;
                    }
                }
                else if (ch == ' ')
                {
                    var length = i - start;
                    tokens.Add(new ZCommandToken(start, length, commandText.Substring(start, length)));
                    start = -1;
                }
                else if (wordSeps.Contains(ch))
                {
                    var length = i - start;
                    tokens.Add(new ZCommandToken(start, length, commandText.Substring(start, length)));

                    tokens.Add(new ZCommandToken(i, 1, ch.ToString()));

                    start = -1;
                }

                i++;
            }

            if (start >= 0)
            {
                var length = commandText.Length - start;
                tokens.Add(new ZCommandToken(start, length, commandText.Substring(start, length)));
            }

            return tokens.ToArray();
        }

        public ushort LookupWord(string word, int dictionaryAddress)
        {
            int resolution = version <= 3 ? 2 : 3;

            if (word.Length > resolution * 3)
            {
                word = word.Substring(0, resolution * 3);
            }

            ushort[] encoded = EncodeZText(word);

            byte wordSepCount = memory.ReadByte(ref dictionaryAddress);
            dictionaryAddress += wordSepCount;

            byte entryLength = memory.ReadByte(ref dictionaryAddress);
            ushort entryCount = memory.ReadWord(ref dictionaryAddress);

            bool sorted;
            if ((short)entryCount < 0)
            {
                entryCount = (ushort)(-(short)entryCount);
                sorted = false;
            }
            else
            {
                sorted = true;
            }

            ushort lower = 0;
            ushort upper = (ushort)(entryCount - 1);

            while (lower <= upper)
            {
                ushort entryNumber = sorted
                    ? (ushort)((lower + upper) / 2)
                    : lower;

                ushort entryAddress = (ushort)(dictionaryAddress + (entryNumber * entryLength));

                ushort addr = entryAddress;

                bool cont = false;
                for (int i = 0; i < resolution; i++)
                {
                    ushort entry = memory.ReadWord(addr);

                    if (encoded[i] != entry)
                    {
                        if (sorted)
                        {
                            if (encoded[i] > entry)
                            {
                                lower = (ushort)(entryNumber + 1);
                            }
                            else
                            {
                                upper = (ushort)(entryNumber - 1);
                            }
                        }
                        else
                        {
                            lower++;
                        }

                        cont = true;
                        break;
                    }

                    addr += 2;
                }

                if (cont)
                {
                    continue;
                }

                // exact match found
                return entryAddress;
            }

            return 0;
        }
    }
}
