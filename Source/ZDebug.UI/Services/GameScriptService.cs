using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml.Linq;

namespace ZDebug.UI.Services
{
    [Export]
    internal class GameScriptService : IService, IPersistable
    {
        private readonly List<string> commands = new List<string>();
        private int commandIndex;

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

        public void Load(XElement xml)
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

        public XElement Store()
        {
            return new XElement("gamescript",
                commands.Select(c => new XElement("command", c)));
        }
    }
}
