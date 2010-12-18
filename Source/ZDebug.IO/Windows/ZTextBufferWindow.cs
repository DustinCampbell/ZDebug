using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ZDebug.IO.Services;

namespace ZDebug.IO.Windows
{
    internal sealed class ZTextBufferWindow : ZWindow
    {
        private readonly FlowDocumentScrollViewer scrollViewer;
        private readonly FlowDocument document;
        private readonly Paragraph paragraph;
        private readonly Size fontCharSize;

        private readonly Grid inputGrid;
        private readonly TextBox inputTextBox;
        private readonly RowDefinition inputUpperMarginRow;
        private readonly ColumnDefinition inputLeftMarginColumn;

        private bool bold;
        private bool italic;
        private bool fixedPitch;
        private bool reverse;

        internal ZTextBufferWindow(ZWindowManager manager)
            : base(manager)
        {
            this.document = new FlowDocument();
            this.document.FontFamily = FontsAndColorsService.NormalFontFamily;
            this.document.FontSize = FontsAndColorsService.FontSize;
            this.document.PagePadding = new Thickness(8);
            this.paragraph = new Paragraph();
            this.document.Blocks.Add(paragraph);

            this.scrollViewer = new FlowDocumentScrollViewer();
            this.scrollViewer.FocusVisualStyle = null;
            this.scrollViewer.Document = this.document;

            this.Children.Add(scrollViewer);

            this.inputGrid = new Grid();
            this.inputGrid.Visibility = Visibility.Collapsed;

            this.inputUpperMarginRow = new RowDefinition();
            var inputRow = new RowDefinition();
            this.inputGrid.RowDefinitions.Add(inputUpperMarginRow);
            this.inputGrid.RowDefinitions.Add(inputRow);

            this.inputLeftMarginColumn = new ColumnDefinition();
            var inputColumn = new ColumnDefinition();
            this.inputGrid.ColumnDefinitions.Add(inputLeftMarginColumn);
            this.inputGrid.ColumnDefinitions.Add(inputColumn);

            this.inputTextBox = new TextBox();
            this.inputTextBox.FontFamily = FontsAndColorsService.NormalFontFamily;
            this.inputTextBox.FontSize = FontsAndColorsService.FontSize;
            this.inputTextBox.Padding = new Thickness(0);
            this.inputTextBox.BorderBrush = Brushes.Transparent;
            this.inputTextBox.BorderThickness = new Thickness(0);
            Grid.SetRow(inputTextBox, 1);
            Grid.SetColumn(inputTextBox, 1);

            inputGrid.Children.Add(inputTextBox);

            this.Children.Add(inputGrid);

            var zero = new FormattedText(
                textToFormat: "0",
                culture: CultureInfo.InstalledUICulture,
                flowDirection: FlowDirection.LeftToRight,
                typeface: new Typeface(FontsAndColorsService.NormalFontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                emSize: FontsAndColorsService.FontSize,
                foreground: FontsAndColorsService.DefaultForeground);

            fontCharSize = new Size(zero.Width, zero.Height);
        }

        private Run GetFormattedRun(string text)
        {
            var run = new Run(text);

            if (bold)
            {
                run.FontWeight = FontWeights.Bold;
            }

            if (italic)
            {
                run.FontStyle = FontStyles.Italic;
            }

            if (fixedPitch)
            {
                run.FontFamily = FontsAndColorsService.FixedFontFamily;
            }
            else
            {
                run.FontFamily = FontsAndColorsService.FontFamily;
            }

            if (reverse)
            {
                run.Background = FontsAndColorsService.Foreground;
                run.Foreground = FontsAndColorsService.Background;
            }
            else
            {
                run.Background = FontsAndColorsService.Background;
                run.Foreground = FontsAndColorsService.Foreground;
            }

            return run;
        }

        private ScrollViewer FindScrollViewer()
        {
            DependencyObject obj = scrollViewer;
            do
            {
                if (VisualTreeHelper.GetChildrenCount(obj) > 0)
                {
                    obj = VisualTreeHelper.GetChild(obj as Visual, 0);
                }
                else
                {
                    return null;
                }
            }
            while (!(obj is ScrollViewer));

            return obj as ScrollViewer;
        }

        public override void Clear()
        {
            this.paragraph.Inlines.Clear();
        }

        public override void PutString(string text)
        {
            this.paragraph.Inlines.Add(GetFormattedRun(text));
            FindScrollViewer().ScrollToEnd();
        }

        public override void PutChar(char ch)
        {
            this.paragraph.Inlines.Add(GetFormattedRun(ch.ToString()));
            FindScrollViewer().ScrollToEnd();
        }

        public override void ReadChar(Action<char> callback)
        {
            TextCompositionEventHandler handler = null;
            handler = (s, e) =>
            {
                this.scrollViewer.TextInput -= handler;
                callback(e.Text[0]);
            };

            this.scrollViewer.TextInput += handler;
            Keyboard.Focus(this.scrollViewer);
        }

        public override void ReadCommand(int maxChars, Action<string> callback)
        {
            KeyEventHandler handler = null;
            handler = (s, e) =>
            {
                if (e.Key != Key.Return)
                {
                    return;
                }

                this.inputTextBox.KeyUp -= handler;

                var text = this.inputTextBox.Text;
                PutString(text + '\n');

                this.inputTextBox.Clear();
                this.inputGrid.Visibility = Visibility.Collapsed;

                callback(text);
            };

            var pointer = this.document.ContentEnd;
            var rect = pointer.GetCharacterRect(LogicalDirection.Backward);

            this.inputLeftMarginColumn.Width = new GridLength(rect.Right, GridUnitType.Pixel);
            this.inputUpperMarginRow.Height = new GridLength(rect.Top, GridUnitType.Pixel);
            this.inputGrid.Visibility = Visibility.Visible;

            this.inputTextBox.MaxLength = maxChars;
            this.inputTextBox.KeyUp += handler;
            this.inputTextBox.Focus();
        }

        public override bool SetBold(bool value)
        {
            var oldValue = bold;
            bold = value;
            return oldValue;
        }

        public override bool SetItalic(bool value)
        {
            var oldValue = italic;
            italic = value;
            return oldValue;
        }

        public override bool SetFixedPitch(bool value)
        {
            var oldValue = fixedPitch;
            fixedPitch = value;
            return oldValue;
        }

        public override bool SetReverse(bool value)
        {
            var oldValue = reverse;
            reverse = value;
            return oldValue;
        }

        public override int RowHeight
        {
            get { return (int)fontCharSize.Height; }
        }

        public override int ColumnWidth
        {
            get { return (int)fontCharSize.Width; }
        }

        public override ZWindowType WindowType
        {
            get { return ZWindowType.TextBuffer; }
        }
    }
}
