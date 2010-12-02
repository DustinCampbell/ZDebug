using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using ZDebug.Core.Instructions;
using ZDebug.UI.Services;

namespace ZDebug.UI.Controls
{
    internal partial class InstructionTextDisplayElement
    {
        private class InstructionTextBuilder
        {
            private readonly TextParagraphProperties defaultParagraphProps;

            private readonly InstructionTextSource textSource;
            private readonly TextFormatter formatter;
            private readonly TextRunCache cache;

            public InstructionTextBuilder()
            {
                this.defaultParagraphProps = new SimpleTextParagraphProperties(FontsAndColorsService.DefaultSetting);

                textSource = new InstructionTextSource();
                formatter = TextFormatter.Create(TextFormattingMode.Display);
                cache = new TextRunCache();
            }

            public void Clear()
            {
                textSource.Clear();
            }

            public Size Measure(Size availableSize)
            {
                var height = 0.0;
                var width = 0.0;

                int textSourcePosition = 0;
                while (textSourcePosition < textSource.Length)
                {
                    using (var line = formatter.FormatLine(textSource, textSourcePosition, availableSize.Width, defaultParagraphProps, previousLineBreak: null, textRunCache: cache))
                    {
                        height += line.Height;
                        width = Math.Max(width, line.Width);

                        textSourcePosition += line.Length;
                    }
                }

                return new Size(double.IsPositiveInfinity(availableSize.Width) ? width : availableSize.Width, height);
            }

            public void Draw(DrawingContext context, double width)
            {
                var top = 0.0;
                int textSourcePosition = 0;
                while (textSourcePosition < textSource.Length)
                {
                    using (var line = formatter.FormatLine(textSource, textSourcePosition, width, defaultParagraphProps, previousLineBreak: null, textRunCache: cache))
                    {
                        line.Draw(context, new Point(0.0, top), InvertAxes.None);
                        textSourcePosition += line.Length;
                        top += line.Height;
                    }
                }
            }

            private void AddText(string text, FontAndColorSetting setting)
            {
                if (setting == null)
                {
                    throw new ArgumentNullException("setting");
                }

                textSource.Add(text, setting);
                cache.Invalidate();
            }

            public void AddAddress(int address)
            {
                AddText(address.ToString("x4"), FontsAndColorsService.AddressSetting);
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
                AddText("#" + value.ToString("x4"), FontsAndColorsService.ConstantSetting);
            }

            public void AddConstant(byte value)
            {
                AddText("#" + value.ToString("x2"), FontsAndColorsService.ConstantSetting);
            }

            public void AddKeyword(string text)
            {
                AddText(text, FontsAndColorsService.KeywordSetting);
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
                AddText(text, FontsAndColorsService.SeparatorSetting);
            }

            public void AddVariable(Variable variable, bool @out = false)
            {
                if (variable.Kind == VariableKind.Stack)
                {
                    if (@out)
                    {
                        AddSeparator("-(");
                        AddText("SP", FontsAndColorsService.StackVariableSetting);
                        AddSeparator(")");
                    }
                    else
                    {
                        AddSeparator("(");
                        AddText("SP", FontsAndColorsService.StackVariableSetting);
                        AddSeparator(")+");
                    }
                }
                else if (variable.Kind == VariableKind.Local)
                {
                    AddText(variable.ToString(), FontsAndColorsService.LocalVariableSetting);
                }
                else // VariableKind.Global
                {
                    AddText(variable.ToString(), FontsAndColorsService.GlobalVariableSetting);
                }
            }

            public void AddZText(string ztext)
            {
                AddText(ztext, FontsAndColorsService.ZTextSetting);
            }
        }
    }
}
