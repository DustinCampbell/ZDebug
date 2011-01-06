using System.Collections;
using System.Collections.Generic;
using ZDebug.Core.Collections;

namespace ZDebug.Core.Utilities
{
    public static partial class ArrayExtensions
    {
        private class ArrayIndexedEnumerable<T> : IIndexedEnumerable<T>
        {
            private readonly T[] array;

            public ArrayIndexedEnumerable(T[] array)
            {
                this.array = array;
            }

            public T this[int index]
            {
                get { return array[index]; }
            }

            public int Count
            {
                get { return array.Length; }
            }

            public IEnumerator<T> GetEnumerator()
            {
                foreach (var item in array)
                {
                    yield return item;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

    }
}
