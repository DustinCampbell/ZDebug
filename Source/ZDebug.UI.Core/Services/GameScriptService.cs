using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml.Linq;

namespace ZDebug.UI.Services
{
    [Export]
    public class GameScriptService : IService, IPersistable
    {
        private readonly StoryService storyService;
        private readonly List<string> commands = new List<string>();
        private int commandIndex;

        [ImportingConstructor]
        private GameScriptService(StoryService storyService)
        {
            this.storyService = storyService;
            this.storyService.StoryClosed += StoryService_StoryClosed;
        }

        private void StoryService_StoryClosed(object sender, StoryClosedEventArgs e)
        {
            commands.Clear();
        }

        public void Clear()
        {
            commands.Clear();
        }

        public void SetCommands(IEnumerable<string> commands)
        {
            this.commands.Clear();
            this.commands.AddRange(commands);
            commandIndex = this.commands.Count != 0 ? 0 : -1;
        }

        public bool HasNextCommand()
        {
            return commandIndex >= 0 && commandIndex < commands.Count;
        }

        public string GetNextCommand()
        {
            if (commandIndex < 0 || commandIndex >= commands.Count)
            {
                throw new InvalidOperationException();
            }

            return commands[commandIndex++];
        }

        public int CommandCount
        {
            get { return commands.Count; }
        }

        public IEnumerable<string> Commands
        {
            get
            {
                foreach (var command in commands)
                {
                    yield return command;
                }
            }
        }

        void IPersistable.Load(XElement xml)
        {
            commands.Clear();

            var scriptElem = xml.Element("gamescript");
            if (scriptElem != null)
            {
                foreach (var commandElem in scriptElem.Elements("command"))
                {
                    commands.Add(commandElem.Value);
                }
            }
        }

        XElement IPersistable.Store()
        {
            return new XElement("gamescript",
                commands.Select(c => new XElement("command", c)));
        }
    }
}
