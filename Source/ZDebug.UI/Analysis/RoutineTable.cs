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
        private readonly Story story;
        private readonly InstructionCache cache;
        private readonly IntegerMap<Routine> routines;
        private readonly List<int> sortedAddresses;

        public RoutineTable(Story story, InstructionCache cache = null)
        {
            this.story = story;
            this.cache = cache ?? new InstructionCache();
            this.routines = new IntegerMap<Routine>();
            this.sortedAddresses = new List<int>();

            Add(story.MainRoutineAddress);
        }

        private static bool IsAnalyzableCall(Instruction i)
        {
            return i.Opcode.IsCall &&
                i.OperandCount > 0 &&
                i.Operands[0].Kind != OperandKind.Variable;
        }

        private int UnpackCallAddress(Instruction i)
        {
            return story.UnpackRoutineAddress(i.Operands[0].Value);
        }

        public void Add(int address)
        {
            if (Exists(address))
            {
                return;
            }

            var routine = Routine.Parse(address, story, cache);

            routines.Add(address, routine);

            var index = sortedAddresses.BinarySearch(address);
            sortedAddresses.Insert(~index, address);

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
            return routines.Contains(address);
        }

        public Routine GetByAddress(int address)
        {
            return routines[address];
        }

        public Routine this[int index]
        {
            get { return routines[sortedAddresses[index]]; }
        }

        public int Count
        {
            get { return routines.Count; }
        }

        public IEnumerator<Routine> GetEnumerator()
        {
            for (int i = 0; i < sortedAddresses.Count; i++)
            {
                yield return routines[sortedAddresses[i]];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public event EventHandler<RoutineAddedEventArgs> RoutineAdded;
    }
}
