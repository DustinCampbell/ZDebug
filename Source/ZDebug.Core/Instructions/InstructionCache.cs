using System.Collections.Generic;

namespace ZDebug.Core.Instructions
{
    internal sealed class InstructionCache
    {
        private readonly Dictionary<int, Instruction> map;

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
    }
}
