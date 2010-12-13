using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ZDebug.Core.Execution;
using ZDebug.IO.Services;
using ZDebug.IO.Windows;
using ZDebug.UI.Services;
using ZDebug.UI.Utilities;

namespace ZDebug.UI.ViewModel
{
    internal sealed class OutputViewModel : ViewModelWithViewBase<UserControl>, IScreen
    {
        private ZWindowManager windowManager;
        private Grid windowContainer;

        private ZWindow mainWindow;
        private ZWindow upperWindow;

        public OutputViewModel()
            : base("OutputView")
        {
        }

        protected internal override void Initialize()
        {
            DebuggerService.StoryOpened += DebuggerService_StoryOpened;
            DebuggerService.StoryClosed += DebuggerService_StoryClosed;

            windowManager = new ZWindowManager();
            windowContainer = this.View.FindName<Grid>("windowContainer");
        }

        private void DebuggerService_StoryOpened(object sender, StoryEventArgs e)
        {
            mainWindow = windowManager.Open(ZWindowType.TextBuffer);
            windowContainer.Children.Add(mainWindow);

            windowManager.Activate(mainWindow);

            e.Story.Processor.RegisterScreen(this);
        }

        private void DebuggerService_StoryClosed(object sender, StoryEventArgs e)
        {
            windowContainer.Children.Clear();
            windowManager.Root.Close();
        }

        private FormattedText GetFixedFontMeasureText()
        {
            return new FormattedText(
                textToFormat: "0",
                culture: CultureInfo.CurrentUICulture,
                flowDirection: FlowDirection.LeftToRight,
                typeface: new Typeface(FontsAndColorsService.FixedFontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                emSize: FontsAndColorsService.FontSize,
                foreground: Brushes.Black);
        }

        private bool ForceFixedWidthFont()
        {
            // TODO: Move into appropriate API
            return (DebuggerService.Story.Memory.ReadWord(0x10) & 0x02) == 0x02;
        }

        public void Print(string text)
        {
            if (ForceFixedWidthFont())
            {
                bool oldValue = windowManager.ActiveWindow.SetFixedPitch(true);
                windowManager.ActiveWindow.PutString(text);
                windowManager.ActiveWindow.SetFixedPitch(oldValue);
            }
            else
            {
                windowManager.ActiveWindow.PutString(text);
            }
        }

        public void Print(char ch)
        {
            if (ForceFixedWidthFont())
            {
                bool oldValue = windowManager.ActiveWindow.SetFixedPitch(true);
                windowManager.ActiveWindow.PutChar(ch);
                windowManager.ActiveWindow.SetFixedPitch(oldValue);
            }
            else
            {
                windowManager.ActiveWindow.PutChar(ch);
            }
        }

        public void Clear(int window)
        {
            if (window == 0)
            {
                mainWindow.Clear();
            }
        }

        public void ClearAll(bool unsplit)
        {
            mainWindow.Clear();
        }

        public void Split(int height)
        {
            upperWindow = windowManager.Open(ZWindowType.TextGrid, mainWindow, ZWindowPosition.Above, ZWindowSizeType.Fixed, height);
        }

        public void Unsplit()
        {
            if (upperWindow != null)
            {
                upperWindow.Close();
                upperWindow = null;
            }
        }

        public void SetWindow(int window)
        {
            if (window == 0)
            {
                mainWindow.Activate();
            }
            else if (window == 1)
            {
                upperWindow.Activate();
            }
        }

        public void SetCursor(int line, int column)
        {
            windowManager.ActiveWindow.SetCursor(column, line);
        }

        public void SetTextStyle(ZTextStyle style)
        {
            windowManager.ActiveWindow.SetBold(style.HasFlag(ZTextStyle.Bold));
            windowManager.ActiveWindow.SetItalic(style.HasFlag(ZTextStyle.Italic));
            windowManager.ActiveWindow.SetFixedPitch(style.HasFlag(ZTextStyle.FixedPitch));
            windowManager.ActiveWindow.SetReverse(style.HasFlag(ZTextStyle.Reverse));
        }

        public byte ScreenHeightInLines
        {
            get { return (byte)(windowContainer.ActualHeight / GetFixedFontMeasureText().Height); }
        }

        public byte ScreenWidthInColumns
        {
            get { return (byte)(windowContainer.ActualWidth / GetFixedFontMeasureText().Width); }
        }

        public ushort ScreenHeightInUnits
        {
            get { return (ushort)windowContainer.ActualHeight; }
        }

        public ushort ScreenWidthInUnits
        {
            get { return (ushort)windowContainer.ActualWidth; }
        }

        public byte FontHeightInUnits
        {
            get { return (byte)GetFixedFontMeasureText().Height; }
        }

        public byte FontWidthInUnits
        {
            get { return (byte)GetFixedFontMeasureText().Width; }
        }

        public event EventHandler DimensionsChanged;
    }
}
