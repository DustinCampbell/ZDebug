using System.Reflection.Emit;

namespace ZDebug.Compiler.Generate
{
    public sealed partial class ILBuilder
    {
        private class LabelWrapper : ILabel
        {
            private readonly ILBuilder builder;
            private readonly Label label;
            private bool marked;

            public LabelWrapper(ILBuilder builder)
            {
                this.builder = builder;
                this.label = builder.il.DefineLabel();
            }

            public void Mark()
            {
                if (marked)
                {
                    throw new ZCompilerException("Attempted to mark label that has already been marked.");
                }

                builder.il.MarkLabel(label);
                marked = true;
            }

            public void Branch(bool @short = false)
            {
                if (@short)
                {
                    builder.Emit(OpCodes.Br_S, label);
                }
                else
                {
                    builder.Emit(OpCodes.Br, label);
                }
            }

            public void BranchIf(Condition condition, bool @short = false)
            {
                switch (condition)
                {
                    case Condition.False:
                        if (@short)
                        {
                            builder.Emit(OpCodes.Brfalse_S, label);
                        }
                        else
                        {
                            builder.Emit(OpCodes.Brfalse, label);
                        }

                        break;

                    case Condition.True:
                        if (@short)
                        {
                            builder.Emit(OpCodes.Brtrue_S, label);
                        }
                        else
                        {
                            builder.Emit(OpCodes.Brtrue, label);
                        }

                        break;

                    case Condition.NotEqual:
                        if (@short)
                        {
                            builder.Emit(OpCodes.Bne_Un_S, label);
                        }
                        else
                        {
                            builder.Emit(OpCodes.Bne_Un, label);
                        }

                        break;

                    case Condition.Equal:
                        if (@short)
                        {
                            builder.Emit(OpCodes.Beq_S, label);
                        }
                        else
                        {
                            builder.Emit(OpCodes.Beq, label);
                        }

                        break;

                    case Condition.AtLeast:
                        if (@short)
                        {
                            builder.Emit(OpCodes.Bge_S, label);
                        }
                        else
                        {
                            builder.Emit(OpCodes.Bge, label);
                        }

                        break;

                    case Condition.AtMost:
                        if (@short)
                        {
                            builder.Emit(OpCodes.Ble_S, label);
                        }
                        else
                        {
                            builder.Emit(OpCodes.Ble, label);
                        }

                        break;

                    case Condition.LessThan:
                        if (@short)
                        {
                            builder.Emit(OpCodes.Blt_S, label);
                        }
                        else
                        {
                            builder.Emit(OpCodes.Blt, label);
                        }

                        break;

                    case Condition.GreaterThan:
                        if (@short)
                        {
                            builder.Emit(OpCodes.Bgt_S, label);
                        }
                        else
                        {
                            builder.Emit(OpCodes.Bgt, label);
                        }

                        break;

                    default:
                        throw new ZCompilerException(string.Format("Unsupported branch condition: {0} (short = {1})", condition, @short));
                }
            }
        }

        public ILabel NewLabel()
        {
            return new LabelWrapper(this);
        }
    }
}
