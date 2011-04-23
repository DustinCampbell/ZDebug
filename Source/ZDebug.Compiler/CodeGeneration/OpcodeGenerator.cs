using System.Diagnostics;
using ZDebug.Compiler.CodeGeneration.Generators;
using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration
{
    internal abstract class OpcodeGenerator
    {
        public readonly Instruction Instruction;

        protected OpcodeGenerator(Instruction instruction)
        {
            this.Instruction = instruction;
        }

        public abstract void Generate(ILBuilder il, ICompiler compiler);

        public static OpcodeGenerator GetGenerator(Instruction instruction, byte version)
        {
            var opcode = instruction.Opcode;

            switch (opcode.Kind)
            {
                case OpcodeKind.TwoOp:
                    switch (opcode.Number)
                    {
                        case 0x01:
                            return new JeGenerator(instruction);
                        case 0x02:
                            return new JlGenerator(instruction);
                        case 0x03:
                            return new JgGenerator(instruction);
                        //case 0x04:
                        //    return new DecChkGenerator(instruction);
                        //case 0x05:
                        //    return new IncChkGenerator(instruction);
                        case 0x06:
                            return new JinGenerator(instruction);
                        case 0x07:
                            return new TestGenerator(instruction);
                        case 0x08:
                            return new OrGenerator(instruction);
                        case 0x09:
                            return new AndGenerator(instruction);
                        //case 0x0a:
                        //    op_test_attr();
                        //    return;
                        //case 0x0b:
                        //    op_set_attr();
                        //    return;
                        //case 0x0c:
                        //    op_clear_attr();
                        //    return;
                        case 0x0d:
                            return new StoreGenerator(instruction);
                        //case 0x0e:
                        //    op_insert_obj();
                        //    return;
                        case 0x0f:
                            return new LoadWGenerator(instruction);
                        case 0x10:
                            return new LoadBGenerator(instruction);
                        //case 0x11:
                        //    op_get_prop();
                        //    return;
                        //case 0x12:
                        //    op_get_prop_addr();
                        //    return;
                        //case 0x13:
                        //    op_get_next_prop();
                        //    return;
                        case 0x14:
                            return new AddGenerator(instruction);
                        case 0x15:
                            return new SubGenerator(instruction);
                        case 0x16:
                            return new MulGenerator(instruction);
                        case 0x17:
                            return new DivGenerator(instruction);
                        case 0x18:
                            return new ModGenerator(instruction);
                        case 0x19:
                            if (version >= 4)
                            {
                                return new CallSGenerator(instruction);
                            }
                            break;
                        case 0x1a:
                            if (version >= 5)
                            {
                                return new CallNGenerator(instruction);
                            }
                            break;
                        //case 0x1b:
                        //    if (version == 6)
                        //    {
                        //        op_set_color6();
                        //        return;
                        //    }
                        //    if (version >= 5)
                        //    {
                        //        op_set_color();
                        //        return;
                        //    }
                        //    break;
                    }
                    break;
                case OpcodeKind.OneOp:
                    switch (opcode.Number)
                    {
                        case 0x00:
                            return new JzGenerator(instruction);
                        //case 0x01:
                        //    op_get_sibling();
                        //    return;
                        //case 0x02:
                        //    op_get_child();
                        //    return;
                        //case 0x03:
                        //    op_get_parent();
                        //    return;
                        //case 0x04:
                        //    op_get_prop_len();
                        //    return;
                        case 0x05:
                            return new IncGenerator(instruction);
                        case 0x06:
                            return new DecGenerator(instruction);
                        //case 0x07:
                        //    op_print_addr();
                        //    return;
                        case 0x08:
                            if (version >= 4)
                            {
                                return new CallSGenerator(instruction);
                            }
                            break;
                        //case 0x09:
                        //    op_remove_obj();
                        //    return;
                        //case 0x0a:
                        //    op_print_obj();
                        //    return;
                        case 0x0b:
                            return new RetGenerator(instruction);
                        case 0x0c:
                            return new JumpGenerator(instruction);
                        case 0x0d:
                            return new PrintPAddrGenerator(instruction);
                        case 0x0e:
                            return new LoadGenerator(instruction);
                        case 0x0f:
                            if (version >= 5)
                            {
                                return new CallNGenerator(instruction);
                            }
                            break;
                    }
                    break;
                case OpcodeKind.ZeroOp:
                    switch (opcode.Number)
                    {
                        case 0x00:
                            return new RtrueGenerator(instruction);
                        case 0x01:
                            return new RfalseGenerator(instruction);
                        case 0x02:
                            return new PrintGenerator(instruction);
                        //case 0x03:
                        //    op_print_ret();
                        //    return;
                        //case 0x05:
                        //    if (version <= 4)
                        //    {
                        //        op_save();
                        //        return;
                        //    }
                        //    break;
                        //case 0x06:
                        //    if (version <= 4)
                        //    {
                        //        op_restore();
                        //        return;
                        //    }
                        //    break;
                        //case 0x07:
                        //    op_restart();
                        //    return;
                        case 0x08:
                            return new RetPoppedGenerator(instruction);
                        //case 0x0a:
                        //    op_quit();
                        //    return;
                        //case 0x0b:
                        //    op_new_line();
                        //    return;
                        //case 0x0c:
                        //    if (version == 3)
                        //    {
                        //        op_show_status();
                        //        return;
                        //    }
                        //    break;
                        //case 0x0d:
                        //    if (version >= 3)
                        //    {
                        //        op_verify();
                        //        return;
                        //    }
                        //    break;
                        //case 0x0f:
                        //    if (version >= 5)
                        //    {
                        //        op_piracy();
                        //        return;
                        //    }
                        //    break;
                    }
                    break;
                case OpcodeKind.VarOp:
                    switch (opcode.Number)
                    {
                        case 0x00:
                            return new CallSGenerator(instruction);
                        case 0x01:
                            return new StoreWGenerator(instruction);
                        case 0x02:
                            return new StoreBGenerator(instruction);
                        //case 0x03:
                        //    op_put_prop();
                        //    return;
                        //case 0x04:
                        //    if (version < 4)
                        //    {
                        //        op_sread1();
                        //        return;
                        //    }
                        //    if (version == 4)
                        //    {
                        //        op_sread4();
                        //        return;
                        //    }
                        //    if (version > 4)
                        //    {
                        //        op_aread();
                        //        return;
                        //    }
                        //    break;
                        //case 0x05:
                        //    op_print_char();
                        //    return;
                        case 0x06:
                            return new PrintNumGenerator(instruction);
                        //case 0x07:
                        //    op_random();
                        //    return;
                        case 0x08:
                            return new PushGenerator(instruction);
                        case 0x09:
                            if (version != 6)
                            {
                                return new PullGenerator(instruction);
                            }
                            break;
                        //case 0x0a:
                        //    if (version >= 3)
                        //    {
                        //        op_split_window();
                        //        return;
                        //    }
                        //    break;
                        //case 0x0b:
                        //    if (version >= 3)
                        //    {
                        //        op_set_window();
                        //        return;
                        //    }
                        //    break;
                        //case 0x0d:
                        //    if (version >= 4)
                        //    {
                        //        op_erase_window();
                        //        return;
                        //    }
                        //    break;
                        //case 0x11:
                        //    if (version >= 4)
                        //    {
                        //        op_text_style();
                        //        return;
                        //    }
                        //    break;
                        //case 0x12:
                        //    if (version >= 4)
                        //    {
                        //        op_buffer_mode();
                        //        return;
                        //    }
                        //    break;
                        //case 0x13:
                        //    if (version >= 3 && version < 5)
                        //    {
                        //        op_output_stream();
                        //        return;
                        //    }
                        //    if (version == 6)
                        //    {
                        //        op_output_stream();
                        //        return;
                        //    }
                        //    if (version >= 5)
                        //    {
                        //        op_output_stream();
                        //        return;
                        //    }
                        //    break;
                        case 0x0c:
                            if (version >= 4)
                            {
                                return new CallSGenerator(instruction);
                            }
                            break;
                        //case 0x0f:
                        //    if (version == 6)
                        //    {
                        //        op_set_cursor6();
                        //        return;
                        //    }
                        //    if (version >= 4)
                        //    {
                        //        op_set_cursor();
                        //        return;
                        //    }
                        //    break;
                        //case 0x16:
                        //    if (version >= 4)
                        //    {
                        //        op_read_char();
                        //        return;
                        //    }
                        //    break;
                        case 0x17:
                            if (version >= 4)
                            {
                                return new ScanTableGenerator(instruction);
                            }
                            break;
                        case 0x18:
                            if (version >= 5)
                            {
                                return new NotGenerator(instruction);
                            }
                            break;
                        case 0x19:
                            if (version >= 5)
                            {
                                return new CallNGenerator(instruction);
                            }
                            break;
                        case 0x1a:
                            if (version >= 5)
                            {
                                return new CallNGenerator(instruction);
                            }
                            break;
                        //case 0x1b:
                        //    if (version >= 5)
                        //    {
                        //        op_tokenize();
                        //        return;
                        //    }
                        //    break;
                        case 0x1d:
                            if (version >= 5)
                            {
                                return new CopyTableGenerator(instruction);
                            }
                            break;
                        case 0x1f:
                            if (version >= 5)
                            {
                                return new CheckArgCountGenerator(instruction);
                            }
                            break;
                    }
                    break;
                case OpcodeKind.Ext:
                    switch (opcode.Number)
                    {
                        //case 0x00:
                        //    if (version >= 5)
                        //    {
                        //        op_save();
                        //        return;
                        //    }
                        //    break;
                        //case 0x01:
                        //    if (version >= 5)
                        //    {
                        //        op_restore();
                        //        return;
                        //    }
                        //    break;
                        case 0x02:
                            if (version >= 5)
                            {
                                return new LogShiftGenerator(instruction);
                            }
                            break;
                        case 0x03:
                            if (version >= 5)
                            {
                                return new ArtShiftGenerator(instruction);
                            }
                            break;
                        //case 0x09:
                        //    if (version >= 5)
                        //    {
                        //        op_save_undo();
                        //        return;
                        //    }
                        //    break;
                        //case 0x0a:
                        //    if (version >= 5)
                        //    {
                        //        op_restore_undo();
                        //        return;
                        //    }
                        //    break;
                    }
                    break;
            }

            Debug.WriteLine(
                string.Format("Generating unknown opcode: {0} ({1} {2:x2})", opcode.Name, opcode.Kind, opcode.Number));

            return new UnknownOpcodeGenerator(instruction);
        }
    }
}
