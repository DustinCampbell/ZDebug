using System.Collections.Generic;

namespace ZDebug.Core.Collections
{
    public interface IIndexedEnumerable<T> : IEnumerable<T>
    {
        T this[int index] { get; }

        int Count { get; }
    }
}
