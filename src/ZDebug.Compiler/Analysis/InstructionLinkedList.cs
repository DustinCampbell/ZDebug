using System.Collections.Generic;
using ZDebug.Core.Instructions;
using ZDebug.Core.Routines;

namespace ZDebug.Compiler.Analysis
{
    internal class InstructionLinkedList : LinkedList<Instruction>
    {
        private readonly Dictionary<int, LinkedListNode<Instruction>> addressToNodeMap;

        public InstructionLinkedList(ZRoutine routine)
        {
            foreach (var i in routine.Instructions)
            {
                this.AddLast(i);
            }

            this.addressToNodeMap = new Dictionary<int, LinkedListNode<Instruction>>();

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
