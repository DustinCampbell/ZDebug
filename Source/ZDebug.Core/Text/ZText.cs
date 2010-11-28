using System;
using System.Collections.Generic;
using System.Text;
using ZDebug.Core.Basics;

namespace ZDebug.Core.Text
{
    public static class ZText
    {
        private static byte[] ZWordsToZChars(IList<ushort> zwords)
        {
            var result = new byte[zwords.Count * 3];

            for (int i = 0; i < zwords.Count; i++)
            {
                var zword = zwords[i];

                result[i * 3] = (byte)((zword & 0x7c00) >> 10);
                result[(i * 3) + 1] = (byte)((zword & 0x03e0) >> 5);
                result[(i * 3) + 2] = (byte)(zword & 0x001f);
            }

            return result;
        }

        public static string ZWordsAsString(IList<ushort> zwords, ZTextFlags flags, Memory memory)
        {
            var zchars = ZWordsToZChars(zwords);
            var builder = new StringBuilder(zchars.Length);
            var alphabetTable = new AlphabetTable(memory);
            var version = memory.ReadVersion();

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
                        if (!flags.HasFlag(ZTextFlags.AllowAbbreviations))
                        {
                            throw new InvalidOperationException("Encountered ZSCII code for an illegal abbreviation.");
                        }

                        if (i + 1 < zchars.Length)
                        {
                            var abbreviationCode = zchars[++i];
                            var abbreviationIndex = (32 * (zchar - 1)) + abbreviationCode;
                            var abbreviationZWords = memory.ReadAbbreviation(abbreviationIndex);
                            var abbreviationText = ZWordsAsString(abbreviationZWords, ZTextFlags.None, memory);
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
                        var zscii = ((zscii1 & 0x1f) << 5) | (zscii2 << 5);
                        builder.Append((char)zscii);
                    }
                    else
                    {
                        if (!flags.HasFlag(ZTextFlags.AllowIncompleteMultibyteChars))
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
    }
}
