using System;
using ZDebug.Core.Execution;

namespace ZDebug.Core.Instructions
{
    public sealed class Opcode
    {
        public readonly OpcodeKind Kind;
        public readonly byte Number;
        public readonly string Name;
        public readonly bool HasStoreVariable;
        public readonly bool HasBranch;
        public readonly bool HasZText;
        public readonly bool IsCall;
        public readonly bool IsDoubleVariable;
        public readonly bool IsFirstOpByRef;
        public readonly bool IsJump;
        public readonly bool IsQuit;
        public readonly bool IsReturn;
        private readonly OpcodeRoutine routine;

        internal Opcode(OpcodeKind kind, byte number, string name, OpcodeFlags flags, OpcodeRoutine routine)
        {
            this.Kind = kind;
            this.Number = number;
            this.Name = name;
            this.HasStoreVariable = (flags & OpcodeFlags.Store) != 0;
            this.HasBranch = (flags & OpcodeFlags.Branch) != 0;
            this.HasZText = (flags & OpcodeFlags.ZText) != 0;
            this.IsCall = (flags & OpcodeFlags.Call) != 0;
            this.IsDoubleVariable = (flags & OpcodeFlags.DoubleVar) != 0;
            this.IsFirstOpByRef = (flags & OpcodeFlags.FirstOpByRef) != 0;
            this.IsJump = kind == OpcodeKind.OneOp && number == 0x0c;
            this.IsQuit = kind == OpcodeKind.ZeroOp && number == 0x0a;
            this.IsReturn = (flags & OpcodeFlags.Return) != 0;
            this.routine = routine;
        }

        public void Execute(Instruction instruction, ushort[] opValues, int opCount, IExecutionContext context)
        {
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
Number = {2:x2} ({2})", Name, Kind, Number));
            }

            routine(instruction, opValues, opCount, context);
        }
    }
}
