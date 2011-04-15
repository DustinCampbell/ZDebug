using System.Diagnostics;
using ZDebug.Compiler.CodeGeneration.Generators;
using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration
{
    internal abstract class OpcodeGenerator
    {
        public readonly OpcodeGeneratorKind Kind;

        protected OpcodeGenerator(OpcodeGeneratorKind kind)
        {
            this.Kind = kind;
        }

        public abstract void Generate(ILBuilder il, ICompiler compiler);


        public static OpcodeGenerator GetGenerator(Instruction instruction, byte version)
        {
            var opcode = instruction.Opcode;
            var opCount = instruction.OperandCount;
            var ops = instruction.Operands;

            switch (opcode.Kind)
            {
                case OpcodeKind.TwoOp:
                    switch (opcode.Number)
                    {
                        case 0x01:
                            return new JeGenerator(ops, instruction.Branch);
                        case 0x02:
                            return new JlGenerator(ops[0], ops[1], instruction.Branch);
                        case 0x03:
                            return new JgGenerator(ops[0], ops[1], instruction.Branch);
                        case 0x04:
                            return new DecChkGenerator(ops[0], ops[1], instruction.Branch);
                        case 0x05:
                            return new IncChkGenerator(ops[0], ops[1], instruction.Branch);
                        case 0x06:
                            return new JinGenerator(ops[0], ops[1], instruction.Branch);
                        case 0x07:
                            return new TestGenerator(ops[0], ops[1], instruction.Branch);
                        case 0x08:
                            return new OrGenerator(ops[0], ops[1], instruction.StoreVariable);
                        case 0x09:
                            return new AndGenerator(ops[0], ops[1], instruction.StoreVariable);
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
                            return new StoreGenerator(ops[0], ops[1]);
                        //case 0x0e:
                        //    op_insert_obj();
                        //    return;
                        case 0x0f:
                            return new LoadWGenerator(ops[0], ops[1], instruction.StoreVariable);
                        case 0x10:
                            return new LoadBGenerator(ops[0], ops[1], instruction.StoreVariable);
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
                            return new AddGenerator(ops[0], ops[1], instruction.StoreVariable);
                        case 0x15:
                            return new SubGenerator(ops[0], ops[1], instruction.StoreVariable);
                        case 0x16:
                            return new MulGenerator(ops[0], ops[1], instruction.StoreVariable);
                        case 0x17:
                            return new DivGenerator(ops[0], ops[1], instruction.StoreVariable);
                        case 0x18:
                            return new ModGenerator(ops[0], ops[1], instruction.StoreVariable);
                        case 0x19:
                            if (version >= 4)
                            {
                                return new CallSGenerator(instruction.StoreVariable);
                            }
                            break;
                        case 0x1a:
                            if (version >= 5)
                            {
                                return new CallNGenerator();
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
                            return new JzGenerator(ops[0], instruction.Branch);
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
                            return new IncGenerator(ops[0]);
                        case 0x06:
                            return new DecGenerator(ops[0]);
                        //case 0x07:
                        //    op_print_addr();
                        //    return;
                        case 0x08:
                            if (version >= 4)
                            {
                                return new CallSGenerator(instruction.StoreVariable);
                            }
                            break;
                        //case 0x09:
                        //    op_remove_obj();
                        //    return;
                        //case 0x0a:
                        //    op_print_obj();
                        //    return;
                        case 0x0b:
                            return new RetGenerator(ops[0]);
                        case 0x0c:
                            return new JumpGenerator(instruction.Address + instruction.Length, ops[0]);
                        //case 0x0d:
                        //    op_print_paddr();
                        //    return;
                        case 0x0e:
                            return new LoadGenerator(ops[0], instruction.StoreVariable);
                        case 0x0f:
                            if (version >= 5)
                            {
                                return new CallNGenerator();
                            }
                            break;
                    }
                    break;
                case OpcodeKind.ZeroOp:
                    switch (opcode.Number)
                    {
                        case 0x00:
                            return new RtrueGenerator();
                        case 0x01:
                            return new RfalseGenerator();
                        //case 0x02:
                        //    op_print();
                        //    return;
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
                            return new RetPoppedGenerator();
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
                            return new CallSGenerator(instruction.StoreVariable);
                        case 0x01:
                            return new StoreWGenerator(ops[0], ops[1], ops[2]);
                        case 0x02:
                            return new StoreBGenerator(ops[0], ops[1], ops[2]);
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
                        //case 0x06:
                        //    op_print_num();
                        //    return;
                        //case 0x07:
                        //    op_random();
                        //    return;
                        case 0x08:
                            return new PushGenerator(ops[0]);
                        case 0x09:
                            if (version != 6)
                            {
                                return new PullGenerator(ops[0]);
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
                                return new CallSGenerator(instruction.StoreVariable);
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
                                return new ScanTableGenerator(ops[0], ops[1], ops[2], (opCount > 3 ? ops[3] : (Operand?)null), instruction.StoreVariable, instruction.Branch);
                            }
                            break;
                        case 0x18:
                            if (version >= 5)
                            {
                                return new NotGenerator(ops[0], instruction.StoreVariable);
                            }
                            break;
                        case 0x19:
                            if (version >= 5)
                            {
                                return new CallNGenerator();
                            }
                            break;
                        case 0x1a:
                            if (version >= 5)
                            {
                                return new CallNGenerator();
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
                                return new CopyTableGenerator(ops[0], ops[2], ops[3]);
                            }
                            break;
                        //case 0x1f:
                        //    if (version >= 5)
                        //    {
                        //        op_check_arg_count();
                        //        return;
                        //    }
                        //    break;
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
                                return new LogShiftGenerator(ops[0], ops[1], instruction.StoreVariable);
                            }
                            break;
                        case 0x03:
                            if (version >= 5)
                            {
                                return new ArtShiftGenerator(ops[0], ops[1], instruction.StoreVariable);
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

            Debug.WriteLine(string.Format("Generating unknown opcode: {0} ({1} {2:x2})", opcode.Name, opcode.Kind, opcode.Number));
            return new UnknownOpcodeGenerator(instruction.Address, opcode);
        }
    }
}
