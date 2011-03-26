using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace ZDebug.UI.Services
{
    [Export]
    internal class GameScriptService : IService
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
    }
}
