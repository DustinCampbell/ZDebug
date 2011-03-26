using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace ZDebug.UI.Services
{
    [Export]
    internal class BreakpointService : IService
    {
        private readonly SortedSet<int> breakpoints = new SortedSet<int>();

        public void AddBreakpoint(int address)
        {
            breakpoints.Add(address);

            var handler = BreakpointAdded;
            if (handler != null)
            {
                handler(null, new BreakpointEventArgs(address));
            }
        }

        public void RemoveBreakpoint(int address)
        {
            breakpoints.Remove(address);

            var handler = BreakpointRemoved;
            if (handler != null)
            {
                handler(null, new BreakpointEventArgs(address));
            }
        }

        public void ToggleBreakpoint(int address)
        {
            if (breakpoints.Contains(address))
            {
                RemoveBreakpoint(address);
            }
            else
            {
                AddBreakpoint(address);
            }
        }

        public bool BreakpointExists(int address)
        {
            return breakpoints.Contains(address);
        }

        public void Clear()
        {
            foreach (var breakpoint in breakpoints.ToArray())
            {
                RemoveBreakpoint(breakpoint);
            }
        }

        public IEnumerable<int> Breakpoints
        {
            get
            {
                foreach (var address in breakpoints)
                {
                    yield return address;
                }
            }
        }

        public event EventHandler<BreakpointEventArgs> BreakpointAdded;
        public event EventHandler<BreakpointEventArgs> BreakpointRemoved;
    }
}
