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

        public void Clear(int window)
        {
            throw new NotImplementedException();
        }

        public void ClearAll(bool unsplit = false)
        {
            throw new NotImplementedException();
        }

        public void Split(int height)
        {
            throw new NotImplementedException();
        }

        public void Unsplit()
        {
            throw new NotImplementedException();
        }

        public void SetWindow(int window)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void SetTextStyle(ZTextStyle style)
        {
            throw new NotImplementedException();
        }

        public void SetForegroundColor(ZColor color)
        {
            throw new NotImplementedException();
        }

        public void SetBackgroundColor(ZColor color)
        {
            throw new NotImplementedException();
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
            get { throw new NotImplementedException(); }
        }

        public byte ScreenWidthInColumns
        {
            get { throw new NotImplementedException(); }
        }

        public ushort ScreenHeightInUnits
        {
            get { throw new NotImplementedException(); }
        }

        public ushort ScreenWidthInUnits
        {
            get { throw new NotImplementedException(); }
        }

        public byte FontHeightInUnits
        {
            get { throw new NotImplementedException(); }
        }

        public byte FontWidthInUnits
        {
            get { throw new NotImplementedException(); }
        }

        public ZColor DefaultBackgroundColor
        {
            get { throw new NotImplementedException(); }
        }

        public ZColor DefaultForegroundColor
        {
            get { throw new NotImplementedException(); }
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
