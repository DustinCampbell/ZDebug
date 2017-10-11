using System;
using System.Composition;

namespace ZDebug.UI.Services
{
    [Export, Shared]
    internal class NavigationService : IService
    {
        public void RequestNavigation(int address)
        {
            var handler = NavigationRequested;
            if (handler != null)
            {
                handler(this, new NavigationRequestedEventArgs(address));
            }
        }

        public event EventHandler<NavigationRequestedEventArgs> NavigationRequested;
    }
}
