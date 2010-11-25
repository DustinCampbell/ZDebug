using System;

namespace ZDebug.Core.Text
{
    [Flags]
    public enum ZTextFlags
    {
        None = 0x00,
        AllowAbbreviations = 0x01,
        AllowIncompleteMultibyteChars = 0x02,

        All = AllowAbbreviations | AllowIncompleteMultibyteChars
    }
}
