using System;
using System.Globalization;
using System.Windows.Data;
using ZDebug.Terp.Profiling;

namespace ZDebug.Terp
{
    public class CallPercentageValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var call = (ICall)value;
            return call.Parent != null
                ? ((double)call.Elapsed.Ticks / (double)call.Parent.Elapsed.Ticks) * 100
                : 100.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
