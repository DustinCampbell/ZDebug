using System;
using ZDebug.Core.Objects;

namespace ZDebug.UI.ViewModel
{
    internal sealed class PropertyViewModel : ViewModelBase
    {
        private readonly ZProperty property;

        public PropertyViewModel(ZProperty property)
        {
            this.property = property;
        }

        public int Number
        {
            get { return property.Number; }
        }

        public string DataDisplayText
        {
            get
            {
                var bytes = property.ReadAsBytes();
                var byteStrings = Array.ConvertAll(bytes, b => b.ToString("x2"));
                return string.Join(" ", byteStrings);
            }
        }
    }
}
