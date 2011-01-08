using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZDebug.Compiler.Tests.Utilities
{
    internal static class ArrayExtensions
    {
        public static T[] Resize<T>(this T[] array, int newLength)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (newLength < 0)
            {
                throw new ArgumentOutOfRangeException("newLength");
            }

            var newArray = new T[newLength];

            if (newLength > 0)
            {
                var numberToCopy = Math.Min(array.Length, newLength);
                Array.Copy(array, 0, newArray, 0, numberToCopy);
            }

            return newArray;
        }
    }
}
