using System;
using ZDebug.Core.Execution;

namespace ZDebug.Core.Instructions
{
    public sealed class Opcode
    {
        private readonly OpcodeKind kind;
        private readonly byte number;
        private readonly string name;
        private readonly OpcodeFlags flags;
        private readonly OpcodeRoutine routine;

        public Opcode(OpcodeKind kind, byte number, string name, OpcodeFlags flags, OpcodeRoutine routine)
        {
            this.kind = kind;
            this.number = number;
            this.name = name;
            this.flags = flags;
            this.routine = routine;
        }

        public void Execute(Instruction instruction, IExecutionContext context)
        {
            if (instruction == null)
            {
                throw new ArgumentNullException("instruction");
            }

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (instruction.Opcode != this)
            {
                throw new ArgumentException("Instruction.Opcode must match this opcode.", "instruction");
            }

            if (routine == null)
            {
                throw new InvalidOperationException(
                    string.Format(
@"Routine does not exist for opcode '{0}'.

Kind = {1}
Number = {2:x2} ({2})", name, kind, number));
            }

            routine(instruction, context);
        }

        public OpcodeKind Kind
        {
            get { return kind; }
        }

        public byte Number
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

        public bool HasStoreVariable
        {
            get { return flags.HasFlag(OpcodeFlags.Store); }
        }

        public bool HasBranch
        {
            get { return flags.HasFlag(OpcodeFlags.Branch); }
        }

        public bool HasZText
        {
            get { return flags.HasFlag(OpcodeFlags.ZText); }
        }

        public bool IsCall
        {
            get { return flags.HasFlag(OpcodeFlags.Call); }
        }

        public bool IsDoubleVariable
        {
            get { return flags.HasFlag(OpcodeFlags.DoubleVar); }
        }

        public bool IsFirstOpByRef
        {
            get { return flags.HasFlag(OpcodeFlags.FirstOpByRef); }
        }

        public bool IsJump
        {
            get { return kind == OpcodeKind.OneOp && number == 0x0c; }
        }

        public bool IsQuit
        {
            get { return kind == OpcodeKind.ZeroOp && number == 0x0a; }
        }

        public bool IsReturn
        {
            get { return flags.HasFlag(OpcodeFlags.Return); }
        }
    }
}
