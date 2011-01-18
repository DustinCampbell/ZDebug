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
                    builder.il.Emit(OpCodes.Br_S, label);
                }
                else
                {
                    builder.il.Emit(OpCodes.Br, label);
                }
            }

            public void BranchIf(Condition condition, bool @short = false)
            {
                switch (condition)
                {
                    case Condition.False:
                        if (@short)
                        {
                            builder.il.Emit(OpCodes.Brfalse_S, label);
                        }
                        else
                        {
                            builder.il.Emit(OpCodes.Brfalse, label);
                        }

                        break;

                    case Condition.True:
                        if (@short)
                        {
                            builder.il.Emit(OpCodes.Brtrue_S, label);
                        }
                        else
                        {
                            builder.il.Emit(OpCodes.Brtrue, label);
                        }

                        break;

                    case Condition.NotEqual:
                        if (@short)
                        {
                            builder.il.Emit(OpCodes.Bne_Un_S, label);
                        }
                        else
                        {
                            builder.il.Emit(OpCodes.Bne_Un, label);
                        }

                        break;

                    case Condition.Equal:
                        if (@short)
                        {
                            builder.il.Emit(OpCodes.Beq_S, label);
                        }
                        else
                        {
                            builder.il.Emit(OpCodes.Beq, label);
                        }

                        break;

                    case Condition.AtLeast:
                        if (@short)
                        {
                            builder.il.Emit(OpCodes.Bge_S, label);
                        }
                        else
                        {
                            builder.il.Emit(OpCodes.Bge, label);
                        }

                        break;

                    case Condition.AtMost:
                        if (@short)
                        {
                            builder.il.Emit(OpCodes.Ble_S, label);
                        }
                        else
                        {
                            builder.il.Emit(OpCodes.Ble, label);
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
