using System.Windows;

namespace ZDebug.UI.Extensions
{
    public static class FrameworkElementExtensions
    {
        public static T FindName<T>(this FrameworkElement element, string name)
        {
            return (T)element.FindName(name);
        }
    }
}
