using System.Collections.Generic;

namespace ZDebug.Core.Basics
{
    internal static partial class IMemoryReaderExtensions
    {
        public static ushort[] NextZWords(this IMemoryReader reader)
        {
            var list = new List<ushort>();

            while (true)
            {
                var zword = reader.NextWord();
                list.Add(zword);

                if ((zword & 0x8000) == 0x8000)
                {
                    break;
                }
            }

            return list.ToArray();
        }
    }
}
