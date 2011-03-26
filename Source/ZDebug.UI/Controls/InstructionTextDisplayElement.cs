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
        private readonly StoryService storyService;
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
            this.storyService = App.Current.GetService<StoryService>();
            this.builder = new InstructionTextBuilder();

            TextOptions.SetTextHintingMode(this, TextHintingMode.Fixed);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (update)
            {
                RefreshInstruction();
            }

            return builder.Measure(availableSize.Width);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            builder.Draw(drawingContext, this.DesiredSize.Width);
        }

        private void RefreshInstruction()
        {
            builder.Clear();

            var instruction = Instruction;
            if (instruction == null)
            {
                return;
            }

            if (instruction.OperandCount > 0)
            {
                if (instruction.Opcode.IsCall)
                {
                    var callOp = instruction.Operands[0];
                    if (callOp.Kind == OperandKind.Variable)
                    {
                        builder.AddVariable(Variable.FromByte((byte)callOp.Value));
                    }
                    else
                    {
                        var callAddress = callOp.Value;
                        if (storyService.IsStoryOpen)
                        {
                            builder.AddAddress(storyService.Story.UnpackRoutineAddress(callAddress));
                        }
                        else
                        {
                            builder.AddAddress(callAddress);
                        }
                    }

                    if (instruction.OperandCount > 1)
                    {
                        builder.AddSeparator(" (");
                        builder.AddOperands(instruction.Operands.Skip(1));
                        builder.AddSeparator(")");
                    }

                }
                else if (instruction.Opcode.IsJump)
                {
                    var jumpOffset = (short)instruction.Operands[0].Value;
                    var jumpAddress = instruction.Address + instruction.Length + jumpOffset - 2;
                    builder.AddAddress(jumpAddress);
                }
                else if (instruction.Opcode.IsFirstOpByRef)
                {
                    builder.AddByRefOperand(instruction.Operands[0]);

                    if (instruction.OperandCount > 1)
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

            if (instruction.HasZText && storyService.IsStoryOpen)
            {
                var ztextBuilder = new StringBuilder(storyService.Story.ZText.ZWordsAsString(instruction.ZText, ZTextFlags.All));
                var ztext = ztextBuilder.ToString();

                if (ztext.Length > 0)
                {
                    int lastIndex = ztext.Length;
                    do
                    {
                        lastIndex = ztext.LastIndexOfAny(new char[] { '\n', '\r', '\v', '\t', ' ' }, lastIndex - 1);
                        if (lastIndex < 0)
                        {
                            break;
                        }

                        // Replace the found character with a zero-width space or line separator to allow line breaking
                        // and insert replacement text.
                        char foundChar = ztext[lastIndex];

                        if (foundChar == ' ')
                        {
                            ztextBuilder[lastIndex] = '\u200b';
                        }
                        else if (lastIndex != ztext.Length - 1)
                        {
                            ztextBuilder[lastIndex] = '\u2028';
                        }

                        switch (foundChar)
                        {
                            case '\n':
                                ztextBuilder.Insert(lastIndex, "\\n");
                                break;
                            case '\r':
                                ztextBuilder.Insert(lastIndex, "\\r");
                                break;
                            case '\v':
                                ztextBuilder.Insert(lastIndex, "\\v");
                                break;
                            case '\t':
                                ztextBuilder.Insert(lastIndex, "\\t");
                                break;
                            case ' ':
                                ztextBuilder.Insert(lastIndex, "\u00b7");
                                break;
                        }
                    }
                    while (lastIndex > 0);
                }

                builder.AddZText(ztextBuilder.ToString());
            }

            if (instruction.HasStoreVariable)
            {
                builder.AddSeparator(" -> ");
                builder.AddVariable(instruction.StoreVariable, @out: true);
            }

            if (instruction.HasBranch)
            {
                if (instruction.OperandCount > 0)
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
