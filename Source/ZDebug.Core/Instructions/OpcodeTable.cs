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
            AddOpcode(OpcodeKind.TwoOp, 0x01, "je", OpcodeFlags.Branch, OpcodeRoutines.je);
            AddOpcode(OpcodeKind.TwoOp, 0x02, "jl", OpcodeFlags.Branch, OpcodeRoutines.jl);
            AddOpcode(OpcodeKind.TwoOp, 0x03, "jg", OpcodeFlags.Branch, OpcodeRoutines.jg);
            AddOpcode(OpcodeKind.TwoOp, 0x04, "dec_chk", OpcodeFlags.Branch | OpcodeFlags.FirstOpByRef, OpcodeRoutines.dec_chk);
            AddOpcode(OpcodeKind.TwoOp, 0x05, "inc_chk", OpcodeFlags.Branch | OpcodeFlags.FirstOpByRef, OpcodeRoutines.inc_chk);
            AddOpcode(OpcodeKind.TwoOp, 0x06, "jin", OpcodeFlags.Branch, OpcodeRoutines.jin);
            AddOpcode(OpcodeKind.TwoOp, 0x07, "test", OpcodeFlags.Branch, OpcodeRoutines.test);
            AddOpcode(OpcodeKind.TwoOp, 0x08, "or", OpcodeFlags.Store, OpcodeRoutines.or);
            AddOpcode(OpcodeKind.TwoOp, 0x09, "and", OpcodeFlags.Store, OpcodeRoutines.and);
            AddOpcode(OpcodeKind.TwoOp, 0x0a, "test_attr", OpcodeFlags.Branch, OpcodeRoutines.test_attr);
            AddOpcode(OpcodeKind.TwoOp, 0x0b, "set_attr", routine: OpcodeRoutines.set_attr);
            AddOpcode(OpcodeKind.TwoOp, 0x0c, "clear_attr", routine: OpcodeRoutines.clear_attr);
            AddOpcode(OpcodeKind.TwoOp, 0x0d, "store", OpcodeFlags.FirstOpByRef, OpcodeRoutines.store);
            AddOpcode(OpcodeKind.TwoOp, 0x0e, "insert_obj", routine: OpcodeRoutines.insert_obj);
            AddOpcode(OpcodeKind.TwoOp, 0x0f, "loadw", OpcodeFlags.Store, OpcodeRoutines.loadw);
            AddOpcode(OpcodeKind.TwoOp, 0x10, "loadb", OpcodeFlags.Store, OpcodeRoutines.loadb);
            AddOpcode(OpcodeKind.TwoOp, 0x11, "get_prop", OpcodeFlags.Store, OpcodeRoutines.get_prop);
            AddOpcode(OpcodeKind.TwoOp, 0x12, "get_prop_addr", OpcodeFlags.Store, OpcodeRoutines.get_prop_addr);
            AddOpcode(OpcodeKind.TwoOp, 0x13, "get_next_prop", OpcodeFlags.Store, OpcodeRoutines.get_next_prop);
            AddOpcode(OpcodeKind.TwoOp, 0x14, "add", OpcodeFlags.Store, OpcodeRoutines.add);
            AddOpcode(OpcodeKind.TwoOp, 0x15, "sub", OpcodeFlags.Store, OpcodeRoutines.sub);
            AddOpcode(OpcodeKind.TwoOp, 0x16, "mul", OpcodeFlags.Store, OpcodeRoutines.mul);
            AddOpcode(OpcodeKind.TwoOp, 0x17, "div", OpcodeFlags.Store, OpcodeRoutines.div);
            AddOpcode(OpcodeKind.TwoOp, 0x18, "mod", OpcodeFlags.Store, OpcodeRoutines.mod);
            AddOpcode(OpcodeKind.TwoOp, 0x19, "call_2s", OpcodeFlags.Call | OpcodeFlags.Store, OpcodeRoutines.call_2s, fromVersion: 4);
            AddOpcode(OpcodeKind.TwoOp, 0x1a, "call_2n", OpcodeFlags.Call, OpcodeRoutines.call_2n, fromVersion: 5);
            AddOpcode(OpcodeKind.TwoOp, 0x1b, "set_color", fromVersion: 5, toVersion: 5);
            AddOpcode(OpcodeKind.TwoOp, 0x1b, "set_color", fromVersion: 6);
            AddOpcode(OpcodeKind.TwoOp, 0x1c, "throw", fromVersion: 5);

            // one-operand opcodes
            AddOpcode(OpcodeKind.OneOp, 0x00, "jz", OpcodeFlags.Branch, OpcodeRoutines.jz);
            AddOpcode(OpcodeKind.OneOp, 0x01, "get_sibling", OpcodeFlags.Store | OpcodeFlags.Branch, OpcodeRoutines.get_sibling);
            AddOpcode(OpcodeKind.OneOp, 0x02, "get_child", OpcodeFlags.Store | OpcodeFlags.Branch, OpcodeRoutines.get_child);
            AddOpcode(OpcodeKind.OneOp, 0x03, "get_parent", OpcodeFlags.Store, OpcodeRoutines.get_parent);
            AddOpcode(OpcodeKind.OneOp, 0x04, "get_prop_len", OpcodeFlags.Store, OpcodeRoutines.get_prop_len);
            AddOpcode(OpcodeKind.OneOp, 0x05, "inc", OpcodeFlags.FirstOpByRef, OpcodeRoutines.inc);
            AddOpcode(OpcodeKind.OneOp, 0x06, "dec", OpcodeFlags.FirstOpByRef, OpcodeRoutines.dec);
            AddOpcode(OpcodeKind.OneOp, 0x07, "print_addr", routine: OpcodeRoutines.print_addr);
            AddOpcode(OpcodeKind.OneOp, 0x08, "call_1s", OpcodeFlags.Call | OpcodeFlags.Store, OpcodeRoutines.call_1s, fromVersion: 4);
            AddOpcode(OpcodeKind.OneOp, 0x09, "remove_obj", routine: OpcodeRoutines.remove_obj);
            AddOpcode(OpcodeKind.OneOp, 0x0a, "print_obj", routine: OpcodeRoutines.print_obj);
            AddOpcode(OpcodeKind.OneOp, 0x0b, "ret", OpcodeFlags.Return, OpcodeRoutines.ret);
            AddOpcode(OpcodeKind.OneOp, 0x0c, "jump", routine: OpcodeRoutines.jump);
            AddOpcode(OpcodeKind.OneOp, 0x0d, "print_paddr", routine: OpcodeRoutines.print_paddr);
            AddOpcode(OpcodeKind.OneOp, 0x0e, "load", OpcodeFlags.FirstOpByRef | OpcodeFlags.Store, OpcodeRoutines.load);
            AddOpcode(OpcodeKind.OneOp, 0x0f, "not", OpcodeFlags.Store, OpcodeRoutines.not, toVersion: 4);
            AddOpcode(OpcodeKind.OneOp, 0x0f, "call_1n", OpcodeFlags.Call, OpcodeRoutines.call_1n, fromVersion: 5);

            // zero-operand opcodes
            AddOpcode(OpcodeKind.ZeroOp, 0x00, "rtrue", OpcodeFlags.Return, OpcodeRoutines.rtrue);
            AddOpcode(OpcodeKind.ZeroOp, 0x01, "rfalse", OpcodeFlags.Return, OpcodeRoutines.rfalse);
            AddOpcode(OpcodeKind.ZeroOp, 0x02, "print", OpcodeFlags.ZText, OpcodeRoutines.print);
            AddOpcode(OpcodeKind.ZeroOp, 0x03, "print_ret", OpcodeFlags.Return | OpcodeFlags.ZText, OpcodeRoutines.print_ret);
            AddOpcode(OpcodeKind.ZeroOp, 0x04, "nop");
            AddOpcode(OpcodeKind.ZeroOp, 0x05, "save", OpcodeFlags.Branch, toVersion: 3);
            AddOpcode(OpcodeKind.ZeroOp, 0x05, "save", OpcodeFlags.Store, fromVersion: 4, toVersion: 4);
            AddOpcode(OpcodeKind.ZeroOp, 0x06, "restore", OpcodeFlags.Branch, toVersion: 3);
            AddOpcode(OpcodeKind.ZeroOp, 0x06, "restore", OpcodeFlags.Store, fromVersion: 4, toVersion: 4);
            AddOpcode(OpcodeKind.ZeroOp, 0x07, "restart");
            AddOpcode(OpcodeKind.ZeroOp, 0x08, "ret_popped", OpcodeFlags.Return, OpcodeRoutines.ret_popped);
            AddOpcode(OpcodeKind.ZeroOp, 0x09, "pop", toVersion: 4);
            AddOpcode(OpcodeKind.ZeroOp, 0x09, "catch", OpcodeFlags.Store, fromVersion: 5);
            AddOpcode(OpcodeKind.ZeroOp, 0x0a, "quit", routine: OpcodeRoutines.quit);
            AddOpcode(OpcodeKind.ZeroOp, 0x0b, "new_line", routine: OpcodeRoutines.new_line);
            AddOpcode(OpcodeKind.ZeroOp, 0x0c, "show_status", fromVersion: 3, toVersion: 3);
            AddOpcode(OpcodeKind.ZeroOp, 0x0d, "verify", OpcodeFlags.Branch, OpcodeRoutines.verify, fromVersion: 3);
            AddOpcode(OpcodeKind.ZeroOp, 0x0f, "piracy", OpcodeFlags.Branch, OpcodeRoutines.piracy, fromVersion: 5);

            // variable-operand opcodes
            AddOpcode(OpcodeKind.VarOp, 0x00, "call", OpcodeFlags.Call | OpcodeFlags.Store, OpcodeRoutines.call_vs, toVersion: 4);
            AddOpcode(OpcodeKind.VarOp, 0x00, "call_vs", OpcodeFlags.Call | OpcodeFlags.Store, OpcodeRoutines.call_vs, fromVersion: 5);
            AddOpcode(OpcodeKind.VarOp, 0x01, "storew", routine: OpcodeRoutines.storew);
            AddOpcode(OpcodeKind.VarOp, 0x02, "storeb", routine: OpcodeRoutines.storeb);
            AddOpcode(OpcodeKind.VarOp, 0x03, "put_prop", routine: OpcodeRoutines.put_prop);
            AddOpcode(OpcodeKind.VarOp, 0x04, "sread", toVersion: 3);
            AddOpcode(OpcodeKind.VarOp, 0x04, "sread", fromVersion: 4, toVersion: 4);
            AddOpcode(OpcodeKind.VarOp, 0x04, "aread", OpcodeFlags.Store, fromVersion: 5);
            AddOpcode(OpcodeKind.VarOp, 0x05, "print_char", routine: OpcodeRoutines.print_char);
            AddOpcode(OpcodeKind.VarOp, 0x06, "print_num", routine: OpcodeRoutines.print_num);
            AddOpcode(OpcodeKind.VarOp, 0x07, "random", OpcodeFlags.Store, OpcodeRoutines.random);
            AddOpcode(OpcodeKind.VarOp, 0x08, "push", routine: OpcodeRoutines.push);
            AddOpcode(OpcodeKind.VarOp, 0x09, "pull", OpcodeFlags.FirstOpByRef, OpcodeRoutines.pull, toVersion: 5);
            AddOpcode(OpcodeKind.VarOp, 0x09, "pull", OpcodeFlags.Store, fromVersion: 6, toVersion: 6);
            AddOpcode(OpcodeKind.VarOp, 0x09, "pull", OpcodeFlags.FirstOpByRef, OpcodeRoutines.pull, fromVersion: 7, toVersion: 8);
            AddOpcode(OpcodeKind.VarOp, 0x0a, "split_window", routine: OpcodeRoutines.split_window, fromVersion: 3);
            AddOpcode(OpcodeKind.VarOp, 0x0b, "set_window", routine: OpcodeRoutines.set_window, fromVersion: 3);
            AddOpcode(OpcodeKind.VarOp, 0x0c, "call_vs2", OpcodeFlags.Call | OpcodeFlags.Store | OpcodeFlags.DoubleVar, OpcodeRoutines.call_vs2, fromVersion: 4);
            AddOpcode(OpcodeKind.VarOp, 0x0d, "erase_window", routine: OpcodeRoutines.erase_window, fromVersion: 4);
            AddOpcode(OpcodeKind.VarOp, 0x0e, "erase_line", fromVersion: 4, toVersion: 5);
            AddOpcode(OpcodeKind.VarOp, 0x0e, "erase_line", fromVersion: 6);
            AddOpcode(OpcodeKind.VarOp, 0x0f, "set_cursor", routine: OpcodeRoutines.set_cursor, fromVersion: 4, toVersion: 5);
            AddOpcode(OpcodeKind.VarOp, 0x0f, "set_cursor", fromVersion: 6, toVersion: 6);
            AddOpcode(OpcodeKind.VarOp, 0x0f, "set_cursor", routine: OpcodeRoutines.set_cursor, fromVersion: 7);
            AddOpcode(OpcodeKind.VarOp, 0x10, "get_cursor", fromVersion: 4);
            AddOpcode(OpcodeKind.VarOp, 0x11, "set_text_style", routine: OpcodeRoutines.set_text_style, fromVersion: 4);
            AddOpcode(OpcodeKind.VarOp, 0x12, "buffer_mode", routine: OpcodeRoutines.buffer_mode, fromVersion: 4);
            AddOpcode(OpcodeKind.VarOp, 0x13, "output_stream", routine: OpcodeRoutines.output_stream1, fromVersion: 3, toVersion: 4);
            AddOpcode(OpcodeKind.VarOp, 0x13, "output_stream", routine: OpcodeRoutines.output_stream2, fromVersion: 5, toVersion: 5);
            AddOpcode(OpcodeKind.VarOp, 0x13, "output_stream", fromVersion: 6, toVersion: 6);
            AddOpcode(OpcodeKind.VarOp, 0x13, "output_stream", routine: OpcodeRoutines.output_stream2, fromVersion: 7);
            AddOpcode(OpcodeKind.VarOp, 0x14, "input_stream", fromVersion: 3);
            AddOpcode(OpcodeKind.VarOp, 0x15, "sound_effect", fromVersion: 3);
            AddOpcode(OpcodeKind.VarOp, 0x16, "sound_effect", OpcodeFlags.Store, fromVersion: 4);
            AddOpcode(OpcodeKind.VarOp, 0x17, "scan_table", OpcodeFlags.Store | OpcodeFlags.Branch, fromVersion: 4);
            AddOpcode(OpcodeKind.VarOp, 0x18, "not", OpcodeFlags.Store, OpcodeRoutines.not, fromVersion: 5);
            AddOpcode(OpcodeKind.VarOp, 0x19, "call_vn", OpcodeFlags.Call, OpcodeRoutines.call_vn, fromVersion: 5);
            AddOpcode(OpcodeKind.VarOp, 0x1a, "call_vn2", OpcodeFlags.Call | OpcodeFlags.DoubleVar, OpcodeRoutines.call_vn2, fromVersion: 5);
            AddOpcode(OpcodeKind.VarOp, 0x1b, "tokenize", fromVersion: 5);
            AddOpcode(OpcodeKind.VarOp, 0x1c, "encode_text", fromVersion: 5);
            AddOpcode(OpcodeKind.VarOp, 0x1d, "copy_table", fromVersion: 5);
            AddOpcode(OpcodeKind.VarOp, 0x1e, "print_table", fromVersion: 5);
            AddOpcode(OpcodeKind.VarOp, 0x1f, "check_arg_count", OpcodeFlags.Branch, OpcodeRoutines.check_arg_count, fromVersion: 5);

            // extended opcodes
            AddOpcode(OpcodeKind.Ext, 0x00, "save", OpcodeFlags.Store, fromVersion: 5);
            AddOpcode(OpcodeKind.Ext, 0x01, "restore", OpcodeFlags.Store, fromVersion: 5);
            AddOpcode(OpcodeKind.Ext, 0x02, "log_shift", OpcodeFlags.Store, OpcodeRoutines.log_shift, fromVersion: 5);
            AddOpcode(OpcodeKind.Ext, 0x03, "art_shift", OpcodeFlags.Store, OpcodeRoutines.art_shift, fromVersion: 5);
            AddOpcode(OpcodeKind.Ext, 0x04, "set_font", OpcodeFlags.Store, fromVersion: 5);
            AddOpcode(OpcodeKind.Ext, 0x05, "draw_picture", fromVersion: 6);
            AddOpcode(OpcodeKind.Ext, 0x06, "picture_data", OpcodeFlags.Branch, fromVersion: 6);
            AddOpcode(OpcodeKind.Ext, 0x07, "erase_picture", fromVersion: 6);
            AddOpcode(OpcodeKind.Ext, 0x08, "set_margins", fromVersion: 6);
            AddOpcode(OpcodeKind.Ext, 0x09, "save_undo", OpcodeFlags.Store, fromVersion: 5);
            AddOpcode(OpcodeKind.Ext, 0x0a, "restore_undo", OpcodeFlags.Store, fromVersion: 5);
            AddOpcode(OpcodeKind.Ext, 0x0b, "print_unicode", fromVersion: 5);
            AddOpcode(OpcodeKind.Ext, 0x0c, "check_unicode", fromVersion: 5);
            AddOpcode(OpcodeKind.Ext, 0x10, "move_window", fromVersion: 6);
            AddOpcode(OpcodeKind.Ext, 0x11, "window_size", fromVersion: 6);
            AddOpcode(OpcodeKind.Ext, 0x12, "window_style", fromVersion: 6);
            AddOpcode(OpcodeKind.Ext, 0x13, "get_wind_prop", OpcodeFlags.Store, fromVersion: 6);
            AddOpcode(OpcodeKind.Ext, 0x14, "scroll_window", fromVersion: 6);
            AddOpcode(OpcodeKind.Ext, 0x15, "pop_stack", fromVersion: 6);
            AddOpcode(OpcodeKind.Ext, 0x16, "read_mouse", fromVersion: 6);
            AddOpcode(OpcodeKind.Ext, 0x17, "mouse_window", fromVersion: 6);
            AddOpcode(OpcodeKind.Ext, 0x18, "push_stack", OpcodeFlags.Branch, fromVersion: 6);
            AddOpcode(OpcodeKind.Ext, 0x19, "put_wind_prop", fromVersion: 6);
            AddOpcode(OpcodeKind.Ext, 0x1a, "print_form", fromVersion: 6);
            AddOpcode(OpcodeKind.Ext, 0x1b, "make_menu", OpcodeFlags.Branch, fromVersion: 6);
            AddOpcode(OpcodeKind.Ext, 0x1c, "picture_table", fromVersion: 6);
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
