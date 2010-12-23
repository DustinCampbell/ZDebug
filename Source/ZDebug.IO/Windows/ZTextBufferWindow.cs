using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ZDebug.IO.Services;
using System.Windows.Threading;

namespace ZDebug.IO.Windows
{
    internal sealed class ZTextBufferWindow : ZWindow
    {
        private readonly FlowDocumentScrollViewer scrollViewer;
        private readonly FlowDocument document;
        private readonly Paragraph paragraph;
        private readonly Size fontCharSize;

        private bool bold;
        private bool italic;
        private bool fixedPitch;
        private bool reverse;

        internal ZTextBufferWindow(ZWindowManager manager)
            : base(manager)
        {
            document = new FlowDocument
            {
                FontFamily = FontsAndColorsService.NormalFontFamily,
                FontSize = FontsAndColorsService.FontSize,
                PagePadding = new Thickness(8)
            };

            paragraph = new Paragraph();
            document.Blocks.Add(paragraph);

            scrollViewer = new FlowDocumentScrollViewer
            {
                FocusVisualStyle = null,
                Document = document
            };

            Children.Add(scrollViewer);

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
            paragraph.Inlines.Clear();
        }

        public override void PutString(string text)
        {
            paragraph.Inlines.Add(GetFormattedRun(text));
            FindScrollViewer().ScrollToEnd();
        }

        public override void PutChar(char ch)
        {
            paragraph.Inlines.Add(GetFormattedRun(ch.ToString()));
            FindScrollViewer().ScrollToEnd();
        }

        public override void ReadChar(Action<char> callback)
        {
            TextCompositionEventHandler handler = null;
            handler = (s, e) =>
            {
                scrollViewer.TextInput -= handler;
                callback(e.Text[0]);
            };

            scrollViewer.TextInput += handler;
            Keyboard.Focus(scrollViewer);
        }

        public override void ReadCommand(int maxChars, Action<string> callback)
        {
            var inputTextBox = new TextBox()
            {
                FontFamily = FontsAndColorsService.NormalFontFamily,
                FontSize = FontsAndColorsService.FontSize,
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(0)
            };

            var inlineUIContainer = new InlineUIContainer(inputTextBox, document.ContentEnd)
            {
                BaselineAlignment = BaselineAlignment.TextBottom
            };

            paragraph.Inlines.Add(inlineUIContainer);

            KeyEventHandler handler = null;
            handler = (s, e) =>
            {
                if (e.Key != Key.Return)
                {
                    return;
                }

                inputTextBox.KeyUp -= handler;

                var text = inputTextBox.Text;
                paragraph.Inlines.Remove(inlineUIContainer);

                PutString(text + '\n');

                callback(text);
            };

            inputTextBox.MaxLength = maxChars;
            inputTextBox.KeyUp += handler;
            if (!inputTextBox.Focus())
            {
                inputTextBox.Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(delegate { inputTextBox.Focus(); }));
            }
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
