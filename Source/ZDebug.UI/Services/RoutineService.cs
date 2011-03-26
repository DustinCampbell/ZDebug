using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml.Linq;
using ZDebug.Core.Routines;

namespace ZDebug.UI.Services
{
    [Export]
    internal class RoutineService : IService, IPersistable
    {
        private readonly StoryService storyService;

        private ZRoutineTable routineTable;

        [ImportingConstructor]
        private RoutineService(
            StoryService storyService)
        {
            this.storyService = storyService;
            this.storyService.StoryOpened += StoryService_StoryOpened;
            this.storyService.StoryClosed += StoryService_StoryClosed;
        }

        private void StoryService_StoryOpened(object sender, StoryOpenedEventArgs e)
        {
            routineTable = new ZRoutineTable(e.Story);
        }

        private void StoryService_StoryClosed(object sender, StoryClosedEventArgs e)
        {
            routineTable = null;
        }

        // TODO: Hide this when there's a debugger service with an appropriate event.
        public void Add(int address)
        {
            routineTable.Add(address);
        }

        public void SetRoutineName(int address, string name)
        {
            var routine = routineTable.GetByAddress(address);
            if (routine.Name == name)
            {
                return;
            }

            routine.Name = name;

            var handler = RoutineNameChanged;
            if (handler != null)
            {
                handler(this, new RoutineNameChangedEventArgs(routine));
            }
        }

        public ZRoutineTable RoutineTable
        {
            get
            {
                return routineTable;
            }
        }

        public void Load(XElement xml)
        {
            var routinesElem = xml.Element("knownroutines");
            if (routinesElem != null)
            {
                foreach (var routineElem in routinesElem.Elements("routine"))
                {
                    var addAttr = routineElem.Attribute("address");
                    var nameAttr = routineElem.Attribute("name");

                    var address = (int)addAttr;
                    var name = nameAttr != null ? (string)nameAttr : null;

                    if (routineTable.Exists(address))
                    {
                        routineTable.GetByAddress(address).Name = name;
                    }
                    else
                    {
                        routineTable.Add(address, name);
                    }
                }
            }
        }

        public XElement Store()
        {
            return new XElement("knownroutines",
                routineTable.Select(r => new XElement("routine",
                    new XAttribute("address", r.Address),
                    new XAttribute("name", r.Name))));
        }

        public event EventHandler<RoutineNameChangedEventArgs> RoutineNameChanged;
    }
}
