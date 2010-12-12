using System;

namespace ZDebug.Core.Execution
{
    [Flags]
    public enum ZTextStyle
    {
        Roman = 0x00,
        Reverse = 0x01,
        Bold = 0x02,
        Italic = 0x04,
        FixedPitch = 0x08
    }
}
