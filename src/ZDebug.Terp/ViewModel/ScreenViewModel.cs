using System;
using System.Composition;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ZDebug.Core.Execution;
using ZDebug.Core.Extensions;
using ZDebug.IO.Services;
using ZDebug.IO.Windows;
using ZDebug.UI.Extensions;
using ZDebug.UI.Services;
using ZDebug.UI.ViewModel;

namespace ZDebug.Terp.ViewModel
{
    [Export, Shared]
    internal class ScreenViewModel : ViewModelWithViewBase<UserControl>, IScreen
    {
        private readonly StoryService storyService;
        private readonly GameScriptService gameScriptService;

        private ZWindowManager windowManager;
        private Grid windowContainer;

        private ZWindow mainWindow;
        private ZWindow upperWindow;

        private ZFont font;

        private int currStatusHeight;
        private int machStatusHeight;

        [ImportingConstructor]
        public ScreenViewModel(
            StoryService storyService,
            GameScriptService gameScriptService)
            : base("ScreenView")
        {
            this.storyService = storyService;

            this.storyService.StoryOpened += StoryService_StoryOpened;
            this.storyService.StoryClosing += StoryService_StoryClosing;

            this.gameScriptService = gameScriptService;
        }

        protected override void ViewCreated(UserControl view)
        {
            windowManager = new ZWindowManager();
            windowContainer = this.View.FindName<Grid>("windowContainer");
        }

        private void StoryService_StoryOpened(object sender, StoryOpenedEventArgs e)
        {
            mainWindow = windowManager.Open(ZWindowType.TextBuffer);
            windowContainer.Children.Add(mainWindow);
            upperWindow = windowManager.Open(ZWindowType.TextGrid, mainWindow, ZWindowPosition.Above, ZWindowSizeType.Fixed, 0);

            windowManager.Activate(mainWindow);
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
                foreground: Brushes.Black,
                pixelsPerDip: 1.0);
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

        private void ResetStatusHeight()
        {
            Dispatch(() =>
            {
                if (upperWindow != null)
                {
                    int height = upperWindow.GetHeight();
                    if (machStatusHeight != height)
                    {
                        upperWindow.SetHeight(machStatusHeight);
                    }
                }
            });
        }

        void IScreen.Clear(int window)
        {
            Dispatch(() =>
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
            });
        }

        void IScreen.ClearAll(bool unsplit)
        {
            Dispatch(() =>
            {
                mainWindow.Clear();

                if (upperWindow != null)
                {
                    if (unsplit)
                    {
                        ((IScreen)this).Unsplit();
                    }
                    else
                    {
                        upperWindow.Clear();
                    }
                }
            });
        }

        void IScreen.Split(int lines)
        {
            Dispatch(() =>
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
            });
        }

        void IScreen.Unsplit()
        {
            Dispatch(() =>
            {
                if (upperWindow != null)
                {
                    upperWindow.SetHeight(0);
                    upperWindow.Clear();
                    ResetStatusHeight();
                    currStatusHeight = 0;
                }
            });
        }

        void IScreen.SetWindow(int window)
        {
            Dispatch(() =>
            {
                if (window == 0)
                {
                    mainWindow.Activate();
                }
                else if (window == 1)
                {
                    upperWindow.Activate();
                }
            });
        }

        int IScreen.GetCursorLine()
        {
            return windowManager.ActiveWindow.GetCursorLine();
        }

        int IScreen.GetCursorColumn()
        {
            return windowManager.ActiveWindow.GetCursorColumn();
        }

        void IScreen.SetCursor(int line, int column)
        {
            Dispatch(() =>
            {
                windowManager.ActiveWindow.SetCursor(column, line);
            });
        }

        void IScreen.SetTextStyle(ZTextStyle style)
        {
            Dispatch(() =>
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
            });
        }

        void IScreen.SetForegroundColor(ZColor color)
        {
            Dispatch(() =>
            {
                var brush = color == ZColor.Default
                    ? FontsAndColorsService.DefaultForeground
                    : GetZColorBrush(color);

                FontsAndColorsService.Foreground = brush;
            });
        }

        void IScreen.SetBackgroundColor(ZColor color)
        {
            Dispatch(() =>
            {
                var brush = color == ZColor.Default
                    ? FontsAndColorsService.DefaultBackground
                    : GetZColorBrush(color);

                FontsAndColorsService.Background = brush;
            });
        }

        ZFont IScreen.SetFont(ZFont font)
        {
            if (font == ZFont.Normal)
            {
                Dispatch(() =>
                {
                    FontsAndColorsService.FontFamily = FontsAndColorsService.NormalFontFamily;
                });
            }
            else if (font == ZFont.Fixed)
            {
                Dispatch(() =>
                {
                    FontsAndColorsService.FontFamily = FontsAndColorsService.FixedFontFamily;
                });
            }
            else
            {
                return 0;
            }

            var oldFont = this.font;
            this.font = font;
            return oldFont;
        }

        void IScreen.ShowStatus()
        {
            Dispatch(() =>
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

                var charWidth = ((IScreen)this).ScreenWidthInColumns;
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
            });
        }

        byte IScreen.ScreenHeightInLines
        {
            get { return (byte)(windowContainer.ActualHeight / GetFixedFontMeasureText().Height); }
        }

        byte IScreen.ScreenWidthInColumns
        {
            get { return (byte)(windowContainer.ActualWidth / GetFixedFontMeasureText().Width); }
        }

        ushort IScreen.ScreenHeightInUnits
        {
            get { return (ushort)windowContainer.ActualHeight; }
        }

        ushort IScreen.ScreenWidthInUnits
        {
            get { return (ushort)windowContainer.ActualWidth; }
        }

        byte IScreen.FontHeightInUnits
        {
            get { return (byte)GetFixedFontMeasureText().Height; }
        }

        byte IScreen.FontWidthInUnits
        {
            get { return (byte)GetFixedFontMeasureText().Width; }
        }

        ZColor IScreen.DefaultBackgroundColor
        {
            get { return ZColor.White; }
        }

        ZColor IScreen.DefaultForegroundColor
        {
            get { return ZColor.Black; }
        }

        void IOutputStream.Print(string text)
        {
            Dispatch(() =>
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
            });
        }

        void IOutputStream.Print(char ch)
        {
            Dispatch(() =>
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
            });
        }

        void IInputStream.ReadChar(Action<char> callback)
        {
            Dispatch(() =>
            {
                mainWindow.ReadChar(ch =>
                {
                    ResetStatusHeight();
                    currStatusHeight = 0;

                    callback(ch);
                });
            });
        }

        void IInputStream.ReadCommand(int maxChars, Action<string> callback)
        {
            Dispatch(() =>
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
                    mainWindow.ReadCommand(maxChars, text =>
                    {
                        ResetStatusHeight();
                        currStatusHeight = 0;

                        callback(text);
                    });
                }
            });
        }
    }
}
