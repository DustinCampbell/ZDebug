using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using ZDebug.Core.Instructions;
using ZDebug.Core.Text;
using ZDebug.UI.Services;
using ZDebug.UI.Utilities;

namespace ZDebug.UI.Controls
{
    internal partial class InstructionTextDisplayElement : FrameworkElement
    {
        private FontAndColorSetting defaultSetting = null;

        private FontAndColorSetting GetDefaultSetting()
        {
            if (defaultSetting == null)
            {
                defaultSetting = new FontAndColorSetting()
                {
                    Background = this.Background,
                    FontFamily = this.FontFamily,
                    FontSize = this.FontSize,
                    FontStretch = this.FontStretch,
                    FontStyle = this.FontStyle,
                    FontWeight = this.FontWeight,
                    Foreground = this.Foreground
                };
            }

            return defaultSetting;
        }

        private static void UpdateIfNeeded(FontAndColorSetting setting, DependencyProperty propertyToUpdate, object value)
        {
            if (setting != null && setting.IsDefaultValue(propertyToUpdate))
            {
                setting.SetValue(propertyToUpdate, value);
            }
        }

        private static readonly PropertyChangedCallback resetVisuals = (s, e) =>
        {
            var element = (InstructionTextDisplayElement)s;
            element.defaultSetting = null;
            element.ResetVisuals();
        };

        public static readonly DependencyProperty BackgroundProperty =
            TextElement.BackgroundProperty.AddOwner(
                typeof(InstructionTextDisplayElement),
                new FrameworkPropertyMetadata(
                    Panel.BackgroundProperty.DefaultMetadata.DefaultValue,
                    FrameworkPropertyMetadataOptions.Inherits, resetVisuals));

        public static readonly DependencyProperty FontFamilyProperty =
            TextElement.FontFamilyProperty.AddOwner(
                typeof(InstructionTextDisplayElement),
                new FrameworkPropertyMetadata(
                    TextElement.FontFamilyProperty.DefaultMetadata.DefaultValue,
                    FrameworkPropertyMetadataOptions.Inherits, resetVisuals));

        public static readonly DependencyProperty FontSizeProperty =
            TextElement.FontSizeProperty.AddOwner(
                typeof(InstructionTextDisplayElement),
                new FrameworkPropertyMetadata(
                    TextElement.FontSizeProperty.DefaultMetadata.DefaultValue,
                    FrameworkPropertyMetadataOptions.Inherits, resetVisuals));

        public static readonly DependencyProperty FontStretchProperty =
            TextElement.FontStretchProperty.AddOwner(
                typeof(InstructionTextDisplayElement),
                new FrameworkPropertyMetadata(
                    TextElement.FontStretchProperty.DefaultMetadata.DefaultValue,
                    FrameworkPropertyMetadataOptions.Inherits, resetVisuals));

        public static readonly DependencyProperty FontStyleProperty =
            TextElement.FontStyleProperty.AddOwner(
                typeof(InstructionTextDisplayElement),
                new FrameworkPropertyMetadata(
                    TextElement.FontStyleProperty.DefaultMetadata.DefaultValue,
                    FrameworkPropertyMetadataOptions.Inherits, resetVisuals));

        public static readonly DependencyProperty FontWeightProperty =
            TextElement.FontWeightProperty.AddOwner(
                typeof(InstructionTextDisplayElement),
                new FrameworkPropertyMetadata(FontWeights.Normal, resetVisuals));

        public static readonly DependencyProperty ForegroundProperty =
            TextElement.ForegroundProperty.AddOwner(
                typeof(InstructionTextDisplayElement),
                new FrameworkPropertyMetadata(
                    TextElement.ForegroundProperty.DefaultMetadata.DefaultValue,
                    FrameworkPropertyMetadataOptions.Inherits, resetVisuals));

        public static readonly DependencyProperty InstructionProperty =
            DependencyProperty.Register(
                "Instruction",
                typeof(Instruction),
                typeof(InstructionTextDisplayElement),
                new FrameworkPropertyMetadata(
                    null, FrameworkPropertyMetadataOptions.AffectsRender, resetVisuals));

        public static readonly DependencyProperty AddressSettingProperty =
            DependencyProperty.Register(
                "AddressSetting",
                typeof(FontAndColorSetting),
                typeof(InstructionTextDisplayElement),
                new FrameworkPropertyMetadata(
                    null, FrameworkPropertyMetadataOptions.AffectsRender, resetVisuals));

        public static readonly DependencyProperty CommentSettingProperty =
            DependencyProperty.Register(
                "CommentSetting",
                typeof(FontAndColorSetting),
                typeof(InstructionTextDisplayElement),
                new FrameworkPropertyMetadata(
                    null, FrameworkPropertyMetadataOptions.AffectsRender, resetVisuals));

        public static readonly DependencyProperty ConstantSettingProperty =
            DependencyProperty.Register(
                "ConstantSetting",
                typeof(FontAndColorSetting),
                typeof(InstructionTextDisplayElement),
                new FrameworkPropertyMetadata(
                    null, FrameworkPropertyMetadataOptions.AffectsRender, resetVisuals));

        public static readonly DependencyProperty GlobalVariableSettingProperty =
            DependencyProperty.Register(
                "GlobalVariableSetting",
                typeof(FontAndColorSetting),
                typeof(InstructionTextDisplayElement),
                new FrameworkPropertyMetadata(
                    null, FrameworkPropertyMetadataOptions.AffectsRender, resetVisuals));

        public static readonly DependencyProperty KeywordSettingProperty =
            DependencyProperty.Register(
                "KeywordSetting",
                typeof(FontAndColorSetting),
                typeof(InstructionTextDisplayElement),
                new FrameworkPropertyMetadata(
                    null, FrameworkPropertyMetadataOptions.AffectsRender, resetVisuals));

        public static readonly DependencyProperty LocalVariableSettingProperty =
            DependencyProperty.Register(
                "LocalVariableSetting",
                typeof(FontAndColorSetting),
                typeof(InstructionTextDisplayElement),
                new FrameworkPropertyMetadata(
                    null, FrameworkPropertyMetadataOptions.AffectsRender, resetVisuals));

        public static readonly DependencyProperty SeparatorSettingProperty =
            DependencyProperty.Register(
                "SeparatorSetting",
                typeof(FontAndColorSetting),
                typeof(InstructionTextDisplayElement),
                new FrameworkPropertyMetadata(
                    null, FrameworkPropertyMetadataOptions.AffectsRender, resetVisuals));

        public static readonly DependencyProperty StackVariableSettingProperty =
            DependencyProperty.Register(
                "StackVariableSetting",
                typeof(FontAndColorSetting),
                typeof(InstructionTextDisplayElement),
                new FrameworkPropertyMetadata(
                    null, FrameworkPropertyMetadataOptions.AffectsRender, resetVisuals));

        public static readonly DependencyProperty ZTextSettingProperty =
            DependencyProperty.Register(
                "ZTextSetting",
                typeof(FontAndColorSetting),
                typeof(InstructionTextDisplayElement),
                new FrameworkPropertyMetadata(
                    null, FrameworkPropertyMetadataOptions.AffectsRender, resetVisuals));

        private readonly VisualCollection visuals;

        public InstructionTextDisplayElement()
        {
            this.visuals = new VisualCollection(this);

            TextOptions.SetTextHintingMode(this, TextHintingMode.Fixed);
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
                height: this.ActualHeight,
                defaultSetting: this.GetDefaultSetting(),
                addressSetting: this.AddressSetting,
                commentSetting: this.CommentSetting,
                constantSetting: this.ConstantSetting,
                globalVariableSetting: this.GlobalVariableSetting,
                keywordSetting: this.KeywordSetting,
                localVariableSetting: this.LocalVariableSetting,
                separatorSetting: this.SeparatorSetting,
                stackVariableSetting: this.StackVariableSetting,
                ztextSetting: this.ZTextSetting);

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
            drawingContext.DrawRectangle(this.Background, null, new Rect(new Point(), this.RenderSize));
            ResetVisuals();
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

        public FontAndColorSetting AddressSetting
        {
            get { return (FontAndColorSetting)GetValue(AddressSettingProperty); }
            set { SetValue(AddressSettingProperty, value); }
        }

        public FontAndColorSetting CommentSetting
        {
            get { return (FontAndColorSetting)GetValue(CommentSettingProperty); }
            set { SetValue(CommentSettingProperty, value); }
        }

        public FontAndColorSetting ConstantSetting
        {
            get { return (FontAndColorSetting)GetValue(ConstantSettingProperty); }
            set { SetValue(ConstantSettingProperty, value); }
        }

        public FontAndColorSetting GlobalVariableSetting
        {
            get { return (FontAndColorSetting)GetValue(GlobalVariableSettingProperty); }
            set { SetValue(GlobalVariableSettingProperty, value); }
        }

        public FontAndColorSetting KeywordSetting
        {
            get { return (FontAndColorSetting)GetValue(KeywordSettingProperty); }
            set { SetValue(KeywordSettingProperty, value); }
        }

        public FontAndColorSetting LocalVariableSetting
        {
            get { return (FontAndColorSetting)GetValue(LocalVariableSettingProperty); }
            set { SetValue(LocalVariableSettingProperty, value); }
        }

        public FontAndColorSetting SeparatorSetting
        {
            get { return (FontAndColorSetting)GetValue(SeparatorSettingProperty); }
            set { SetValue(SeparatorSettingProperty, value); }
        }

        public FontAndColorSetting StackVariableSetting
        {
            get { return (FontAndColorSetting)GetValue(StackVariableSettingProperty); }
            set { SetValue(StackVariableSettingProperty, value); }
        }

        public FontAndColorSetting ZTextSetting
        {
            get { return (FontAndColorSetting)GetValue(ZTextSettingProperty); }
            set { SetValue(ZTextSettingProperty, value); }
        }
    }
}
