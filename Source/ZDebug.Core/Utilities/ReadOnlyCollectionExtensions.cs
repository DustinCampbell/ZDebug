using System;
using System.Collections.ObjectModel;

namespace ZDebug.Core.Utilities
{
    internal static class ReadOnlyCollectionExtensions
    {
        public static T[] ToArray<T>(this ReadOnlyCollection<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            var result = new T[collection.Count];
            for (int i = 0; i < collection.Count; i++)
            {
                result[i] = collection[i];
            }

            return result;
        }
    }
}
