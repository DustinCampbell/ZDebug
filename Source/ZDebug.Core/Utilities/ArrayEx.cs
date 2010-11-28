using System;
using System.Collections.ObjectModel;

namespace ZDebug.Core.Utilities
{
    internal static class ArrayEx
    {
        public static ReadOnlyCollection<T> AsReadOnly<T>(this T[] array)
        {
            return Array.AsReadOnly(array);
        }

        public static T[] Concat<T>(this T[] array, T[] other)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (other.Length == 0)
            {
                return array;
            }

            var newArray = array.Resize(array.Length + other.Length);
            Array.Copy(other, 0, newArray, array.Length, other.Length);

            return newArray;
        }

        public static TOutput[] ConvertAll<TInput, TOutput>(this TInput[] array, Converter<TInput, TOutput> converter)
        {
            return Array.ConvertAll(array, converter);
        }

        public static T[] Copy<T>(this T[] array, int index, int length)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            if (index < 0 || index + length > array.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var result = new T[length];

            if (length > 0)
            {
                Array.Copy(array, index, result, 0, length);
            }

            return result;
        }

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

        public static TResult[] Select<T, TResult>(this T[] array, Func<T, TResult> selector)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (array.Length == 0)
            {
                return ArrayEx.Empty<TResult>();
            }

            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }

            var result = new TResult[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                result[i] = selector(array[i]);
            }

            return result;
        }
    }
}
