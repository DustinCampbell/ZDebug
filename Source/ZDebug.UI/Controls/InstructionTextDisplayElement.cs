using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using ZDebug.Core.Instructions;
using ZDebug.Core.Text;
using ZDebug.UI.Services;

namespace ZDebug.UI.Controls
{
    internal partial class InstructionTextDisplayElement : FrameworkElement
    {
        private static readonly PropertyChangedCallback resetVisuals = (s, e) =>
        {
            ((InstructionTextDisplayElement)s).ResetVisuals();
        };

        public static readonly DependencyProperty BackgroundProperty =
            TextElement.BackgroundProperty.AddOwner(
                typeof(InstructionTextDisplayElement),
                new FrameworkPropertyMetadata(resetVisuals));

        public static readonly DependencyProperty FontFamilyProperty =
            TextElement.FontFamilyProperty.AddOwner(
                typeof(InstructionTextDisplayElement),
                new FrameworkPropertyMetadata(resetVisuals));

        public static readonly DependencyProperty FontSizeProperty =
            TextElement.FontSizeProperty.AddOwner(
                typeof(InstructionTextDisplayElement),
                new FrameworkPropertyMetadata(resetVisuals));

        public static readonly DependencyProperty FontStretchProperty =
            TextElement.FontStretchProperty.AddOwner(
                typeof(InstructionTextDisplayElement),
                new FrameworkPropertyMetadata(resetVisuals));

        public static readonly DependencyProperty FontStyleProperty =
            TextElement.FontStyleProperty.AddOwner(
                typeof(InstructionTextDisplayElement),
                new FrameworkPropertyMetadata(resetVisuals));

        public static readonly DependencyProperty FontWeightProperty =
            TextElement.FontWeightProperty.AddOwner(
                typeof(InstructionTextDisplayElement),
                new FrameworkPropertyMetadata(resetVisuals));

        public static readonly DependencyProperty ForegroundProperty =
            TextElement.ForegroundProperty.AddOwner(
                typeof(InstructionTextDisplayElement),
                new FrameworkPropertyMetadata(resetVisuals));

        public static readonly DependencyProperty InstructionProperty =
            DependencyProperty.Register(
                "Instruction",
                typeof(Instruction),
                typeof(InstructionTextDisplayElement),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    resetVisuals));

        private readonly VisualCollection visuals;

        public InstructionTextDisplayElement()
        {
            this.visuals = new VisualCollection(this);
        }

        private void ResetVisuals()
        {
            visuals.Clear();

            var instruction = Instruction;
            if (instruction == null)
            {
                return;
            }

            var builder = new VisualBuilder(
                visuals,
                typeface: new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch),
                fontSize: this.FontSize,
                foreground: this.Foreground,
                background: this.Background,
                height: this.ActualHeight);

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
                builder.AddZText("\"" + ztext.Replace("\n", "\\n") + "\"");
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
                    builder.AddText(" ");
                }

                builder.AddBranch(instruction);
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return visuals[index];
        }

        protected override int VisualChildrenCount
        {
            get { return visuals.Count; }
        }

        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public FontStretch FontStretch
        {
            get { return (FontStretch)GetValue(FontStretchProperty); }
            set { SetValue(FontStretchProperty, value); }
        }

        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public Instruction Instruction
        {
            get { return (Instruction)GetValue(InstructionProperty); }
            set { SetValue(InstructionProperty, value); }
        }
    }
}
