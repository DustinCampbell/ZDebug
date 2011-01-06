using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZDebug.UI.Services
{
    public sealed class NavigationRequestedEventArgs : EventArgs
    {
        private readonly int address;

        public NavigationRequestedEventArgs(int address)
        {
            this.address = address;
        }

        public int Address
        {
            get { return address; }
        }
    }
}
