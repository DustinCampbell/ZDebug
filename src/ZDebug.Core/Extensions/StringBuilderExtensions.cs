using System;
using System.Text;

namespace ZDebug.Core.Extensions;

public static class StringBuilderExtensions
{
    public static StringBuilder AppendCommaSeparatedList<T>(this StringBuilder builder, ReadOnlySpan<T> source, Func<T, string> selector)
    {
        var first = true;

        foreach (var item in source)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                builder.Append(", ");
            }

            builder.Append(selector(item));
        }

        return builder;
    }
}
