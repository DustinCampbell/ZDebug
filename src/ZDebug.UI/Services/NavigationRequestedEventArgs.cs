using System;

namespace ZDebug.UI.Services;

public sealed class NavigationRequestedEventArgs : EventArgs
{
    private readonly int address;

    public NavigationRequestedEventArgs(int address)
    {
        this.address = address;
    }

    public int Address => address;
}
