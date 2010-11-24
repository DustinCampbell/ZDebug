using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ZDebug.UI.Controls
{
    public class AutoDisableImage : Image
    {
        static AutoDisableImage()
        {
            Image.IsEnabledProperty.OverrideMetadata(
                typeof(AutoDisableImage),
                new FrameworkPropertyMetadata(
                    defaultValue: true,
                    propertyChangedCallback: (s, e) =>
                    {
                        var img = s as AutoDisableImage;
                        var isEnabled = Convert.ToBoolean(e.NewValue);
                        if (isEnabled)
                        {
                            img.Source = ((FormatConvertedBitmap)img.Source).Source;
                            img.OpacityMask = null;
                        }
                        else
                        {
                            var bitmapImage = new BitmapImage(new Uri(img.Source.ToString()));
                            img.Source = new FormatConvertedBitmap(bitmapImage, PixelFormats.Gray32Float, null, 0.0);
                            img.OpacityMask = new ImageBrush(bitmapImage);
                        }
                    }));
        }
    }
}
