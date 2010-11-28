using System;
using System.Collections.Generic;

namespace ZDebug.Core.Instructions
{
    public static class OpcodeTable
    {
        private static readonly Dictionary<Tuple<OpcodeKind, int, byte>, Opcode> opcodeMap;

        static OpcodeTable()
        {
            opcodeMap = new Dictionary<Tuple<OpcodeKind, int, byte>, Opcode>();
        }

        private static Tuple<OpcodeKind, int, byte> CreateKey(OpcodeKind kind, int number, byte version)
        {
            return Tuple.Create(kind, number, version);
        }

        private static void AddOpcode(OpcodeKind kind, int number, string name, OpcodeFlags flags, byte fromVersion = 1, byte toVersion = 8)
        {
            var opcode = new Opcode(kind, number, name, flags);

            for (byte v = fromVersion; v <= toVersion; v++)
            {
                var key = CreateKey(kind, number, v);
                opcodeMap.Add(key, opcode);
            }
        }

        public static Opcode GetOpcode(OpcodeKind kind, int number, byte version)
        {
            var key = CreateKey(kind, number, version);

            Opcode opcode;
            if (!opcodeMap.TryGetValue(key, out opcode))
            {
                throw new InvalidOperationException(
                    string.Format("Could not find opcode. Kind = {0}, Number = {1:x2} ({1}), Version = {2}", kind, number, version));
            }

            return opcode;
        }
    }
}
