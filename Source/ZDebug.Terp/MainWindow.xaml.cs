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

        public void Clear(int window)
        {
            throw new NotImplementedException();
        }

        public void ClearAll(bool unsplit = false)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
