﻿using System;
using System.Collections.Generic;

namespace ZDebug.Core.Instructions
{
    public static class OpcodeTable
    {
        private static readonly Dictionary<Tuple<OpcodeKind, byte, byte>, Opcode> opcodeMap;

        static OpcodeTable()
        {
            opcodeMap = new Dictionary<Tuple<OpcodeKind, byte, byte>, Opcode>();

            // one-operand opcodes
            AddOpcode(OpcodeKind.OneOp, 0x0f, "call_1n", OpcodeFlags.Call, fromVersion: 5);

            // variable-operand opcodes
            AddOpcode(OpcodeKind.VarOp, 0x00, "call", OpcodeFlags.Call | OpcodeFlags.Store, toVersion: 4);
            AddOpcode(OpcodeKind.VarOp, 0x00, "call_vs", OpcodeFlags.Call | OpcodeFlags.Store, fromVersion: 5);
        }

        private static Tuple<OpcodeKind, byte, byte> CreateKey(OpcodeKind kind, byte number, byte version)
        {
            return Tuple.Create(kind, number, version);
        }

        private static void AddOpcode(
            OpcodeKind kind,
            byte number,
            string name,
            OpcodeFlags flags,
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
