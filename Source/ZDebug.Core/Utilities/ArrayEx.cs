using System;

namespace ZDebug.Core.Utilities
{
    internal static class ArrayEx
    {
        public static T[] Create<T>(int length, Func<int, T> createItem = null)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            if (length == 0)
            {
                return new T[0];
            }

            var result = new T[length];

            if (createItem != null)
            {
                for (int i = 0; i < length; i++)
                {
                    result[i] = createItem(i);
                }
            }

            return result;
        }

        public static T[] Empty<T>()
        {
            return new T[0];
        }
    }
}
