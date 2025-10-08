using System;
using System.Collections;
using System.Collections.Generic;

namespace ZDebug.Core.Collections;

public readonly struct ReadOnlyArray<T> : IEnumerable<T>
{
    public struct Enumerator : IEnumerator<T>
    {
        private readonly T[] array;
        private readonly int startIndex;
        private readonly int length;
        private int index;
        private T current;

        internal Enumerator(T[] array, int startIndex, int length)
        {
            this.array = array;
            this.startIndex = startIndex;
            this.length = length;

            index = startIndex;
            current = default(T);
        }

        public T Current => current;

        public void Dispose()
        {
        }

        object IEnumerator.Current => current;

        public bool MoveNext()
        {
            if (index - startIndex < length)
            {
                current = array[index];
                index++;
                return true;
            }

            return false;
        }

        void IEnumerator.Reset()
        {
            index = startIndex;
            current = default(T);
        }
    }

    private readonly T[] array;
    private readonly int startIndex;
    public readonly int Length;

    internal static readonly ReadOnlyArray<T> Empty = new ReadOnlyArray<T>(null);

    internal ReadOnlyArray(T[] array, int startIndex, int length)
    {
        this.array = array;
        this.startIndex = startIndex;
        Length = length;
    }

    public ReadOnlyArray(T[] array)
    {
        this.array = array;
        startIndex = 0;
        Length = array != null ? array.Length : 0;
    }

    internal T[] InnerArray => array;

    internal int StartIndex => startIndex;

    public T this[int index]
    {
        get => array[startIndex + index]; internal set => array[startIndex + index] = value;
    }

    public bool IsEmpty => Length == 0;

    public ReadOnlyArray<T> Skip(int count)
    {
        if (count < 0 || Length < count)
        {
            throw new ArgumentOutOfRangeException("count");
        }

        return new ReadOnlyArray<T>(array, startIndex + count, Length - count);
    }

    public Enumerator GetEnumerator() => new Enumerator(array, startIndex, Length);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(array, startIndex, Length);

    IEnumerator IEnumerable.GetEnumerator() => new Enumerator(array, startIndex, Length);
}
