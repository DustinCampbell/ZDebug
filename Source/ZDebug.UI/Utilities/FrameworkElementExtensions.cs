using System.Windows;

namespace ZDebug.UI.Utilities
{
    internal static class FrameworkElementExtensions
    {
        public static T FindName<T>(this FrameworkElement element, string name)
        {
            return (T)element.FindName(name);
        }
    }
}
