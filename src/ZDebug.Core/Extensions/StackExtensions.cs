using System.Collections.Generic;

namespace ZDebug.Core.Extensions
{
    public static class StackExtensions
    {
        public static IEnumerable<T> TopToBottom<T>(this Stack<T> stack)
        {
            foreach (var item in stack)
            {
                yield return item;
            }
        }
    }
}
