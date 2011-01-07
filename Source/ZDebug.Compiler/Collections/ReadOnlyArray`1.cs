using System;
using System.Collections;
using System.Collections.Generic;

namespace ZDebug.Compiler.Collections
{
    public struct ReadOnlyArray<T> : IEnumerable<T>
    {
        public struct Enumerator : IEnumerator<T>
        {
            private T[] array;
            private int index;
            private int length;
            private T current;

            internal Enumerator(T[] array, int length)
            {
                this.array = array;

                this.index = 0;
                this.length = length;
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
                if (index < length)
                {
                    current = array[index];
                    index++;
                    return true;
                }

                return false;
            }

            void IEnumerator.Reset()
            {
                index = 0;
                current = default(T);
            }
        }

        private readonly T[] array;
        private readonly int length;

        internal static readonly ReadOnlyArray<T> Empty = new ReadOnlyArray<T>(null);

        public ReadOnlyArray(T[] array)
        {
            this.array = array;
            this.length = array != null ? array.Length : 0;
        }

        public T this[int index]
        {
            get { return array[index]; }
        }

        public int Length
        {
            get { return length; }
        }

        public bool IsEmpty
        {
            get { return length == 0; }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(array, length);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(array, length);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(array, length);
        }
    }
}
