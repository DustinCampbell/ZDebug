using System;
using ZDebug.Core.Processor;

namespace ZDebug.Core.Instructions
{
    public class Opcode
    {
        private readonly OpcodeKind kind;
        private readonly int number;
        private readonly string name;
        private readonly OpcodeFlags flags;
        private readonly OpcodeRoutine routine;

        public Opcode(OpcodeKind kind, int number, string name, OpcodeFlags flags, OpcodeRoutine routine)
        {
            this.kind = kind;
            this.number = number;
            this.name = name;
            this.flags = flags;
            this.routine = routine;
        }

        public void Execute(IExecutionContext context)
        {
            if (routine == null)
            {
                throw new InvalidOperationException(
                    string.Format(
@"Routine does not exist for opcode '{0}'.

Kind = {1}
Number = {2:x2} ({2})", name, kind, number));
            }

            routine(context);
        }

        public OpcodeKind Kind
        {
            get { return kind; }
        }

        public int Number
        {
            get { return number; }
        }

        public string Name
        {
            get { return name; }
        }

        public OpcodeFlags Flags
        {
            get { return flags; }
        }
    }
}
