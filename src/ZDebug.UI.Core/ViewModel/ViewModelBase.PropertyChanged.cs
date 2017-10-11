using System.Collections.Generic;
using System.ComponentModel;

namespace ZDebug.UI.ViewModel
{
    public abstract partial class ViewModelBase : INotifyPropertyChanged
    {
        private PropertyChangedEventHandler propertyChangedHandler;

        private static readonly Dictionary<string, PropertyChangedEventArgs> eventArgsCache = new Dictionary<string, PropertyChangedEventArgs>();

        private static PropertyChangedEventArgs GetEventArgs(string name)
        {
            PropertyChangedEventArgs eventArgs;

            lock (eventArgsCache)
            {
                if (!eventArgsCache.TryGetValue(name, out eventArgs))
                {
                    eventArgs = new PropertyChangedEventArgs(name);
                    eventArgsCache.Add(name, eventArgs);
                }
            }

            return eventArgs;
        }

        protected void PropertyChanged(string name)
        {
            var handler = propertyChangedHandler;
            if (handler != null)
            {
                handler(this, GetEventArgs(name));
            }
        }

        protected void AllPropertiesChanged()
        {
            PropertyChanged(string.Empty);
        }

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { propertyChangedHandler += value; }
            remove { propertyChangedHandler -= value; }
        }
    }
}
