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

                var b1 = (byte)(address >> 8);
                var b2 = (byte)(address & 0x00ff);

                bytes[parse] = b1;
                bytes[parse + 1] = b2;
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

            int textPtr1 = text;
            int textPtr2 = 0;

            int length = 0;
            if (version >= 5)
            {
                length = memory.ReadByte(++textPtr1);
            }

            byte zc;
            do
            {
                // Get next ZSCII character
                if (version >= 5 && textPtr1 == text + 2 + length)
                {
                    zc = 0;
                }
                else
                {
                    zc = memory.ReadByte(++textPtr1);
                }

                // Check for separator
                int sepPtr = dictionary;
                byte sepCount = bytes[sepPtr++];
                byte sep;
                do
                {
                    sep = bytes[sepPtr++];
                }
                while (zc != sep && --sepCount != 0);

                // This could be the start or end of a word
                if (sepCount == 0 && zc != 0x20 && zc != 0)
                {
                    if (textPtr2 == 0)
                    {
                        textPtr2 = textPtr1;
                    }
                }
                else if (textPtr2 != 0)
                {
                    TokenizeWord(bytes, text, (ushort)(textPtr2 - text), (ushort)(textPtr1 - textPtr2), parse, dictionary, flag);

                    textPtr2 = 0;
                }

                // Translate separator (which is a word in its own right)
                if (sepCount != 0)
                {
                    TokenizeWord(bytes, text, (ushort)(textPtr1 - text), (ushort)1, parse, dictionary, flag);
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
            for (int i = 0; i < commandText.Length; i++)
            {
                var ch = commandText[i];
                if (start < 0)
                {
                    if (ch != ' ')
                    {
                        start = i;
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
            int zwordsSize = version <= 3 ? 2 : 3;

            if (word.Length > zwordsSize * 3)
            {
                word = word.Substring(0, zwordsSize * 3);
            }

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

                // TODO: Encode word and compare z-words directly

                ushort[] entryZWords = memory.ReadWords(entryAddress, zwordsSize);
                string entryText = ZWordsAsString(entryZWords, ZTextFlags.All);

                if (entryText == word)
                {
                    return entryAddress;
                }

                if (sorted)
                {
                    if (string.CompareOrdinal(word, entryText) > 0)
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
            }

            return 0;
        }
    }
}
