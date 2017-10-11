using System;

namespace ZDebug.Core.Collections
{
    internal static class HashHelpers
    {
        internal static readonly int[] primes = new int[]
    { 
        0x00000003, 0x00000007, 0x0000000b, 0x00000011, 0x00000017, 0x0000001d, 0x00000025, 0x0000002f,
        0x0000003b, 0x00000047, 0x00000059, 0x0000006b, 0x00000083, 0x000000a3, 0x000000c5, 0x000000ef,
        0x00000125, 0x00000161, 0x000001af, 0x00000209, 0x00000277, 0x000002f9, 0x00000397, 0x0000044f,
        0x0000052f, 0x0000063d, 0x0000078b, 0x0000091d, 0x00000af1, 0x00000d2b, 0x00000fd1, 0x000012fd,
        0x000016cf, 0x00001b65, 0x000020e3, 0x00002777, 0x00002f6f, 0x000038ff, 0x0000446f, 0x0000521f,
        0x0000628d, 0x00007655, 0x00008e01, 0x0000aa6b, 0x0000cc89, 0x0000f583, 0x000126a7, 0x0001619b,
        0x0001a857, 0x0001fd3b, 0x00026315, 0x0002dd67, 0x0003701b, 0x00042023, 0x0004f361, 0x0005f0ed,
        0x00072125, 0x00088e31, 0x000a443b, 0x000c51eb, 0x000ec8c1, 0x0011bdbf, 0x00154a3f, 0x00198c4f,
        0x001ea867, 0x0024ca19, 0x002c25c1, 0x0034fa1b, 0x003f928f, 0x004c4987, 0x005b8b6f, 0x006dda89
     };

        internal static int GetPrime(int min)
        {
            if (min < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            for (int i = 0; i < primes.Length; i++)
            {
                int p = primes[i];
                if (p >= min)
                {
                    return p;
                }
            }

            for (int i = min | 1; i < 0x7fffffff; i += 2)
            {
                if (IsPrime(i))
                {
                    return i;
                }
            }

            return min;
        }

        internal static bool IsPrime(int candidate)
        {
            if ((candidate & 1) == 0)
            {
                return (candidate == 2);
            }

            int sq = (int)Math.Sqrt((double)candidate);

            for (int i = 3; i <= sq; i += 2)
            {
                if ((candidate % i) == 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
