using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using ZDebug.Core.Instructions;

namespace ZDebug.UI.Controls
{
    internal partial class InstructionTextDisplayElement
    {
        private class VisualBuilder
        {
            private readonly VisualCollection visuals;
            private readonly Typeface typeface;
            private readonly Typeface ztextTypeface;
            private readonly double fontSize;
            private readonly Brush foreground;
            private readonly Brush background;
            private readonly double height;
            private double left;

            public VisualBuilder(
                VisualCollection visuals,
                Typeface typeface,
                double fontSize,
                Brush foreground,
                Brush background,
                double height)
            {
                this.visuals = visuals;
                this.typeface = typeface;
                this.ztextTypeface = new Typeface("Cambria");
                this.fontSize = fontSize;
                this.foreground = foreground;
                this.background = background;
                this.height = height;
            }

            private FormattedText CreateFormattedText(string text, Typeface typeface = null, Brush foreground = null)
            {
                typeface = typeface ?? this.typeface;
                foreground = foreground ?? this.foreground;
                return new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, fontSize, foreground);
            }

            private void AddVisual(string text, Typeface typeface = null, Brush foreground = null)
            {
                var visual = new DrawingVisual();
                var context = visual.RenderOpen();

                var formattedText = CreateFormattedText(text, typeface, foreground);
                var width = formattedText.WidthIncludingTrailingWhitespace;

                if (background != null)
                {
                    context.DrawRectangle(background, null, new Rect(left, 0.0, width, height));
                }

                context.DrawText(formattedText, new Point(left, 1.0));

                context.Close();

                visuals.Add(visual);

                left = left + width;
            }

            public void AddAddress(int address)
            {
                AddVisual(address.ToString("x4"));
            }

            public void AddConstant(ushort value)
            {
                AddVisual("#" + value.ToString("x4"));
            }

            public void AddConstant(byte value)
            {
                AddVisual("#" + value.ToString("x2"));
            }

            public void AddSeparator(string text)
            {
                AddVisual(text, foreground: Brushes.DarkSlateGray);
            }

            public void AddText(string text)
            {
                AddVisual(text);
            }

            public void AddVariable(Variable variable, bool @out = false)
            {
                if (variable.Kind == VariableKind.Stack)
                {
                    if (@out)
                    {
                        AddSeparator("-(");
                        AddVisual("SP");
                        AddSeparator(")");
                    }
                    else
                    {
                        AddSeparator("(");
                        AddVisual("SP");
                        AddSeparator(")+");
                    }
                }
                else // VariableKind.Local || VariableKind.Global
                {
                    AddVisual(variable.ToString());
                }
            }

            public void AddOperand(Operand operand)
            {
                if (operand.Kind == OperandKind.LargeConstant)
                {
                    AddConstant(operand.Value.RawValue);
                }
                else if (operand.Kind == OperandKind.SmallConstant)
                {
                    AddConstant((byte)operand.Value.RawValue);
                }
                else // OperandKind.Variable
                {
                    AddVariable(Variable.FromByte((byte)operand.Value.RawValue));
                }
            }

            public void AddOperands(IEnumerable<Operand> operands)
            {
                var firstOpAdded = false;

                foreach (var op in operands)
                {
                    if (firstOpAdded)
                    {
                        AddSeparator(", ");
                    }

                    AddOperand(op);

                    firstOpAdded = true;
                }
            }

            public void AddByRefOperand(Operand operand)
            {
                if (operand.Kind == OperandKind.SmallConstant)
                {
                    AddVariable(Variable.FromByte((byte)operand.Value.RawValue));
                }
                else if (operand.Kind == OperandKind.Variable)
                {
                    AddSeparator("[");
                    AddOperand(operand);
                    AddSeparator("]");
                }
                else // OperandKind.LargeConstant
                {
                    throw new InvalidOperationException("ByRef operand must be a small constant or a variable.");
                }
            }

            public void AddBranch(Instruction instruction)
            {
                var branch = instruction.Branch;

                AddSeparator("[");
                AddText(branch.Condition ? "TRUE" : "FALSE");
                AddSeparator("] ");

                if (branch.Kind == BranchKind.RFalse)
                {
                    AddText("rfalse");
                }
                else if (branch.Kind == BranchKind.RTrue)
                {
                    AddText("rtrue");
                }
                else // BranchKind.Address
                {
                    var targetAddress = instruction.Address + instruction.Length + branch.Offset - 2;
                    AddAddress(targetAddress);
                }
            }

            public void AddZText(string ztext)
            {
                // TODO: Add wrapping for ZText
                AddVisual(ztext, ztextTypeface, Brushes.Maroon);
            }
        }
    }
}
