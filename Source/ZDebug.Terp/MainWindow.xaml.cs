using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using ZDebug.Compiler;
using ZDebug.IO.Windows;
using ZDebug.Core.Execution;
using System.Windows.Threading;
using ZDebug.IO.Services;
using System.Globalization;

namespace ZDebug.Terp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IScreen
    {
        private ZWindowManager windowManager;
        private ZWindow mainWindow;
        private ZWindow upperWindow;

        private int currStatusHeight;
        private int machStatusHeight;

        private ZMachine machine;

        public MainWindow()
        {
            InitializeComponent();

            windowManager = new ZWindowManager();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Open Story File",
                Filter = "Supported Files (*.z3,*.z4,*.z5,*.z6,*.z7,*.z8,*.zblorb)|*.z3;*.z4;*.z5;*.z6;*.z7;*.z8;*.zblorb|" +
                         "Z-Code Files (*.z3,*.z4,*.z5,*.z6,*.z7,*.z8)|*.z3;*.z4;*.z5;*.z6;*.z7;*.z8|" +
                         "Blorb Files (*.zblorb)|*.zblorb|" +
                         "All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog(this) == true)
            {
                OpenStory(dialog.FileName);
            }
        }

        private void OpenStory(string fileName)
        {
            if (machine != null)
            {
                windowManager.Root.Close();

                mainWindow = null;
                upperWindow = null;
                machine = null;
            }

            var memory = File.ReadAllBytes(fileName);
            machine = new ZMachine(memory, this);

            mainWindow = windowManager.Open(ZWindowType.TextBuffer);
            windowContainer.Children.Add(mainWindow);
            upperWindow = windowManager.Open(ZWindowType.TextGrid, mainWindow, ZWindowPosition.Above);

            windowManager.Activate(mainWindow);

            Dispatcher.BeginInvoke(new Action(Run), DispatcherPriority.Background);
        }

        private void Run()
        {
            try
            {
                machine.Run();
            }
            catch (Exception ex)
            {
                Print("\n");
                Print(ex.GetType().FullName);
                Print("\n");
                Print(ex.Message);
                Print("\n");
                Print(ex.StackTrace);
            }
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

            if (machine.Version == 3)
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
            throw new NotImplementedException();
        }

        public int GetCursorColumn()
        {
            throw new NotImplementedException();
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

        public ZFont SetFont(ZFont font)
        {
            throw new NotImplementedException();
        }

        public void ShowStatus()
        {
            //var story = DebuggerService.Story;
            //if (story.Version > 3)
            //{
            //    return;
            //}

            //if (upperWindow == null)
            //{
            //    upperWindow = windowManager.Open(ZWindowType.TextGrid, mainWindow, ZWindowPosition.Above, ZWindowSizeType.Fixed, 1);
            //}
            //else
            //{
            //    int height = upperWindow.GetHeight();
            //    if (height != 1)
            //    {
            //        upperWindow.SetHeight(1);
            //        currStatusHeight = 1;
            //        machStatusHeight = 1;
            //    }
            //}

            //upperWindow.Clear();

            //var charWidth = ScreenWidthInColumns;
            //var locationText = " " + story.ObjectTable.GetByNumber(story.GlobalVariablesTable[0]).ShortName;

            //upperWindow.SetReverse(true);

            //if (charWidth < 5)
            //{
            //    upperWindow.PutString(new string(' ', charWidth));
            //    return;
            //}

            //if (locationText.Length > charWidth)
            //{
            //    locationText = locationText.Substring(0, charWidth - 3) + "...";
            //    upperWindow.PutString(locationText);
            //    return;
            //}

            //upperWindow.PutString(locationText);

            //string rightText;
            //if (IsScoreGame())
            //{
            //    int score = (short)story.GlobalVariablesTable[1];
            //    int moves = (ushort)story.GlobalVariablesTable[2];
            //    rightText = string.Format("Score: {0,-8} Moves: {1,-6} ", score, moves);
            //}
            //else
            //{
            //    int hours = (ushort)story.GlobalVariablesTable[1];
            //    int minutes = (ushort)story.GlobalVariablesTable[2];
            //    var pm = (hours / 12) > 0;
            //    if (pm)
            //    {
            //        hours = hours % 12;
            //    }

            //    rightText = string.Format("{0}:{1:n2} {2}", hours, minutes, (pm ? "pm" : "am"));
            //}

            //if (rightText.Length < charWidth - locationText.Length - 1)
            //{
            //    upperWindow.PutString(new string(' ', charWidth - locationText.Length - rightText.Length));
            //    upperWindow.PutString(rightText);
            //}
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

        public void Print(string text)
        {
            windowManager.ActiveWindow.PutString(text);
        }

        public void Print(char ch)
        {
            windowManager.ActiveWindow.PutChar(ch);
        }

        public void ReadChar(Action<char> callback)
        {
            throw new NotImplementedException();
        }

        public void ReadCommand(int maxChars, Action<string> callback)
        {
            throw new NotImplementedException();
        }
    }
}
