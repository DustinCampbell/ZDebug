using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml.Linq;

namespace ZDebug.UI.Services
{
    [Export]
    internal class BreakpointService : IService, IPersistable
    {
        private readonly SortedSet<int> breakpoints = new SortedSet<int>();

        public void Add(int address)
        {
            breakpoints.Add(address);

            var handler = Added;
            if (handler != null)
            {
                handler(this, new BreakpointEventArgs(address));
            }
        }

        public void Remove(int address)
        {
            breakpoints.Remove(address);

            var handler = Removed;
            if (handler != null)
            {
                handler(this, new BreakpointEventArgs(address));
            }
        }

        public void Toggle(int address)
        {
            if (breakpoints.Contains(address))
            {
                Remove(address);
            }
            else
            {
                Add(address);
            }
        }

        public bool Exists(int address)
        {
            return breakpoints.Contains(address);
        }

        public void Clear()
        {
            foreach (var breakpoint in breakpoints.ToArray())
            {
                Remove(breakpoint);
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

        public void Load(XElement xml)
        {
            breakpoints.Clear();

            var bpsElem = xml.Element("breakpoints");
            if (bpsElem != null)
            {
                foreach (var bpElem in bpsElem.Elements("breakpoint"))
                {
                    var addAttr = bpElem.Attribute("address");
                    breakpoints.Add((int)addAttr);
                }
            }
        }

        public XElement Store()
        {
            return new XElement("breakpoints",
                breakpoints.Select(b => new XElement("breakpoint", new XAttribute("address", b))));
        }

        public event EventHandler<BreakpointEventArgs> Added;
        public event EventHandler<BreakpointEventArgs> Removed;
    }
}
