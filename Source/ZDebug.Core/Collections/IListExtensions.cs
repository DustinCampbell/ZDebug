using System;
using System.Collections;
using System.Collections.Generic;

namespace ZDebug.Core.Collections
{
    internal static class IListExtensions
    {
        private class IndexedEnumerableWrapper<T> : IIndexedEnumerable<T>
        {
            private readonly IList<T> list;

            public IndexedEnumerableWrapper(IList<T> list)
            {
                this.list = list;
            }

            public T this[int index]
            {
                get { return list[index]; }
            }

            public int Count
            {
                get { return list.Count; }
            }

            public IEnumerator<T> GetEnumerator()
            {
                return list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public static IIndexedEnumerable<T> ToIndexedEnumerable<T>(this IList<T> list)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }

            return new IndexedEnumerableWrapper<T>(list);
        }
    }
}
