using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ZDebug.Core.Basics;
using ZDebug.Core.Collections;

namespace ZDebug.Core.Instructions
{
    public sealed class RoutineTable : IIndexedEnumerable<Routine>
    {
        private readonly Memory memory;
        private readonly byte version;
        private readonly SortedList<int, Routine> routines;

        internal RoutineTable(Memory memory)
        {
            this.memory = memory;
            this.version = memory.ReadVersion();
            this.routines = new SortedList<int, Routine>();

            var mainRoutineAddress = memory.ReadMainRoutineAddress();
            Add(mainRoutineAddress);
        }

        private static bool IsAnalyzableCall(Instruction i)
        {
            return i.Opcode.IsCall &&
                i.Operands.Length > 0 &&
                i.Operands[0].Kind != OperandKind.Variable;
        }

        private int UnpackCallAddress(Instruction i)
        {
            return memory.UnpackRoutineAddress(i.Operands[0].RawValue);
        }

        public void Add(int address)
        {
            if (Exists(address))
            {
                return;
            }

            var reader = memory.CreateReader(address);
            var routine = Routine.Parse(reader, version);

            routines.Add(address, routine);

            var handler = RoutineAdded;
            if (handler != null)
            {
                handler(this, new RoutineAddedEventArgs(routine));
            }

            foreach (var i in routine.Instructions.Where(i => IsAnalyzableCall(i)))
            {
                Add(UnpackCallAddress(i));
            }
        }

        public bool Exists(int address)
        {
            return routines.ContainsKey(address);
        }

        public Routine GetByAddress(int address)
        {
            return routines[address];
        }

        public Routine this[int index]
        {
            get { return routines.Values[index]; }
        }

        public int Count
        {
            get { return routines.Count; }
        }

        public IEnumerator<Routine> GetEnumerator()
        {
            return routines.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public event EventHandler<RoutineAddedEventArgs> RoutineAdded;
    }
}
