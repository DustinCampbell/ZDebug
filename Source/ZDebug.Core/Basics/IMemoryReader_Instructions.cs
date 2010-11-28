using System;
using ZDebug.Core.Instructions;

namespace ZDebug.Core.Basics
{
    public static class IMemoryReader_Instructions
    {
        public static InstructionReader AsInstructionReader(this IMemoryReader reader, byte version)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            return new InstructionReader(reader, version);
        }
    }
}
