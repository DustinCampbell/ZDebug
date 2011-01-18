using ZDebug.Core.Instructions;
using System.Linq;
using System.Text;

namespace ZDebug.Compiler
{
    internal static class InstructionExtensions
    {
        private static bool Is(this Opcode op, OpcodeKind kind, int number)
        {
            return op.Kind == kind && op.Number == number;
        }

        public static bool UsesScreen(this Instruction i)
        {
            var op = i.Opcode;

            return op.Is(OpcodeKind.ZeroOp, 0x02)  // print
                || op.Is(OpcodeKind.ZeroOp, 0x0b)  // new_line
                || op.Is(OpcodeKind.OneOp, 0x0a)   // print_obj
                || op.Is(OpcodeKind.ZeroOp, 0x03); // print_ret
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

        public static string PrettyPrint(this Instruction i, ZMachine machine)
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
                var ztext = machine.ConvertEmbeddedZText(i.ZText);
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
