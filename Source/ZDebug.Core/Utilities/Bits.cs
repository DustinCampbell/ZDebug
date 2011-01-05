using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZDebug.Core.Utilities
{
    internal static class Bits
    {
        public static ushort Clear(ushort value, byte bit)
        {
            if (bit > 15)
            {
                throw new ArgumentOutOfRangeException("bit", bit, "'bit' should be in the range 0 to 15");
            }

            return (ushort)(value & ~(1 << bit));
        }

        public static ushort Set(ushort value, byte bit)
        {
            if (bit > 15)
            {
                throw new ArgumentOutOfRangeException("bit", bit, "'bit' should be in the range 0 to 15");
            }

            return (ushort)(value | (1 << bit));
        }

        public static ushort Toggle(ushort value, byte bit)
        {
            if (bit > 15)
            {
                throw new ArgumentOutOfRangeException("bit", bit, "'bit' should be in the range 0 to 15");
            }

            return (ushort)(value ^ (1 << bit));
        }
    }
}
