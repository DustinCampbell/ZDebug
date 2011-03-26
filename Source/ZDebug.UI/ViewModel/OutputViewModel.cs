using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ZDebug.Core.Execution;
using ZDebug.Core.Extensions;
using ZDebug.IO.Services;
using ZDebug.IO.Windows;
using ZDebug.UI.Extensions;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    [Export]
    internal sealed class OutputViewModel : ViewModelWithViewBase<UserControl>, IScreen, ISoundEngine
    {
        private readonly StoryService storyService;
        private readonly GameScriptService gameScriptService;
        private readonly DebuggerService debuggerService;

        private ZWindowManager windowManager;
        private Grid windowContainer;

        private ZWindow mainWindow;
        private ZWindow upperWindow;

        private ZFont font;

        private int currStatusHeight;
        private int machStatusHeight;

        [ImportingConstructor]
        private OutputViewModel(StoryService storyService,
            DebuggerService debuggerService,
            GameScriptService gameScriptService)
            : base("OutputView")
        {
            this.storyService = storyService;
            this.storyService.StoryClosing += StoryService_StoryClosing;

            this.debuggerService = debuggerService;
            this.debuggerService.MachineCreated += DebuggerService_MachineCreated;

            this.gameScriptService = gameScriptService;
        }

        protected override void ViewCreated(UserControl view)
        {
            windowManager = new ZWindowManager();
            windowContainer = this.View.FindName<Grid>("windowContainer");
        }

        void storyService_StoryOpened(object sender, StoryOpenedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DebuggerService_MachineCreated(object sender, MachineCreatedEventArgs e)
        {
            mainWindow = windowManager.Open(ZWindowType.TextBuffer);
            windowContainer.Children.Add(mainWindow);
            upperWindow = windowManager.Open(ZWindowType.TextGrid, mainWindow, ZWindowPosition.Above, ZWindowSizeType.Fixed, 0);

            windowManager.Activate(mainWindow);

            debuggerService.Machine.RegisterScreen(this);
            debuggerService.Machine.RegisterSoundEngine(this);
        }

        private void StoryService_StoryClosing(object sender, StoryClosingEventArgs e)
        {
            windowManager.Root.Close();

            mainWindow = null;
            upperWindow = null;
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
            return (storyService.Story.Memory.ReadWord(0x10) & 0x02) == 0x02;
        }

        private bool IsScoreGame()
        {
            // TODO: Move into appropriate API
            var story = storyService.Story;
            if (story.Version > 3)
            {
                throw new InvalidOperationException("status line should only be drawn be V1- V3");
            }

            if (story.Version < 3)
            {
                return true;
            }

            return (story.Memory.ReadByte(0x01) & 0x01) == 0x00;
        }

        private void ResetStatusHeight()
        {
            if (upperWindow != null)
            {
                int height = upperWindow.GetHeight();
                if (machStatusHeight != height)
                {
                    upperWindow.SetHeight(machStatusHeight);
                }
            }
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

        public void ReadChar(Action<char> callback)
        {
            debuggerService.BeginAwaitingInput();

            mainWindow.ReadChar(ch =>
            {
                ResetStatusHeight();
                currStatusHeight = 0;

                callback(ch);
                debuggerService.EndAwaitingInput();
            });
        }

        public void ReadCommand(int maxChars, Action<string> callback)
        {
            if (gameScriptService.HasNextCommand())
            {
                ResetStatusHeight();
                currStatusHeight = 0;

                string command = gameScriptService.GetNextCommand();
                windowManager.ActiveWindow.PutString(command + "\r\n");
                callback(command);
            }
            else
            {
                debuggerService.BeginAwaitingInput();

                mainWindow.ReadCommand(maxChars, text =>
                {
                    ResetStatusHeight();
                    currStatusHeight = 0;

                    callback(text);
                    debuggerService.EndAwaitingInput();
                });
            }
        }

        public void Clear(int window)
        {
            if (window == 0)
            {
                mainWindow.Clear();
            }
            else if (window == 1 && upperWindow != null)
            {
                upperWindow.Clear();
                ResetStatusHeight();
                currStatusHeight = 0;
            }
        }

        public void ClearAll(bool unsplit)
        {
            mainWindow.Clear();

            if (upperWindow != null)
            {
                if (unsplit)
                {
                    Unsplit();
                }
                else
                {
                    upperWindow.Clear();
                }
            }
        }

        public void Split(int lines)
        {
            if (upperWindow == null)
            {
                return;
            }

            if (lines == 0 || lines > currStatusHeight)
            {
                int height = upperWindow.GetHeight();
                if (lines != height)
                {
                    upperWindow.SetHeight(lines);
                    currStatusHeight = lines;
                }
            }

            machStatusHeight = lines;

            if (storyService.Story.Version == 3)
            {
                upperWindow.Clear();
            }
        }

        public void Unsplit()
        {
            if (upperWindow != null)
            {
                upperWindow.SetHeight(0);
                upperWindow.Clear();
                ResetStatusHeight();
                currStatusHeight = 0;
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

        public int GetCursorLine()
        {
            return windowManager.ActiveWindow.GetCursorLine();
        }

        public int GetCursorColumn()
        {
            return windowManager.ActiveWindow.GetCursorColumn();
        }

        public void SetCursor(int line, int column)
        {
            windowManager.ActiveWindow.SetCursor(column, line);
        }

        public void SetTextStyle(ZTextStyle style)
        {
            var activeWindow = windowManager.ActiveWindow;

            if (style == ZTextStyle.Roman)
            {
                activeWindow.SetBold(false);
                activeWindow.SetItalic(false);
                activeWindow.SetFixedPitch(false);
                activeWindow.SetReverse(false);
            }
            else if (style == ZTextStyle.Bold)
            {
                activeWindow.SetBold(true);
            }
            else if (style == ZTextStyle.Italic)
            {
                activeWindow.SetItalic(true);
            }
            else if (style == ZTextStyle.FixedPitch)
            {
                activeWindow.SetFixedPitch(true);
            }
            else if (style == ZTextStyle.Reverse)
            {
                activeWindow.SetReverse(true);
            }
        }

        public ZFont SetFont(ZFont font)
        {
            if (font == ZFont.Normal)
            {
                FontsAndColorsService.FontFamily = FontsAndColorsService.NormalFontFamily;
            }
            else if (font == ZFont.Fixed)
            {
                FontsAndColorsService.FontFamily = FontsAndColorsService.FixedFontFamily;
            }
            else
            {
                return 0;
            }

            var oldFont = this.font;
            this.font = font;
            return oldFont;
        }

        private Brush GetZColorBrush(ZColor color)
        {
            switch (color)
            {
                case ZColor.Black:
                    return Brushes.Black;
                case ZColor.Red:
                    return Brushes.Red;
                case ZColor.Green:
                    return Brushes.Green;
                case ZColor.Yellow:
                    return Brushes.Yellow;
                case ZColor.Blue:
                    return Brushes.Blue;
                case ZColor.Magenta:
                    return Brushes.Magenta;
                case ZColor.Cyan:
                    return Brushes.Cyan;
                case ZColor.White:
                    return Brushes.White;
                case ZColor.Gray:
                    return Brushes.Gray;

                default:
                    throw new ArgumentException("Unexpected color: " + color, "color");
            }
        }

        public void SetForegroundColor(ZColor color)
        {
            var brush = color == ZColor.Default
                ? FontsAndColorsService.DefaultForeground
                : GetZColorBrush(color);

            FontsAndColorsService.Foreground = brush;
        }

        public void SetBackgroundColor(ZColor color)
        {
            var brush = color == ZColor.Default
                ? FontsAndColorsService.DefaultBackground
                : GetZColorBrush(color);

            FontsAndColorsService.Background = brush;
        }

        public void ShowStatus()
        {
            var story = storyService.Story;
            if (story.Version > 3)
            {
                return;
            }

            if (upperWindow == null)
            {
                upperWindow = windowManager.Open(ZWindowType.TextGrid, mainWindow, ZWindowPosition.Above, ZWindowSizeType.Fixed, 1);
            }
            else
            {
                int height = upperWindow.GetHeight();
                if (height != 1)
                {
                    upperWindow.SetHeight(1);
                    currStatusHeight = 1;
                    machStatusHeight = 1;
                }
            }

            upperWindow.Clear();

            var charWidth = ScreenWidthInColumns;
            var locationText = " " + story.ObjectTable.GetByNumber(story.GlobalVariablesTable[0]).ShortName;

            upperWindow.SetReverse(true);

            if (charWidth < 5)
            {
                upperWindow.PutString(new string(' ', charWidth));
                return;
            }

            if (locationText.Length > charWidth)
            {
                locationText = locationText.Substring(0, charWidth - 3) + "...";
                upperWindow.PutString(locationText);
                return;
            }

            upperWindow.PutString(locationText);

            string rightText;
            if (IsScoreGame())
            {
                int score = (short)story.GlobalVariablesTable[1];
                int moves = (ushort)story.GlobalVariablesTable[2];
                rightText = string.Format("Score: {0,-8} Moves: {1,-6} ", score, moves);
            }
            else
            {
                int hours = (ushort)story.GlobalVariablesTable[1];
                int minutes = (ushort)story.GlobalVariablesTable[2];
                var pm = (hours / 12) > 0;
                if (pm)
                {
                    hours = hours % 12;
                }

                rightText = string.Format("{0}:{1:n2} {2}", hours, minutes, (pm ? "pm" : "am"));
            }

            if (rightText.Length < charWidth - locationText.Length - 1)
            {
                upperWindow.PutString(new string(' ', charWidth - locationText.Length - rightText.Length));
                upperWindow.PutString(rightText);
            }
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

        public ZColor DefaultBackgroundColor
        {
            get { return ZColor.White; }
        }

        public ZColor DefaultForegroundColor
        {
            get { return ZColor.Black; }
        }


        public void HighBeep()
        {
            SystemSounds.Asterisk.Play();
        }

        public void LowBeep()
        {
            SystemSounds.Beep.Play();
        }
    }
}
