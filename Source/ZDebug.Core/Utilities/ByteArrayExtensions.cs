namespace ZDebug.Core.Utilities
{
    internal static class ByteArrayExtensions
    {
        public static ushort ReadWord(this byte[] bytes, int index)
        {
            var b1 = bytes[index];
            var b2 = bytes[index + 1];

            return (ushort)(b1 << 8 | b2);
        }

        public static void WriteWord(this byte[] bytes, int index, ushort value)
        {
            var b1 = (byte)(value >> 8);
            var b2 = (byte)(value & 0x00ff);

            bytes[index] = b1;
            bytes[index + 1] = b2;
        }
    }
}
