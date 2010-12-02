using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using ZDebug.Core.Instructions;
using ZDebug.Core.Text;
using ZDebug.UI.Services;

namespace ZDebug.UI.Controls
{
    internal partial class InstructionTextDisplayElement : FrameworkElement
    {
        private readonly InstructionTextBuilder builder;
        private bool update = true;

        public static readonly DependencyProperty InstructionProperty =
            DependencyProperty.Register(
                "Instruction",
                typeof(Instruction),
                typeof(InstructionTextDisplayElement),
                new FrameworkPropertyMetadata(
                    defaultValue: null,
                    flags: FrameworkPropertyMetadataOptions.AffectsRender,
                    propertyChangedCallback: (s, e) =>
                    {
                        var element = (InstructionTextDisplayElement)s;
                        element.InvalidateMeasure();
                        element.update = true;
                    }));

        public InstructionTextDisplayElement()
        {
            this.builder = new InstructionTextBuilder();

            TextOptions.SetTextHintingMode(this, TextHintingMode.Fixed);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (update)
            {
                RefreshInstruction();
            }

            return builder.Measure(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return base.ArrangeOverride(finalSize);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            builder.Draw(drawingContext, this.ActualWidth);
        }

        private void RefreshInstruction()
        {
            builder.Clear();

            var instruction = Instruction;
            if (instruction == null)
            {
                return;
            }

            if (instruction.Operands.Count > 0)
            {
                if (instruction.Opcode.IsCall)
                {
                    var callAddress = instruction.Operands[0].Value.RawValue;
                    if (DebuggerService.HasStory)
                    {
                        builder.AddAddress(DebuggerService.Story.UnpackRoutineAddress(callAddress));
                    }
                    else
                    {
                        builder.AddAddress(callAddress);
                    }

                    if (instruction.Operands.Count > 1)
                    {
                        builder.AddSeparator(" (");
                        builder.AddOperands(instruction.Operands.Skip(1));
                        builder.AddSeparator(")");
                    }
                }
                else if (instruction.Opcode.IsJump)
                {
                    var jumpOffset = (short)instruction.Operands[0].Value.RawValue;
                    var jumpAddress = instruction.Address + instruction.Length + jumpOffset - 2;
                    builder.AddAddress(jumpAddress);
                }
                else if (instruction.Opcode.IsFirstOpByRef)
                {
                    builder.AddByRefOperand(instruction.Operands[0]);

                    if (instruction.Operands.Count > 1)
                    {
                        builder.AddSeparator(", ");
                        builder.AddOperands(instruction.Operands.Skip(1));
                    }
                }
                else
                {
                    builder.AddOperands(instruction.Operands);
                }
            }

            if (instruction.HasZText && DebuggerService.HasStory)
            {
                var ztextBuilder = new StringBuilder(ZText.ZWordsAsString(instruction.ZText, ZTextFlags.All, DebuggerService.Story.Memory));
                ztextBuilder.Replace("\n", "\\n");
                ztextBuilder.Replace("\v", "\\v");
                ztextBuilder.Replace("\r", "\\r");
                ztextBuilder.Replace("\t", "\\t");
                ztextBuilder.Replace(' ', '·');
                builder.AddZText(ztextBuilder.ToString());
            }

            if (instruction.HasStoreVariable)
            {
                builder.AddSeparator(" -> ");
                builder.AddVariable(instruction.StoreVariable, @out: true);
            }

            if (instruction.HasBranch)
            {
                if (instruction.Operands.Count > 0)
                {
                    builder.AddSeparator(" ");
                }

                builder.AddBranch(instruction);
            }

            update = false;
        }

        public Instruction Instruction
        {
            get { return (Instruction)GetValue(InstructionProperty); }
            set { SetValue(InstructionProperty, value); }
        }
    }
}
