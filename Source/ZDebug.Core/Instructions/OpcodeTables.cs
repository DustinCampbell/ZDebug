
namespace ZDebug.Core.Instructions
{
    public static class OpcodeTables
    {
        private static readonly OpcodeTable[] opcodeTables;

        static OpcodeTables()
        {
            opcodeTables = new OpcodeTable[8];
            for (byte i = 0; i < 8; i++)
            {
                opcodeTables[i] = new OpcodeTable((byte)(i + 1));
            }

            // two-operand opcodes
            AddOpcode(OpcodeKind.TwoOp, 0x01, "je", OpcodeFlags.Branch, new OpcodeRoutines.je());
            AddOpcode(OpcodeKind.TwoOp, 0x02, "jl", OpcodeFlags.Branch, new OpcodeRoutines.jl());
            AddOpcode(OpcodeKind.TwoOp, 0x03, "jg", OpcodeFlags.Branch, new OpcodeRoutines.jg());
            AddOpcode(OpcodeKind.TwoOp, 0x04, "dec_chk", OpcodeFlags.Branch | OpcodeFlags.FirstOpByRef, new OpcodeRoutines.dec_chk());
            AddOpcode(OpcodeKind.TwoOp, 0x05, "inc_chk", OpcodeFlags.Branch | OpcodeFlags.FirstOpByRef, new OpcodeRoutines.inc_chk());
            AddOpcode(OpcodeKind.TwoOp, 0x06, "jin", OpcodeFlags.Branch, new OpcodeRoutines.jin());
            AddOpcode(OpcodeKind.TwoOp, 0x07, "test", OpcodeFlags.Branch, new OpcodeRoutines.test());
            AddOpcode(OpcodeKind.TwoOp, 0x08, "or", OpcodeFlags.Store, new OpcodeRoutines.or());
            AddOpcode(OpcodeKind.TwoOp, 0x09, "and", OpcodeFlags.Store, new OpcodeRoutines.and());
            AddOpcode(OpcodeKind.TwoOp, 0x0a, "test_attr", OpcodeFlags.Branch, new OpcodeRoutines.test_attr());
            AddOpcode(OpcodeKind.TwoOp, 0x0b, "set_attr", routine: new OpcodeRoutines.set_attr());
            AddOpcode(OpcodeKind.TwoOp, 0x0c, "clear_attr", routine: new OpcodeRoutines.clear_attr());
            AddOpcode(OpcodeKind.TwoOp, 0x0d, "store", OpcodeFlags.FirstOpByRef, new OpcodeRoutines.store());
            AddOpcode(OpcodeKind.TwoOp, 0x0e, "insert_obj", routine: new OpcodeRoutines.insert_obj());
            AddOpcode(OpcodeKind.TwoOp, 0x0f, "loadw", OpcodeFlags.Store, new OpcodeRoutines.loadw());
            AddOpcode(OpcodeKind.TwoOp, 0x10, "loadb", OpcodeFlags.Store, new OpcodeRoutines.loadb());
            AddOpcode(OpcodeKind.TwoOp, 0x11, "get_prop", OpcodeFlags.Store, new OpcodeRoutines.get_prop());
            AddOpcode(OpcodeKind.TwoOp, 0x12, "get_prop_addr", OpcodeFlags.Store, new OpcodeRoutines.get_prop_addr());
            AddOpcode(OpcodeKind.TwoOp, 0x13, "get_next_prop", OpcodeFlags.Store, new OpcodeRoutines.get_next_prop());
            AddOpcode(OpcodeKind.TwoOp, 0x14, "add", OpcodeFlags.Store, new OpcodeRoutines.add());
            AddOpcode(OpcodeKind.TwoOp, 0x15, "sub", OpcodeFlags.Store, new OpcodeRoutines.sub());
            AddOpcode(OpcodeKind.TwoOp, 0x16, "mul", OpcodeFlags.Store, new OpcodeRoutines.mul());
            AddOpcode(OpcodeKind.TwoOp, 0x17, "div", OpcodeFlags.Store, new OpcodeRoutines.div());
            AddOpcode(OpcodeKind.TwoOp, 0x18, "mod", OpcodeFlags.Store, new OpcodeRoutines.mod());
            AddOpcode(OpcodeKind.TwoOp, 0x19, "call_2s", OpcodeFlags.Call | OpcodeFlags.Store, new OpcodeRoutines.call_2s(), fromVersion: 4);
            AddOpcode(OpcodeKind.TwoOp, 0x1a, "call_2n", OpcodeFlags.Call, new OpcodeRoutines.call_2n(), fromVersion: 5);
            AddOpcode(OpcodeKind.TwoOp, 0x1b, "set_color", routine: new OpcodeRoutines.set_color(), fromVersion: 5, toVersion: 5);
            AddOpcode(OpcodeKind.TwoOp, 0x1b, "set_color", fromVersion: 6, toVersion: 6);
            AddOpcode(OpcodeKind.TwoOp, 0x1b, "set_color", routine: new OpcodeRoutines.set_color(), fromVersion: 7);
            AddOpcode(OpcodeKind.TwoOp, 0x1c, "throw", fromVersion: 5);

            // one-operand opcodes
            AddOpcode(OpcodeKind.OneOp, 0x00, "jz", OpcodeFlags.Branch, new OpcodeRoutines.jz());
            AddOpcode(OpcodeKind.OneOp, 0x01, "get_sibling", OpcodeFlags.Store | OpcodeFlags.Branch, new OpcodeRoutines.get_sibling());
            AddOpcode(OpcodeKind.OneOp, 0x02, "get_child", OpcodeFlags.Store | OpcodeFlags.Branch, new OpcodeRoutines.get_child());
            AddOpcode(OpcodeKind.OneOp, 0x03, "get_parent", OpcodeFlags.Store, new OpcodeRoutines.get_parent());
            AddOpcode(OpcodeKind.OneOp, 0x04, "get_prop_len", OpcodeFlags.Store, new OpcodeRoutines.get_prop_len());
            AddOpcode(OpcodeKind.OneOp, 0x05, "inc", OpcodeFlags.FirstOpByRef, new OpcodeRoutines.inc());
            AddOpcode(OpcodeKind.OneOp, 0x06, "dec", OpcodeFlags.FirstOpByRef, new OpcodeRoutines.dec());
            AddOpcode(OpcodeKind.OneOp, 0x07, "print_addr", routine: new OpcodeRoutines.print_addr());
            AddOpcode(OpcodeKind.OneOp, 0x08, "call_1s", OpcodeFlags.Call | OpcodeFlags.Store, new OpcodeRoutines.call_1s(), fromVersion: 4);
            AddOpcode(OpcodeKind.OneOp, 0x09, "remove_obj", routine: new OpcodeRoutines.remove_obj());
            AddOpcode(OpcodeKind.OneOp, 0x0a, "print_obj", routine: new OpcodeRoutines.print_obj());
            AddOpcode(OpcodeKind.OneOp, 0x0b, "ret", OpcodeFlags.Return, new OpcodeRoutines.ret());
            AddOpcode(OpcodeKind.OneOp, 0x0c, "jump", routine: new OpcodeRoutines.jump());
            AddOpcode(OpcodeKind.OneOp, 0x0d, "print_paddr", routine: new OpcodeRoutines.print_paddr());
            AddOpcode(OpcodeKind.OneOp, 0x0e, "load", OpcodeFlags.FirstOpByRef | OpcodeFlags.Store, new OpcodeRoutines.load());
            AddOpcode(OpcodeKind.OneOp, 0x0f, "not", OpcodeFlags.Store, new OpcodeRoutines.not(), toVersion: 4);
            AddOpcode(OpcodeKind.OneOp, 0x0f, "call_1n", OpcodeFlags.Call, new OpcodeRoutines.call_1n(), fromVersion: 5);

            // zero-operand opcodes
            AddOpcode(OpcodeKind.ZeroOp, 0x00, "rtrue", OpcodeFlags.Return, new OpcodeRoutines.rtrue());
            AddOpcode(OpcodeKind.ZeroOp, 0x01, "rfalse", OpcodeFlags.Return, new OpcodeRoutines.rfalse());
            AddOpcode(OpcodeKind.ZeroOp, 0x02, "print", OpcodeFlags.ZText, new OpcodeRoutines.print());
            AddOpcode(OpcodeKind.ZeroOp, 0x03, "print_ret", OpcodeFlags.Return | OpcodeFlags.ZText, new OpcodeRoutines.print_ret());
            AddOpcode(OpcodeKind.ZeroOp, 0x04, "nop");
            AddOpcode(OpcodeKind.ZeroOp, 0x05, "save", OpcodeFlags.Branch, toVersion: 3);
            AddOpcode(OpcodeKind.ZeroOp, 0x05, "save", OpcodeFlags.Store, fromVersion: 4, toVersion: 4);
            AddOpcode(OpcodeKind.ZeroOp, 0x06, "restore", OpcodeFlags.Branch, toVersion: 3);
            AddOpcode(OpcodeKind.ZeroOp, 0x06, "restore", OpcodeFlags.Store, fromVersion: 4, toVersion: 4);
            AddOpcode(OpcodeKind.ZeroOp, 0x07, "restart");
            AddOpcode(OpcodeKind.ZeroOp, 0x08, "ret_popped", OpcodeFlags.Return, new OpcodeRoutines.ret_popped());
            AddOpcode(OpcodeKind.ZeroOp, 0x09, "pop", toVersion: 4);
            AddOpcode(OpcodeKind.ZeroOp, 0x09, "catch", OpcodeFlags.Store, fromVersion: 5);
            AddOpcode(OpcodeKind.ZeroOp, 0x0a, "quit", routine: new OpcodeRoutines.quit());
            AddOpcode(OpcodeKind.ZeroOp, 0x0b, "new_line", routine: new OpcodeRoutines.new_line());
            AddOpcode(OpcodeKind.ZeroOp, 0x0c, "show_status", routine: new OpcodeRoutines.show_status(), fromVersion: 3, toVersion: 3);
            AddOpcode(OpcodeKind.ZeroOp, 0x0d, "verify", OpcodeFlags.Branch, new OpcodeRoutines.verify(), fromVersion: 3);
            AddOpcode(OpcodeKind.ZeroOp, 0x0f, "piracy", OpcodeFlags.Branch, new OpcodeRoutines.piracy(), fromVersion: 5);

            // variable-operand opcodes
            AddOpcode(OpcodeKind.VarOp, 0x00, "call", OpcodeFlags.Call | OpcodeFlags.Store, new OpcodeRoutines.call_vs(), toVersion: 4);
            AddOpcode(OpcodeKind.VarOp, 0x00, "call_vs", OpcodeFlags.Call | OpcodeFlags.Store, new OpcodeRoutines.call_vs(), fromVersion: 5);
            AddOpcode(OpcodeKind.VarOp, 0x01, "storew", routine: new OpcodeRoutines.storew());
            AddOpcode(OpcodeKind.VarOp, 0x02, "storeb", routine: new OpcodeRoutines.storeb());
            AddOpcode(OpcodeKind.VarOp, 0x03, "put_prop", routine: new OpcodeRoutines.put_prop());
            AddOpcode(OpcodeKind.VarOp, 0x04, "sread", routine: new OpcodeRoutines.sread1(), toVersion: 3);
            AddOpcode(OpcodeKind.VarOp, 0x04, "sread", routine: new OpcodeRoutines.sread2(), fromVersion: 4, toVersion: 4);
            AddOpcode(OpcodeKind.VarOp, 0x04, "aread", OpcodeFlags.Store, new OpcodeRoutines.aread(), fromVersion: 5);
            AddOpcode(OpcodeKind.VarOp, 0x05, "print_char", routine: new OpcodeRoutines.print_char());
            AddOpcode(OpcodeKind.VarOp, 0x06, "print_num", routine: new OpcodeRoutines.print_num());
            AddOpcode(OpcodeKind.VarOp, 0x07, "random", OpcodeFlags.Store, new OpcodeRoutines.random());
            AddOpcode(OpcodeKind.VarOp, 0x08, "push", routine: new OpcodeRoutines.push());
            AddOpcode(OpcodeKind.VarOp, 0x09, "pull", OpcodeFlags.FirstOpByRef, new OpcodeRoutines.pull(), toVersion: 5);
            AddOpcode(OpcodeKind.VarOp, 0x09, "pull", OpcodeFlags.Store, fromVersion: 6, toVersion: 6);
            AddOpcode(OpcodeKind.VarOp, 0x09, "pull", OpcodeFlags.FirstOpByRef, new OpcodeRoutines.pull(), fromVersion: 7, toVersion: 8);
            AddOpcode(OpcodeKind.VarOp, 0x0a, "split_window", routine: new OpcodeRoutines.split_window(), fromVersion: 3);
            AddOpcode(OpcodeKind.VarOp, 0x0b, "set_window", routine: new OpcodeRoutines.set_window(), fromVersion: 3);
            AddOpcode(OpcodeKind.VarOp, 0x0c, "call_vs2", OpcodeFlags.Call | OpcodeFlags.Store | OpcodeFlags.DoubleVar, new OpcodeRoutines.call_vs2(), fromVersion: 4);
            AddOpcode(OpcodeKind.VarOp, 0x0d, "erase_window", routine: new OpcodeRoutines.erase_window(), fromVersion: 4);
            AddOpcode(OpcodeKind.VarOp, 0x0e, "erase_line", fromVersion: 4, toVersion: 5);
            AddOpcode(OpcodeKind.VarOp, 0x0e, "erase_line", fromVersion: 6);
            AddOpcode(OpcodeKind.VarOp, 0x0f, "set_cursor", routine: new OpcodeRoutines.set_cursor(), fromVersion: 4, toVersion: 5);
            AddOpcode(OpcodeKind.VarOp, 0x0f, "set_cursor", fromVersion: 6, toVersion: 6);
            AddOpcode(OpcodeKind.VarOp, 0x0f, "set_cursor", routine: new OpcodeRoutines.set_cursor(), fromVersion: 7);
            AddOpcode(OpcodeKind.VarOp, 0x10, "get_cursor", fromVersion: 4);
            AddOpcode(OpcodeKind.VarOp, 0x11, "set_text_style", routine: new OpcodeRoutines.set_text_style(), fromVersion: 4);
            AddOpcode(OpcodeKind.VarOp, 0x12, "buffer_mode", routine: new OpcodeRoutines.buffer_mode(), fromVersion: 4);
            AddOpcode(OpcodeKind.VarOp, 0x13, "output_stream", routine: new OpcodeRoutines.output_stream(), fromVersion: 3, toVersion: 4);
            AddOpcode(OpcodeKind.VarOp, 0x13, "output_stream", routine: new OpcodeRoutines.output_stream(), fromVersion: 5, toVersion: 5);
            AddOpcode(OpcodeKind.VarOp, 0x13, "output_stream", fromVersion: 6, toVersion: 6);
            AddOpcode(OpcodeKind.VarOp, 0x13, "output_stream", routine: new OpcodeRoutines.output_stream(), fromVersion: 7);
            AddOpcode(OpcodeKind.VarOp, 0x14, "input_stream", fromVersion: 3);
            AddOpcode(OpcodeKind.VarOp, 0x15, "sound_effect", fromVersion: 3);
            AddOpcode(OpcodeKind.VarOp, 0x16, "read_char", OpcodeFlags.Store, new OpcodeRoutines.read_char(), fromVersion: 4);
            AddOpcode(OpcodeKind.VarOp, 0x17, "scan_table", OpcodeFlags.Store | OpcodeFlags.Branch, new OpcodeRoutines.scan_table(), fromVersion: 4);
            AddOpcode(OpcodeKind.VarOp, 0x18, "not", OpcodeFlags.Store, new OpcodeRoutines.not(), fromVersion: 5);
            AddOpcode(OpcodeKind.VarOp, 0x19, "call_vn", OpcodeFlags.Call, new OpcodeRoutines.call_vn(), fromVersion: 5);
            AddOpcode(OpcodeKind.VarOp, 0x1a, "call_vn2", OpcodeFlags.Call | OpcodeFlags.DoubleVar, new OpcodeRoutines.call_vn2(), fromVersion: 5);
            AddOpcode(OpcodeKind.VarOp, 0x1b, "tokenize", routine: new OpcodeRoutines.tokenize(), fromVersion: 5);
            AddOpcode(OpcodeKind.VarOp, 0x1c, "encode_text", fromVersion: 5);
            AddOpcode(OpcodeKind.VarOp, 0x1d, "copy_table", routine: new OpcodeRoutines.copy_table(), fromVersion: 5);
            AddOpcode(OpcodeKind.VarOp, 0x1e, "print_table", routine: new OpcodeRoutines.print_table(), fromVersion: 5);
            AddOpcode(OpcodeKind.VarOp, 0x1f, "check_arg_count", OpcodeFlags.Branch, new OpcodeRoutines.check_arg_count(), fromVersion: 5);

            // extended opcodes
            AddOpcode(OpcodeKind.Ext, 0x00, "save", OpcodeFlags.Store, fromVersion: 5);
            AddOpcode(OpcodeKind.Ext, 0x01, "restore", OpcodeFlags.Store, fromVersion: 5);
            AddOpcode(OpcodeKind.Ext, 0x02, "log_shift", OpcodeFlags.Store, new OpcodeRoutines.log_shift(), fromVersion: 5);
            AddOpcode(OpcodeKind.Ext, 0x03, "art_shift", OpcodeFlags.Store, new OpcodeRoutines.art_shift(), fromVersion: 5);
            AddOpcode(OpcodeKind.Ext, 0x04, "set_font", OpcodeFlags.Store, new OpcodeRoutines.set_font(), fromVersion: 5);
            AddOpcode(OpcodeKind.Ext, 0x05, "draw_picture", fromVersion: 6);
            AddOpcode(OpcodeKind.Ext, 0x06, "picture_data", OpcodeFlags.Branch, fromVersion: 6);
            AddOpcode(OpcodeKind.Ext, 0x07, "erase_picture", fromVersion: 6);
            AddOpcode(OpcodeKind.Ext, 0x08, "set_margins", fromVersion: 6);
            AddOpcode(OpcodeKind.Ext, 0x09, "save_undo", OpcodeFlags.Store, new OpcodeRoutines.save_undo(), fromVersion: 5);
            AddOpcode(OpcodeKind.Ext, 0x0a, "restore_undo", OpcodeFlags.Store, new OpcodeRoutines.restore_undo(), fromVersion: 5);
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

        private static void AddOpcode(
            OpcodeKind kind,
            byte number,
            string name,
            OpcodeFlags flags = OpcodeFlags.None,
            OpcodeRoutine routine = null,
            byte fromVersion = 1,
            byte toVersion = 8)
        {
            for (byte v = fromVersion; v <= toVersion; v++)
            {
                opcodeTables[v - 1].Add(kind, number, name, flags, routine);
            }
        }

        internal static OpcodeTable GetOpcodeTable(byte version)
        {
            return opcodeTables[version - 1];
        }
    }
}
