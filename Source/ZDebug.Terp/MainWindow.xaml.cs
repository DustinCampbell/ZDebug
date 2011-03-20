using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using ZDebug.Compiler;
using ZDebug.Core;
using ZDebug.Core.Execution;
using ZDebug.Core.Instructions;
using ZDebug.IO.Services;
using ZDebug.IO.Windows;
using ZDebug.Terp.Profiling;

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

        private byte[] storyBytes;
        private ZDebug.Compiler.CompiledZMachine machine;
        private Thread machineThread;
        private ZMachineProfiler profiler;
        private Stopwatch watch;

        private string[] script;
        private int scriptIndex;

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

        private void OpenScript_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Open Game Script"
            };

            if (dialog.ShowDialog(this) == true)
            {
                script = File.ReadAllLines(dialog.FileName);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            machine.Stop();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (machineThread != null)
            {
                machineThread.Abort();
            }
        }

        private void OpenStory(string fileName)
        {
            if (machine != null)
            {
                windowManager.Root.Close();

                mainWindow = null;
                upperWindow = null;
                storyBytes = null;
                machine = null;
                profiler = null;
                script = null;
                scriptIndex = 0;
                watch = null;
            }

            storyBytes = File.ReadAllBytes(fileName);
            profiler = new ZMachineProfiler();
            machine = new ZDebug.Compiler.CompiledZMachine(Story.FromBytes(storyBytes), profiler: profiler);
            machine.RegisterScreen(this);
            machine.SetRandomSeed(42);

            mainWindow = windowManager.Open(ZWindowType.TextBuffer);
            windowContainer.Children.Add(mainWindow);
            upperWindow = windowManager.Open(ZWindowType.TextGrid, mainWindow, ZWindowPosition.Above);

            windowManager.Activate(mainWindow);

            machineThread = new Thread(new ThreadStart(Run));
            machineThread.Start();
        }

        private void Run()
        {
            watch = Stopwatch.StartNew();
            try
            {
                machine.Run();
            }
            catch (ZMachineQuitException)
            {
                // done
                UpdateProfilerStatistics();
            }
            catch (ZMachineInterruptedException)
            {
                // done
                UpdateProfilerStatistics();
            }
            catch (Exception ex)
            {
                Print("\n");
                Print(ex.GetType().FullName);
                Print("\n");
                Print(ex.Message);
                Print("\n");
                Print(ex.StackTrace);
                UpdateProfilerStatistics();
            }

            watch.Stop();

            if (profiler != null)
            {
                profiler.Stop(watch.Elapsed);
                PopulateProfilerData();
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

        private void Dispatch(Action method)
        {
            Dispatcher.BeginInvoke(method, DispatcherPriority.Normal);
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

        public void Clear(int window)
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

        public void ClearAll(bool unsplit)
        {
            Dispatch(() =>
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
            });
        }

        public void Split(int lines)
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

                if (machine.Version == 3)
                {
                    upperWindow.Clear();
                }
            });
        }

        public void Unsplit()
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

        public void SetWindow(int window)
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
            Dispatch(() =>
            {
                windowManager.ActiveWindow.SetCursor(column, line);
            });
        }

        public void SetTextStyle(ZTextStyle style)
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
            Dispatch(() =>
            {
                var brush = color == ZColor.Default
                    ? FontsAndColorsService.DefaultForeground
                    : GetZColorBrush(color);

                FontsAndColorsService.Foreground = brush;
            });
        }

        public void SetBackgroundColor(ZColor color)
        {
            Dispatch(() =>
            {
                var brush = color == ZColor.Default
                    ? FontsAndColorsService.DefaultBackground
                    : GetZColorBrush(color);

                FontsAndColorsService.Background = brush;
            });
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
            Dispatch(() =>
            {
                windowManager.ActiveWindow.PutString(text);
            });
        }

        public void Print(char ch)
        {
            Dispatch(() =>
            {
                windowManager.ActiveWindow.PutChar(ch);
            });
        }

        public void ReadChar(Action<char> callback)
        {
            UpdateProfilerStatistics();

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

        public void ReadCommand(int maxChars, Action<string> callback)
        {
            UpdateProfilerStatistics();

            if (script != null && scriptIndex >= script.Length)
            {
                machine.Stop();
                return;
            }

            Dispatch(() =>
            {
                if (script != null && scriptIndex < script.Length)
                {
                    ResetStatusHeight();
                    currStatusHeight = 0;
                    string command = script[scriptIndex++];
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

        private void UpdateProfilerStatistics()
        {
            if (profiler != null)
            {
                Dispatch(() =>
                {
                    var ticks = profiler.CompilationStatistics.Sum(s => s.CompileTime.Ticks);
                    var compileTime = new TimeSpan(ticks);

                    var ratios = profiler.CompilationStatistics.Select(s => (double)s.Size / (double)s.Routine.Length);
                    var ratio = ratios.Average();

                    var calculatedVariableLoads = profiler.CompilationStatistics.Sum(s => s.CalculatedLoadVariableCount);
                    var calculatedVariableStores = profiler.CompilationStatistics.Sum(s => s.CalculatedStoreVariableCount);

                    elapsedTimeText.Text = string.Format("{0:#,0}.{1:000}", compileTime.Seconds, compileTime.Milliseconds);
                    routinesCompiled.Text = profiler.RoutinesCompiled.ToString("#,#");
                    zcodeToILRatio.Text = string.Format("1 / {0:0.###} ({1:0.###}%)", ratio, ratio * 100);
                    routinesAndInstructionsExecuted.Text = string.Format("{0:#,0} / {1:#,0}", profiler.RoutinesExecuted, profiler.InstructionsExecuted);
                    calculatedVariables.Text = string.Format("{0:#,0} / {1:#,0}", calculatedVariableLoads, calculatedVariableStores);
                    calls.Text = string.Format("{0:#,0} / {1:#,0}", profiler.DirectCallCount, profiler.CalculatedCallCount);
                });
            }
        }

        private void PopulateProfilerData()
        {
            if (profiler != null)
            {
                Dispatch(() =>
                {
                    callTree.ItemsSource = new List<ICall>() { profiler.RootCall };
                    routineGrid.ItemsSource = profiler.Routines;

                    var reader = new InstructionReader(0, storyBytes);

                    var instructions = profiler.InstructionTimings.Select(timing =>
                    {
                        reader.Address = timing.Item1;
                        var i = reader.NextInstruction();
                        return new
                        {
                            Instruction = i,
                            Address = i.Address,
                            OpcodeName = i.Opcode.Name,
                            TimesExecuted = timing.Item2.Item1,
                            TotalTime = timing.Item2.Item2
                        };
                    });

                    instructionsGrid.ItemsSource = instructions.OrderByDescending(x => x.TotalTime);

                    var opcodes = from i in instructions
                                  group i by i.Instruction.Opcode.Name into g
                                  select new
                                  {
                                      Name = g.Key,
                                      TotalTime = g.Aggregate(TimeSpan.Zero, (r, t) => r + t.TotalTime),
                                      Count = g.Sum(x => x.TimesExecuted)
                                  };

                    worstOpcodes.ItemsSource = opcodes.OrderByDescending(x => x.TotalTime);
                });
            }
        }
    }
}
