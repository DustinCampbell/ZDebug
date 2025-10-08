using System.Windows.Media;

namespace ZDebug.IO.Utilities;

public static class DpiHelper
{
    public static float GetPixelsPerDip(this Visual visual)
    {
        return visual != null
            ? (float)VisualTreeHelper.GetDpi(visual).PixelsPerDip
            : 1.0f;
    }
}
