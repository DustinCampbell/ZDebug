using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using ZDebug.Compiler;
using ZDebug.Core.Execution;
using ZDebug.Terp.Profiling;
using ZDebug.UI.Extensions;
using ZDebug.UI.Services;
using ZDebug.UI.Utilities;
using ZDebug.UI.ViewModel;

namespace ZDebug.Terp.ViewModel
{
    [Export]
    internal class MainWindowViewModel : ViewModelWithViewBase<Window>, ISoundEngine
    {
        private readonly StoryService storyService;
        private readonly ScreenViewModel screenViewModel;

        private IScreen screen;
        private CompiledZMachine zmachine;
        private Thread zmachineThread;
        private ZMachineProfiler profiler;
        private Stopwatch watch;
        private DispatcherTimer updateTimer;

        private TimeSpan compileTime;
        private int routinesCompiled;
        private double zcodeToILRatio;
        private int routinesExecuted;
        private int instructionsExecuted;
        private int calculatedVariableLoads;
        private int calculatedVariableStores;
        private int directCalls;
        private int calculatedCalls;

        [ImportingConstructor]
        public MainWindowViewModel(
            StoryService storyService,
            ScreenViewModel screenViewModel)
            : base("MainWindowView")
        {
            this.storyService = storyService;
            this.screenViewModel = screenViewModel;

            this.OpenStoryCommand = RegisterCommand(
                text: "Open",
                name: "Open",
                executed: OpenStoryExecuted,
                canExecute: OpenStoryCanExecute,
                inputGestures: new KeyGesture(Key.O, ModifierKeys.Control));

            this.ExitCommand = RegisterCommand(
                text: "Exit",
                name: "Exit",
                executed: ExitExecuted,
                canExecute: ExitCanExecute,
                inputGestures: new KeyGesture(Key.F4, ModifierKeys.Alt));

            this.AboutGameCommand = RegisterCommand(
                text: "AboutGame",
                name: "About Game",
                executed: AboutGameExecuted,
                canExecute: AboutGameCanExecute);
        }

        // commands...
        public ICommand OpenStoryCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }
        public ICommand AboutGameCommand { get; private set; }

        //private void OpenScript_Click(object sender, RoutedEventArgs e)
        //{
        //    var dialog = new OpenFileDialog
        //    {
        //        Title = "Open Game Script"
        //    };

        //    if (dialog.ShowDialog(this) == true)
        //    {
        //        script = File.ReadAllLines(dialog.FileName);
        //    }
        //}

        private bool OpenStoryCanExecute()
        {
            return true;
        }

        private void OpenStoryExecuted()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Open Story File",
                Filter = "Supported Files (*.z3,*.z4,*.z5,*.z6,*.z7,*.z8,*.zblorb)|*.z3;*.z4;*.z5;*.z6;*.z7;*.z8;*.zblorb|" +
                         "Z-Code Files (*.z3,*.z4,*.z5,*.z6,*.z7,*.z8)|*.z3;*.z4;*.z5;*.z6;*.z7;*.z8|" +
                         "Blorb Files (*.zblorb)|*.zblorb|" +
                         "All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog(this.View) == true)
            {
                storyService.OpenStory(dialog.FileName);
            }

            this.updateTimer.Start();
        }

        private bool ExitCanExecute()
        {
            return true;
        }

        private void ExitExecuted()
        {
            this.View.Close();
        }

        private bool AboutGameCanExecute()
        {
            return storyService.HasGameInfo;
        }

        private void AboutGameExecuted()
        {
            var gameInfoDialogViewModel = new GameInfoViewModel();
            var gameInfoDialog = gameInfoDialogViewModel.CreateView();
            gameInfoDialogViewModel.SetGameinfo(storyService.GameInfo);
            gameInfoDialog.Owner = this.View;
            gameInfoDialog.ShowDialog();
        }

        public string Title
        {
            get
            {
                return "Z-Terp";
            }
        }

        public TimeSpan CompileTime
        {
            get
            {
                return compileTime;
            }
        }

        public int RoutinesCompiled
        {
            get
            {
                return routinesCompiled;
            }
        }

        public double ZCodeToILRatio
        {
            get
            {
                return zcodeToILRatio;
            }
        }

        public double ZCodeToILRatioPercent
        {
            get
            {
                return zcodeToILRatio * 100;
            }
        }

        public int RoutinesExecuted
        {
            get
            {
                return routinesExecuted;
            }
        }

        public int InstructionsExecuted
        {
            get
            {
                return instructionsExecuted;
            }
        }

        public int CalculatedVariableLoads
        {
            get
            {
                return calculatedVariableLoads;
            }
        }

        public int CalculatedVariableStores
        {
            get
            {
                return calculatedVariableStores;
            }
        }

        public int DirectCalls
        {
            get
            {
                return directCalls;
            }
        }

        public int CalculatedCalls
        {
            get
            {
                return calculatedCalls;
            }
        }

        void StoryService_StoryOpened(object sender, StoryOpenedEventArgs e)
        {
            profiler = new ZMachineProfiler();
            PropertyChanged("Profiling");

            e.Story.RegisterInterpreter(new Interpreter());
            zmachine = new CompiledZMachine(e.Story, precompile: true, profiler: profiler);

            zmachine.RegisterScreen(screen);
            zmachine.RegisterSoundEngine(this);

            zmachineThread = new Thread(new ThreadStart(Run));
            zmachineThread.Start();

            PropertyChanged("Title");
        }

        void StoryService_StoryClosing(object sender, StoryClosingEventArgs e)
        {
            PropertyChanged("Title");
        }

        private void View_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (zmachineThread != null)
            {
                zmachineThread.Abort();
            }
        }

        private void Run()
        {
            watch = Stopwatch.StartNew();
            try
            {
                zmachine.Run();
            }
            catch (ZMachineQuitException)
            {
                // done
                updateTimer.Stop();
                UpdateProfilerStatistics();
            }
            catch (ZMachineInterruptedException)
            {
                // done
                updateTimer.Stop();
                UpdateProfilerStatistics();
            }
            catch (ThreadAbortException)
            {
                // done
                updateTimer.Stop();
            }
            catch (Exception ex)
            {
                updateTimer.Stop();
                screen.Print("\n");
                screen.Print(ex.GetType().FullName);
                screen.Print("\n");
                screen.Print(ex.Message);
                screen.Print("\n");
                screen.Print(ex.StackTrace);
                UpdateProfilerStatistics();
            }

            watch.Stop();
            updateTimer.Stop();

            if (profiler != null)
            {
                profiler.Stop(watch.Elapsed);
                PopulateProfilerData();
            }
        }

        private void UpdateProfilerStatistics()
        {
            if (profiler == null)
            {
                return;
            }

            var compilationStatistics = profiler.CompilationStatistics.ToList();
            if (compilationStatistics.Count > 0)
            {
                this.compileTime = new TimeSpan(compilationStatistics.Sum(s => s.CompileTime.Ticks));
                this.routinesCompiled = profiler.RoutinesCompiled;
                this.zcodeToILRatio = compilationStatistics.Select(s => (double)s.Size / (double)s.Routine.Length).Average();
                this.routinesExecuted = profiler.RoutinesExecuted;
                this.instructionsExecuted = profiler.InstructionsExecuted;
                this.calculatedVariableLoads = compilationStatistics.Sum(s => s.CalculatedLoadVariableCount);
                this.calculatedVariableStores = compilationStatistics.Sum(s => s.CalculatedStoreVariableCount);
                this.directCalls = profiler.DirectCallCount;
                this.calculatedCalls = profiler.CalculatedCallCount;

                PropertyChanged("CompileTime");
                PropertyChanged("RoutinesCompiled");
                PropertyChanged("ZCodeToILRatio");
                PropertyChanged("ZCodeToILRatioPercent");
                PropertyChanged("RoutinesExecuted");
                PropertyChanged("InstructionsExecuted");
                PropertyChanged("CalculatedVariableLoads");
                PropertyChanged("CalculatedVariableStores");
                PropertyChanged("DirectCalls");
                PropertyChanged("CalculatedCalls");
            }
        }

        private void PopulateProfilerData()
        {
            if (profiler == null)
            {
                return;
            }

            Dispatch(() =>
            {
                //callTree.ItemsSource = new List<ICall>() { profiler.RootCall };
                //routineGrid.ItemsSource = profiler.Routines;

                //var reader = new InstructionReader(0, Services.StoryService.Story.Memory);

                //var instructions = profiler.InstructionTimings.Select(timing =>
                //{
                //    reader.Address = timing.Item1;
                //    var i = reader.NextInstruction();
                //    return new
                //    {
                //        Instruction = i,
                //        Address = i.Address,
                //        OpcodeName = i.Opcode.Name,
                //        TimesExecuted = timing.Item2.Item1,
                //        TotalTime = timing.Item2.Item2
                //    };
                //});

                //instructionsGrid.ItemsSource = instructions.OrderByDescending(x => x.TotalTime);

                //var opcodes = from i in instructions
                //              group i by i.Instruction.Opcode.Name into g
                //              select new
                //              {
                //                  Name = g.Key,
                //                  TotalTime = g.Aggregate(TimeSpan.Zero, (r, t) => r + t.TotalTime),
                //                  Count = g.Sum(x => x.TimesExecuted)
                //              };

                //worstOpcodes.ItemsSource = opcodes.OrderByDescending(x => x.TotalTime);
            });
        }

        public bool Profiling
        {
            get
            {
                return profiler != null;
            }
        }

        void ISoundEngine.HighBeep()
        {
            SystemSounds.Asterisk.Play();
        }

        void ISoundEngine.LowBeep()
        {
            SystemSounds.Beep.Play();
        }

        protected override void ViewCreated(Window view)
        {
            storyService.StoryOpened += StoryService_StoryOpened;
            storyService.StoryClosing += StoryService_StoryClosing;

            this.View.Closing += View_Closing;

            var screenContent = this.View.FindName<Grid>("screenContent");
            var screenView = screenViewModel.CreateView();
            screenContent.Children.Add(screenView);
            this.screen = screenViewModel;

            this.updateTimer = new DispatcherTimer(
                interval: TimeSpan.FromMilliseconds(100),
                priority: DispatcherPriority.Normal,
                callback: delegate { UpdateProfilerStatistics(); },
                dispatcher: this.View.Dispatcher);

            this.updateTimer.Stop();
        }
    }
}
