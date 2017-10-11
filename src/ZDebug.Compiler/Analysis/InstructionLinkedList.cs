using System.Collections.Generic;
using ZDebug.Core.Collections;
using ZDebug.Core.Instructions;
using ZDebug.Core.Routines;

namespace ZDebug.Compiler.Analysis
{
    internal class InstructionLinkedList : LinkedList<Instruction>
    {
        private readonly IntegerMap<LinkedListNode<Instruction>> addressToNodeMap;

        public InstructionLinkedList(ZRoutine routine)
            : base(routine.Instructions)
        {
            this.addressToNodeMap = new IntegerMap<LinkedListNode<Instruction>>();

            var node = this.First;
            while (node != null)
            {
                addressToNodeMap.Add(node.Value.Address, node);
                node = node.Next;
            }
        }

        public LinkedListNode<Instruction> Find(int address)
        {
            return addressToNodeMap[address];
        }
    }
}
