using System;
using ZDebug.Core.Basics;
using ZDebug.Core.Instructions;
using ZDebug.Core.Utilities;

namespace ZDebug.Core
{
    public class GlobalVariablesTable
    {
        private readonly Memory memory;
        private readonly int address;

        private readonly ValueKind[] valueKinds = ArrayEx.Create(240, i => ValueKind.Number);

        public GlobalVariablesTable(Memory memory)
        {
            this.memory = memory;
            this.address = memory.ReadGlobalVariableTableAddress();
        }

        private Value ReadGlobalVariable(int index)
        {
            if (index < 0 || index > 239)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            return new Value(valueKinds[index], memory.ReadWord(address + (index * 2)));
        }

        private void WriteGlobalVariable(int index, Value value)
        {
            if (index < 0 || index > 239)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            memory.WriteWord(address + (index * 2), value.RawValue);
            valueKinds[index] = value.Kind;
        }

        public int Address
        {
            get { return address; }
        }

        public Value this[int index]
        {
            get { return ReadGlobalVariable(index); }
            set { WriteGlobalVariable(index, value); }
        }
    }
}
