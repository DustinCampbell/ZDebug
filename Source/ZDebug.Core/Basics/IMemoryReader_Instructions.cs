using System;
using ZDebug.Core.Instructions;

namespace ZDebug.Core.Basics
{
    public static class IMemoryReader_Instructions
    {
        internal static InstructionReader AsInstructionReader(this MemoryReader reader, byte version, InstructionCache cache = null)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            return new InstructionReader(reader, version, cache);
        }
    }
}
