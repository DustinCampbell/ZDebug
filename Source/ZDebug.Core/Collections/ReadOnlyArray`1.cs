using System;
using System.Collections;
using System.Collections.Generic;

namespace ZDebug.Core.Collections
{
    public struct ReadOnlyArray<T> : IEnumerable<T>
    {
        public struct Enumerator : IEnumerator<T>
        {
            private T[] array;
            private int startIndex;
            private int length;
            private int index;
            private T current;

            internal Enumerator(T[] array, int startIndex, int length)
            {
                this.array = array;
                this.startIndex = startIndex;
                this.length = length;

                this.index = startIndex;
                this.current = default(T);
            }

            public T Current
            {
                get { return current; }
            }

            public void Dispose()
            {
            }

            object IEnumerator.Current
            {
                get { return current; }
            }

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
        private readonly int length;

        internal static readonly ReadOnlyArray<T> Empty = new ReadOnlyArray<T>(null);

        internal ReadOnlyArray(T[] array, int startIndex, int length)
        {
            this.array = array;
            this.startIndex = startIndex;
            this.length = length;
        }

        public ReadOnlyArray(T[] array)
        {
            this.array = array;
            this.startIndex = 0;
            this.length = array != null ? array.Length : 0;
        }

        internal T[] InnerArray
        {
            get { return array; }
        }

        internal int StartIndex
        {
            get { return startIndex; }
        }

        public T this[int index]
        {
            get { return array[startIndex + index]; }
            internal set { array[startIndex + index] = value; }
        }

        public int Length
        {
            get { return length; }
        }

        public bool IsEmpty
        {
            get { return length == 0; }
        }

        public ReadOnlyArray<T> Skip(int count)
        {
            if (count < 0 || length < count)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            return new ReadOnlyArray<T>(array, startIndex + count, length - count);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(array, startIndex, length);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(array, startIndex, length);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(array, startIndex, length);
        }
    }
}
