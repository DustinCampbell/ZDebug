using System;
using System.Collections.ObjectModel;

namespace ZDebug.Core.Utilities
{
    internal static class ArrayExtensions
    {
        public static ReadOnlyCollection<T> AsReadOnly<T>(this T[] array)
        {
            return Array.AsReadOnly(array);
        }

        public static string AsString(this char[] array)
        {
            return new string(array);
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

        public static T[] FindAll<T>(this T[] array, Predicate<T> predicate)
        {
            return Array.FindAll(array, predicate);
        }

        private static T[] ShallowCopyCore<T>(T[] array, int index, int length)
        {
            var result = new T[length];

            if (length > 0)
            {
                Array.Copy(array, index, result, 0, length);
            }

            return result;
        }

        public static T[] ShallowCopy<T>(this T[] array, int index, int length)
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

            return ShallowCopyCore(array, index, length);
        }

        public static T[] ShallowCopy<T>(this T[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            return ShallowCopyCore(array, 0, array.Length);
        }

        public static T First<T>(this T[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            return array[0];
        }

        public static T FirstOrDefault<T>(this T[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (array.Length > 0)
            {
                return array[0];
            }
            else
            {
                return default(T);
            }
        }

        public static T Last<T>(this T[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            return array[array.Length - 1];
        }

        public static T LastOrDefault<T>(this T[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (array.Length > 0)
            {
                return array[array.Length - 1];
            }
            else
            {
                return default(T);
            }
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

        public static T[] Skip<T>(this T[] array, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (count < 0 || array.Length < count)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            return ShallowCopyCore(array, count, array.Length - count);
        }
    }
}
