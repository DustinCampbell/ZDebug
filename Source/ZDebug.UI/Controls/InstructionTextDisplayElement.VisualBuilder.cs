using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using ZDebug.Core.Instructions;

namespace ZDebug.UI.Controls
{
    internal partial class InstructionTextDisplayElement
    {
        private class VisualBuilder
        {
            private readonly VisualCollection visuals;

            private readonly double height;

            private readonly FontAndColorSetting defaultSetting;
            private readonly FontAndColorSetting addressSetting;
            private readonly FontAndColorSetting commentSetting;
            private readonly FontAndColorSetting constantSetting;
            private readonly FontAndColorSetting globalVariableSetting;
            private readonly FontAndColorSetting keywordSetting;
            private readonly FontAndColorSetting localVariableSetting;
            private readonly FontAndColorSetting separatorSetting;
            private readonly FontAndColorSetting stackVariableSetting;
            private readonly FontAndColorSetting ztextSetting;

            private double left;

            public VisualBuilder(
                VisualCollection visuals,
                double height,
                FontAndColorSetting defaultSetting,
                FontAndColorSetting addressSetting,
                FontAndColorSetting commentSetting,
                FontAndColorSetting constantSetting,
                FontAndColorSetting globalVariableSetting,
                FontAndColorSetting keywordSetting,
                FontAndColorSetting localVariableSetting,
                FontAndColorSetting separatorSetting,
                FontAndColorSetting stackVariableSetting,
                FontAndColorSetting ztextSetting)
            {
                this.visuals = visuals;
                this.height = height;
                this.defaultSetting = defaultSetting;
                this.addressSetting = addressSetting;
                this.commentSetting = commentSetting;
                this.constantSetting = constantSetting;
                this.globalVariableSetting = globalVariableSetting;
                this.keywordSetting = keywordSetting;
                this.localVariableSetting = localVariableSetting;
                this.separatorSetting = separatorSetting;
                this.stackVariableSetting = stackVariableSetting;
                this.ztextSetting = ztextSetting;
            }

            private FormattedText CreateFormattedText(string text, FontAndColorSetting setting)
            {
                return new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    setting.GetTypeface(), setting.FontSize, setting.Foreground ?? defaultSetting.Foreground, null, TextFormattingMode.Display);
            }

            private void AddVisual(string text, FontAndColorSetting setting)
            {
                setting = setting ?? defaultSetting;

                var visual = new DrawingVisual();
                var context = visual.RenderOpen();

                var heightText = CreateFormattedText("Yy", setting);
                var top = (height - heightText.Height) / 2;

                var formattedText = CreateFormattedText(text, setting);
                var width = formattedText.WidthIncludingTrailingWhitespace;

                var background = setting.Background ?? defaultSetting.Background;
                if (background != null)
                {
                    context.DrawRectangle(setting.Background ?? defaultSetting.Background, null, new Rect(left, 0.0, width, height));
                }

                context.DrawText(formattedText, new Point(left, top));

                context.Close();

                visuals.Add(visual);

                left = left + width;
            }

            public void AddAddress(int address)
            {
                AddVisual(address.ToString("x4"), addressSetting);
            }

            public void AddBranch(Instruction instruction)
            {
                var branch = instruction.Branch;

                AddSeparator("[");
                AddKeyword(branch.Condition ? "TRUE" : "FALSE");
                AddSeparator("] ");

                if (branch.Kind == BranchKind.RFalse)
                {
                    AddKeyword("rfalse");
                }
                else if (branch.Kind == BranchKind.RTrue)
                {
                    AddKeyword("rtrue");
                }
                else // BranchKind.Address
                {
                    var targetAddress = instruction.Address + instruction.Length + branch.Offset - 2;
                    AddAddress(targetAddress);
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

            public void AddConstant(ushort value)
            {
                AddVisual("#" + value.ToString("x4"), constantSetting);
            }

            public void AddConstant(byte value)
            {
                AddVisual("#" + value.ToString("x2"), constantSetting);
            }

            public void AddKeyword(string text)
            {
                AddVisual(text, keywordSetting);
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

            public void AddSeparator(string text)
            {
                AddVisual(text, separatorSetting);
            }

            public void AddVariable(Variable variable, bool @out = false)
            {
                if (variable.Kind == VariableKind.Stack)
                {
                    if (@out)
                    {
                        AddSeparator("-(");
                        AddVisual("SP", stackVariableSetting);
                        AddSeparator(")");
                    }
                    else
                    {
                        AddSeparator("(");
                        AddVisual("SP", stackVariableSetting);
                        AddSeparator(")+");
                    }
                }
                else if (variable.Kind == VariableKind.Local)
                {
                    AddVisual(variable.ToString(), localVariableSetting);
                }
                else // VariableKind.Global
                {
                    AddVisual(variable.ToString(), globalVariableSetting);
                }
            }

            public void AddZText(string ztext)
            {
                AddSeparator("[");
                // TODO: Add wrapping for ZText
                AddVisual(ztext, ztextSetting);
                AddSeparator("]");
            }
        }
    }
}
