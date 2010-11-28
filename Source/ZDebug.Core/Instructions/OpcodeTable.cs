using System;
using System.Collections.Generic;

namespace ZDebug.Core.Instructions
{
    public static class OpcodeTable
    {
        private static readonly Dictionary<Tuple<OpcodeKind, byte, byte>, Opcode> opcodeMap;

        static OpcodeTable()
        {
            opcodeMap = new Dictionary<Tuple<OpcodeKind, byte, byte>, Opcode>();

            // two-operand opcodes
            AddOpcode(OpcodeKind.TwoOp, 0x01, "je", OpcodeFlags.Branch);
            AddOpcode(OpcodeKind.TwoOp, 0x02, "jl", OpcodeFlags.Branch);
            AddOpcode(OpcodeKind.TwoOp, 0x03, "jg", OpcodeFlags.Branch);
            AddOpcode(OpcodeKind.TwoOp, 0x04, "dec_chk", OpcodeFlags.Branch | OpcodeFlags.FirstOpByRef);
            AddOpcode(OpcodeKind.TwoOp, 0x05, "inc_chk", OpcodeFlags.Branch | OpcodeFlags.FirstOpByRef);
            AddOpcode(OpcodeKind.TwoOp, 0x06, "jin", OpcodeFlags.Branch);
            AddOpcode(OpcodeKind.TwoOp, 0x07, "test", OpcodeFlags.Branch);
            AddOpcode(OpcodeKind.TwoOp, 0x08, "or", OpcodeFlags.Store);
            AddOpcode(OpcodeKind.TwoOp, 0x09, "and", OpcodeFlags.Store);
            AddOpcode(OpcodeKind.TwoOp, 0x0a, "test_attr", OpcodeFlags.Branch);
            AddOpcode(OpcodeKind.TwoOp, 0x0b, "set_attr");
            AddOpcode(OpcodeKind.TwoOp, 0x0c, "clear_attr");
            AddOpcode(OpcodeKind.TwoOp, 0x0d, "store", OpcodeFlags.FirstOpByRef);
            AddOpcode(OpcodeKind.TwoOp, 0x0e, "insert_obj");
            AddOpcode(OpcodeKind.TwoOp, 0x0f, "loadw", OpcodeFlags.Store);
            AddOpcode(OpcodeKind.TwoOp, 0x10, "loadb", OpcodeFlags.Store);
            AddOpcode(OpcodeKind.TwoOp, 0x11, "get_prop", OpcodeFlags.Store);
            AddOpcode(OpcodeKind.TwoOp, 0x12, "get_prop_addr", OpcodeFlags.Store);
            AddOpcode(OpcodeKind.TwoOp, 0x13, "get_next_prop", OpcodeFlags.Store);
            AddOpcode(OpcodeKind.TwoOp, 0x14, "add", OpcodeFlags.Store);
            AddOpcode(OpcodeKind.TwoOp, 0x15, "sub", OpcodeFlags.Store);
            AddOpcode(OpcodeKind.TwoOp, 0x16, "mul", OpcodeFlags.Store);
            AddOpcode(OpcodeKind.TwoOp, 0x17, "div", OpcodeFlags.Store);
            AddOpcode(OpcodeKind.TwoOp, 0x18, "mod", OpcodeFlags.Store);
            AddOpcode(OpcodeKind.TwoOp, 0x19, "call_2s", OpcodeFlags.Call | OpcodeFlags.Store, fromVersion: 4);
            AddOpcode(OpcodeKind.TwoOp, 0x1a, "call_2n", OpcodeFlags.Call, fromVersion: 5);
            AddOpcode(OpcodeKind.TwoOp, 0x1b, "set_color", fromVersion: 5, toVersion: 5);
            AddOpcode(OpcodeKind.TwoOp, 0x1b, "set_color", fromVersion: 6);
            AddOpcode(OpcodeKind.TwoOp, 0x1c, "throw", fromVersion: 5);

            // one-operand opcodes
            AddOpcode(OpcodeKind.OneOp, 0x0f, "call_1n", OpcodeFlags.Call, fromVersion: 5);

            // zero-operand opcodes
            AddOpcode(OpcodeKind.ZeroOp, 0x00, "rtrue", OpcodeFlags.Return);
            AddOpcode(OpcodeKind.ZeroOp, 0x01, "rfalse", OpcodeFlags.Return);
            AddOpcode(OpcodeKind.ZeroOp, 0x0a, "quit");

            // variable-operand opcodes
            AddOpcode(OpcodeKind.VarOp, 0x00, "call", OpcodeFlags.Call | OpcodeFlags.Store, toVersion: 4);
            AddOpcode(OpcodeKind.VarOp, 0x00, "call_vs", OpcodeFlags.Call | OpcodeFlags.Store, fromVersion: 5);
            AddOpcode(OpcodeKind.VarOp, 0x19, "call_vn", OpcodeFlags.Call, fromVersion: 5);
        }

        private static Tuple<OpcodeKind, byte, byte> CreateKey(OpcodeKind kind, byte number, byte version)
        {
            return Tuple.Create(kind, number, version);
        }

        private static void AddOpcode(
            OpcodeKind kind,
            byte number,
            string name,
            OpcodeFlags flags = OpcodeFlags.None,
            OpcodeRoutine routine = null,
            byte fromVersion = 1,
            byte toVersion = 8)
        {
            var opcode = new Opcode(kind, number, name, flags, routine);

            for (byte v = fromVersion; v <= toVersion; v++)
            {
                var key = CreateKey(kind, number, v);
                opcodeMap.Add(key, opcode);
            }
        }

        public static Opcode GetOpcode(OpcodeKind kind, byte number, byte version)
        {
            var key = CreateKey(kind, number, version);

            Opcode opcode;
            if (!opcodeMap.TryGetValue(key, out opcode))
            {
                throw new InvalidOperationException(
                    string.Format(
@"Could not find opcode.

Kind = {0}
Number = {1:x2} ({1})
Version = {2}", kind, number, version));
            }

            return opcode;
        }
    }
}
