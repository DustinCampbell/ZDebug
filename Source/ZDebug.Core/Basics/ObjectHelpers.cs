using System;

namespace ZDebug.Core.Basics
{
    internal static class ObjectHelpers
    {
        public static byte GetPropertyCount(byte version)
        {
            return (byte)(version <= 3 ? 31 : 63);
        }

        public static byte GetPropertyDefaultsTableSize(byte version)
        {
            return (byte)(GetPropertyCount(version) * 2);
        }

        public static ushort GetMaxObjects(byte version)
        {
            return (ushort)(version <= 3 ? 255 : 65535);
        }

        public static byte GetEntrySize(byte version)
        {
            return (byte)(version <= 3 ? 9 : 14);
        }

        public static byte GetAttributeBytesSize(byte version)
        {
            return (byte)(version <= 3 ? 4 : 6);
        }

        public static byte GetAttributeCount(byte version)
        {
            return (byte)(version <= 3 ? 32 : 48);
        }

        public static byte GetParentOffset(byte version)
        {
            return (byte)(version <= 3 ? 4 : 6);
        }

        public static byte GetSiblingOffset(byte version)
        {
            return (byte)(version <= 3 ? 5 : 8);
        }

        public static byte GetChildOffset(byte version)
        {
            return (byte)(version <= 3 ? 6 : 10);
        }

        public static byte GetPropertyTableAddressOffset(byte version)
        {
            return (byte)(version <= 3 ? 7 : 12);
        }

        public static byte GetNumberSize(byte version)
        {
            return (byte)(version <= 3 ? 1 : 2);
        }
    }
}
