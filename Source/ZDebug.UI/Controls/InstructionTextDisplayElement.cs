using System.Linq;
using System.Windows;
using System.Windows.Media;
using ZDebug.Core.Instructions;
using ZDebug.Core.Text;
using ZDebug.UI.Services;

namespace ZDebug.UI.Controls
{
    internal partial class InstructionTextDisplayElement : FrameworkElement
    {
        private bool needToRefreshVisuals = true;

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
                        element.needToRefreshVisuals = true;
                    }));

        private readonly VisualCollection visuals;

        public InstructionTextDisplayElement()
        {
            this.visuals = new VisualCollection(this);

            TextOptions.SetTextHintingMode(this, TextHintingMode.Fixed);
        }

        private void RefreshVisuals()
        {
            visuals.Clear();

            var instruction = Instruction;
            if (instruction == null)
            {
                return;
            }

            using (var builder = new VisualBuilder(
                visuals,
                height: this.ActualHeight,
                width: this.ActualWidth))
            {
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
                    var ztext = ZText.ZWordsAsString(instruction.ZText, ZTextFlags.All, DebuggerService.Story.Memory);
                    builder.AddZText(ztext.Replace("\n", "\\n").Replace(' ', '·'));
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
            }

            needToRefreshVisuals = false;
        }

        protected override Visual GetVisualChild(int index)
        {
            return visuals[index];
        }

        protected override int VisualChildrenCount
        {
            get { return visuals.Count; }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            //drawingContext.DrawRectangle(this.Background, null, new Rect(new Point(), this.RenderSize));
            if (needToRefreshVisuals)
            {
                RefreshVisuals();
            }
        }

        public Instruction Instruction
        {
            get { return (Instruction)GetValue(InstructionProperty); }
            set { SetValue(InstructionProperty, value); }
        }
    }
}
