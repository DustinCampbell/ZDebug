using System;

namespace ZDebug.Core.Instructions
{
    [Flags]
    public enum OpcodeFlags
    {
        None = 0x00,
        Store = 0x01,
        Branch = 0x02,
        Text = 0x04,
        Call = 0x10,
        DoubleVar = 0x20,
        FirstOpByRef = 0x40,
        Return = 0x80
    }
}
