using System;
using System.Globalization;
using System.Windows.Data;

namespace ZDebug.Terp
{
    public class TimeSpanToStringValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var timeSpan = (TimeSpan)value;

            return timeSpan.TotalSeconds.ToString("0.######");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
