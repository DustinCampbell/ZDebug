using System;
using System.Collections.Generic;
using ZDebug.Core.Collections;

namespace ZDebug.Core.Instructions
{
    internal sealed class InstructionCache
    {
        private readonly Dictionary<int, Instruction> map;

        private Operand[] operands = new Operand[1024];
        private int freeIndex;
        private int size = 1024;

        public InstructionCache()
        {
            map = new Dictionary<int, Instruction>();
        }

        public bool TryGet(int address, out Instruction instruction)
        {
            return map.TryGetValue(address, out instruction);
        }

        public void Add(int address, Instruction instruction)
        {
            map.Add(address, instruction);
        }

        public ReadOnlyArray<Operand> AllocateOperands(int length)
        {
            if (length == 0)
            {
                return ReadOnlyArray<Operand>.Empty;
            }

            if (freeIndex > size - length)
            {
                var newSize = operands.Length * 2;
                var newOperands = new Operand[newSize];
                Array.Copy(operands, 0, newOperands, 0, operands.Length);
                operands = newOperands;
                size = newSize;
            }

            var result = new ReadOnlyArray<Operand>(operands, freeIndex, length);
            freeIndex += length;
            return result;
        }
    }
}
