using System.Linq;
using System.Text;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler
{
    internal static class InstructionExtensions
    {
        private static bool Is(this Opcode op, OpcodeKind kind, int number)
        {
            return op.Kind == kind && op.Number == number;
        }

        public static bool UsesOutputStreams(this Instruction i)
        {
            var op = i.Opcode;

            return op.Is(OpcodeKind.ZeroOp, 0x02)  // print
                || op.Is(OpcodeKind.ZeroOp, 0x0b)  // new_line
                || op.Is(OpcodeKind.VarOp, 0x05)   // print_char
                || op.Is(OpcodeKind.VarOp, 0x06)   // print_num
                || op.Is(OpcodeKind.OneOp, 0x07)   // print_addr
                || op.Is(OpcodeKind.OneOp, 0x0a)   // print_obj
                || op.Is(OpcodeKind.OneOp, 0x0d)   // print_paddr
                || op.Is(OpcodeKind.ZeroOp, 0x03)  // print_ret
                || op.Is(OpcodeKind.VarOp, 0x13);  // output_stream
        }

        public static bool UsesScreen(this Instruction i)
        {
            var op = i.Opcode;

            return op.Is(OpcodeKind.ZeroOp, 0x02)  // print
                || op.Is(OpcodeKind.ZeroOp, 0x0b)  // new_line
                || op.Is(OpcodeKind.VarOp, 0x05)   // print_char
                || op.Is(OpcodeKind.VarOp, 0x06)   // print_num
                || op.Is(OpcodeKind.OneOp, 0x07)   // print_addr
                || op.Is(OpcodeKind.OneOp, 0x0a)   // print_obj
                || op.Is(OpcodeKind.OneOp, 0x0d)   // print_paddr
                || op.Is(OpcodeKind.ZeroOp, 0x03)  // print_ret
                || op.Is(OpcodeKind.VarOp, 0x0a)   // split_window
                || op.Is(OpcodeKind.VarOp, 0x0b)   // set_window
                || op.Is(OpcodeKind.TwoOp, 0x1b)   // set_color
                || op.Is(OpcodeKind.VarOp, 0x0f)   // set_cursor
                || op.Is(OpcodeKind.VarOp, 0x11);  // set_textStyle
        }

        public static bool UsesStack(this Instruction i)
        {
            // TODO: Need to check Z-Machine version
            var op = i.Opcode;
            if (op.Is(OpcodeKind.VarOp, 0x08) ||  // push
                op.Is(OpcodeKind.VarOp, 0x09) ||  // pull
                op.Is(OpcodeKind.ZeroOp, 0x08) || // ret_popped
                op.Is(OpcodeKind.ZeroOp, 0x09))   // pop
            {
                return true;
            }

            if (i.HasStoreVariable && i.StoreVariable.Kind == VariableKind.Stack)
            {
                return true;
            }

            if (i.Opcode.IsFirstOpByRef && i.Operands[0].Value == 0)
            {
                return true;
            }

            foreach (var o in i.Operands)
            {
                if (o.Kind == OperandKind.Variable && o.Value == 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool UsesMemory(this Instruction i)
        {
            // TODO: Need to check Z-Machine version
            var op = i.Opcode;
            if (op.Is(OpcodeKind.TwoOp, 0x06) ||  // jin
                op.Is(OpcodeKind.TwoOp, 0x0a) ||  // test_attr
                op.Is(OpcodeKind.TwoOp, 0x0b) ||  // set_attr
                op.Is(OpcodeKind.TwoOp, 0x0c) ||  // clear_attr
                op.Is(OpcodeKind.TwoOp, 0x0e) ||  // insert_obj
                op.Is(OpcodeKind.TwoOp, 0x0f) ||  // loadw
                op.Is(OpcodeKind.TwoOp, 0x10) ||  // loadb
                op.Is(OpcodeKind.TwoOp, 0x11) ||  // get_prop
                op.Is(OpcodeKind.TwoOp, 0x12) ||  // get_prop_addr
                op.Is(OpcodeKind.TwoOp, 0x13) ||  // get_next_prop
                op.Is(OpcodeKind.OneOp, 0x01) ||  // get_sibling
                op.Is(OpcodeKind.OneOp, 0x02) ||  // get_child
                op.Is(OpcodeKind.OneOp, 0x03) ||  // get_parent
                op.Is(OpcodeKind.OneOp, 0x04) ||  // get_prop_len
                op.Is(OpcodeKind.OneOp, 0x07) ||  // print_addr
                op.Is(OpcodeKind.OneOp, 0x09) ||  // remove_obj
                op.Is(OpcodeKind.OneOp, 0x0a) ||  // print_obj
                op.Is(OpcodeKind.OneOp, 0x0d) ||  // print_paddr
                op.Is(OpcodeKind.ZeroOp, 0x0d) || // verify
                op.Is(OpcodeKind.VarOp, 0x01) ||  // storew
                op.Is(OpcodeKind.VarOp, 0x02) ||  // storeb
                op.Is(OpcodeKind.VarOp, 0x03) ||  // put_prop
                op.Is(OpcodeKind.VarOp, 0x04) ||  // read
                op.Is(OpcodeKind.VarOp, 0x17) ||  // scan_table
                op.Is(OpcodeKind.VarOp, 0x1b) ||  // tokenize
                op.Is(OpcodeKind.VarOp, 0x1c) ||  // encode_text
                op.Is(OpcodeKind.VarOp, 0x1d) ||  // copy_table
                op.Is(OpcodeKind.VarOp, 0x1e) ||  // print_table
                op.Is(OpcodeKind.ZeroOp, 0x09))   // pop
            {
                return true;
            }

            if (i.HasStoreVariable && i.StoreVariable.Kind == VariableKind.Global)
            {
                return true;
            }

            if (i.Opcode.IsFirstOpByRef && i.Operands[0].Value > 0x0f)
            {
                return true;
            }

            foreach (var o in i.Operands)
            {
                if (o.Kind == OperandKind.Variable && o.Value > 0x0f)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsInterruptable(this Instruction i)
        {
            var op = i.Opcode;
            return !(op.Is(OpcodeKind.TwoOp, 0x14) || // add
                op.Is(OpcodeKind.TwoOp, 0x15) ||      // sub
                op.Is(OpcodeKind.TwoOp, 0x16) ||      // mul
                op.Is(OpcodeKind.TwoOp, 0x17) ||      // div
                op.Is(OpcodeKind.TwoOp, 0x18));       // mod
        }

        private static string PrettyPrint(this Variable var, bool @out = false)
        {
            switch (var.Kind)
            {
                case VariableKind.Stack:
                    return @out ? "-(SP)" : "(SP)+";
                default:
                    return var.ToString();
            }
        }

        private static string PrettyPrint(this Operand op)
        {
            switch (op.Kind)
            {
                case OperandKind.LargeConstant:
                    return "#" + op.Value.ToString("x4");
                case OperandKind.SmallConstant:
                    return "#" + op.Value.ToString("x2");
                default: // OperandKind.Variable
                    return Variable.FromByte((byte)op.Value).PrettyPrint();
            }
        }

        private static string PrettyPrintByRef(this Operand op)
        {
            if (op.Kind == OperandKind.SmallConstant)
            {
                return Variable.FromByte((byte)op.Value).PrettyPrint();
            }
            else if (op.Kind == OperandKind.Variable)
            {
                return "[" + op.PrettyPrint() + "]";
            }
            else // OperandKind.LargeConstant
            {
                throw new ZCompilerException("ByRef operand must be a small constant or a variable.");
            }
        }

        public static string PrettyPrint(this Instruction i, CompiledZMachine machine)
        {
            var builder = new StringBuilder();

            builder.AppendFormat("{0:x4}: {1}", i.Address, i.Opcode.Name);

            if (i.OperandCount > 0)
            {
                builder.Append(" ");

                if (i.Opcode.IsCall)
                {
                    if (i.Operands[0].Kind != OperandKind.Variable)
                    {
                        builder.Append(machine.UnpackRoutineAddress(i.Operands[0].Value).ToString("x4"));
                    }
                    else
                    {
                        builder.Append(Variable.FromByte((byte)i.Operands[0].Value));
                    }

                    if (i.OperandCount > 1)
                    {
                        builder.Append(" (");
                        builder.Append(string.Join(", ", i.Operands.Skip(1).Select(op => op.PrettyPrint())));
                        builder.Append(")");
                    }
                }
                else if (i.Opcode.IsJump)
                {
                    var jumpOffset = (short)i.Operands[0].Value;
                    var jumpAddress = i.Address + i.Length + jumpOffset - 2;
                    builder.Append(jumpAddress.ToString("x4"));
                }
                else if (i.Opcode.IsFirstOpByRef)
                {
                    builder.Append(i.Operands[0].PrettyPrintByRef());

                    if (i.OperandCount > 1)
                    {
                        builder.Append(", ");
                        builder.Append(string.Join(", ", i.Operands.Skip(1).Select(op => op.PrettyPrint())));
                    }
                }
                else
                {
                    builder.Append(string.Join(", ", i.Operands.Select(op => op.PrettyPrint())));
                }
            }

            if (i.HasZText)
            {
                var ztext = machine.ConvertZText(i.ZText);
                ztext = ztext.Replace("\n", @"\n").Replace(' ', '\u00b7').Replace("\t", @"\t");
                builder.Append(" ");
                builder.Append(ztext);
            }

            if (i.HasStoreVariable)
            {
                builder.Append(" -> ");
                builder.Append(i.StoreVariable.PrettyPrint(@out: true));
            }

            if (i.HasBranch)
            {
                builder.Append(" [");
                builder.Append(i.Branch.Condition.ToString().ToUpper());
                builder.Append("] ");

                switch (i.Branch.Kind)
                {
                    case BranchKind.Address:
                        var jumpAddress = i.Address + i.Length + i.Branch.Offset - 2;
                        builder.Append(jumpAddress.ToString("x4"));
                        break;

                    case BranchKind.RFalse:
                        builder.Append("rfalse");
                        break;

                    case BranchKind.RTrue:
                        builder.Append("rtrue");
                        break;
                }
            }

            return builder.ToString();
        }
    }
}
