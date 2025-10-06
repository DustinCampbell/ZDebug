using System.Windows;
using System.Windows.Media;

namespace ZDebug.IO.Utilities
{
    public static class DpiHelper
    {
        public static float GetPixelsPerDip()
        {
            return Application.Current.MainWindow is Window mainWindow
                ? (float)VisualTreeHelper.GetDpi(mainWindow).PixelsPerDip
                : 1.0f;
        }

        public static float GetPixelsPerDip(this Visual visual)
        {
            return visual != null
                ? (float)VisualTreeHelper.GetDpi(visual).PixelsPerDip
                : 1.0f;
        }   
    }
}
