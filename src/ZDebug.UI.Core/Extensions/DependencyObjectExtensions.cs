using System.Windows;

namespace ZDebug.UI.Extensions;

public static class DependencyObjectExtensions
{
    public static bool IsDefaultValue(this DependencyObject obj, DependencyProperty dp) => DependencyPropertyHelper.GetValueSource(obj, dp).BaseValueSource == BaseValueSource.Default;
}
