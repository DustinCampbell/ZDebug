using System;
using System.Composition;
using System.Media;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using AvalonDock;
using Microsoft.Win32;
using ZDebug.Compiler;
using ZDebug.Core.Execution;
using ZDebug.Terp.Services;
using ZDebug.UI.Extensions;
using ZDebug.UI.Services;
using ZDebug.UI.Utilities;
using ZDebug.UI.ViewModel;

namespace ZDebug.Terp.ViewModel;

[Export, Shared]
internal class MainWindowViewModel : ViewModelWithViewBase<Window>, ISoundEngine
{
    private readonly StoryService storyService;
    private readonly GameScriptService gameScriptService;
    private readonly ProfilerService profilerService;

    private readonly ScreenViewModel screenViewModel;
    private readonly ProfilerViewModel profilerViewModel;
    private readonly GameInfoDialogViewModel gameInfoDialogViewModel;
    private readonly GameScriptDialogViewModel gameScriptDialogViewModel;

    private IScreen screen;
    private CompiledZMachine zmachine;
    private Thread zmachineThread;
    private DispatcherTimer updateTimer;

    [ImportingConstructor]
    public MainWindowViewModel(
        StoryService storyService,
        GameScriptService gameScriptService,
        ProfilerService profilerService,
        ScreenViewModel screenViewModel,
        ProfilerViewModel profilerViewModel,
        GameInfoDialogViewModel gameInfoDialogViewModel,
        GameScriptDialogViewModel gameScriptDialogViewModel)
        : base("MainWindowView")
    {
        this.storyService = storyService;
        this.storyService.StoryOpened += StoryService_StoryOpened;
        this.storyService.StoryClosing += StoryService_StoryClosing;

        this.gameScriptService = gameScriptService;

        this.profilerService = profilerService;

        this.screenViewModel = screenViewModel;
        this.profilerViewModel = profilerViewModel;
        this.gameInfoDialogViewModel = gameInfoDialogViewModel;
        this.gameScriptDialogViewModel = gameScriptDialogViewModel;

        OpenStoryCommand = RegisterCommand(
            text: "Open",
            name: "Open",
            executed: OpenStoryExecuted,
            canExecute: OpenStoryCanExecute,
            inputGestures: new KeyGesture(Key.O, ModifierKeys.Control));

        EditGameScriptCommand = RegisterCommand(
            text: "EditGameScript",
            name: "Edit Game Script",
            executed: EditGameScriptExecuted,
            canExecute: EditGameScriptCanExecute);

        ExitCommand = RegisterCommand(
            text: "Exit",
            name: "Exit",
            executed: ExitExecuted,
            canExecute: ExitCanExecute,
            inputGestures: new KeyGesture(Key.F4, ModifierKeys.Alt));

        StopCommand = RegisterCommand(
            text: "Stop",
            name: "Stop",
            executed: StopExecuted,
            canExecute: StopCanExecute);

        AboutGameCommand = RegisterCommand(
            text: "AboutGame",
            name: "About Game",
            executed: AboutGameExecuted,
            canExecute: AboutGameCanExecute);
    }

    protected override void ViewCreated(Window view)
    {
        var screenContent = View.FindName<DocumentContent>("screenContent");
        screenContent.Content = screenViewModel.CreateView();
        screen = screenViewModel;

        var profilerContent = View.FindName<DocumentContent>("profilerContent");
        profilerContent.Content = profilerViewModel.CreateView();

        updateTimer = new DispatcherTimer(
            interval: TimeSpan.FromMilliseconds(100),
            priority: DispatcherPriority.Normal,
            callback: delegate { UpdateProfilerStatistics(); },
            dispatcher: View.Dispatcher);

        updateTimer.Stop();

        View.SourceInitialized += (s, e) =>
        {
            Storage.RestoreWindowLayout(View);
        };

        var dockManager = View.FindName<DockingManager>("dockManager");
        dockManager.Loaded += (s, e) =>
        {
            Storage.SaveDockingLayout(dockManager, "original");
            Storage.RestoreDockingLayout(dockManager);
        };

        View.Closing += (s, e) =>
        {
            storyService.CloseStory();
            Storage.SaveDockingLayout(dockManager);
            Storage.SaveWindowLayout(View);
        };
    }

    // commands...
    public ICommand OpenStoryCommand { get; private set; }
    public ICommand EditGameScriptCommand { get; private set; }
    public ICommand ExitCommand { get; private set; }
    public ICommand StopCommand { get; private set; }
    public ICommand AboutGameCommand { get; private set; }

    private bool OpenStoryCanExecute() => true;

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

        if (dialog.ShowDialog(View) == true)
        {
            storyService.OpenStory(dialog.FileName);
        }

        updateTimer.Start();
    }

    private bool EditGameScriptCanExecute() => true;

    private void EditGameScriptExecuted() => gameScriptDialogViewModel.ShowDialog(owner: View);

    private bool ExitCanExecute() => true;

    private void ExitExecuted() => View.Close();

    private bool StopCanExecute() => zmachine != null;

    private void StopExecuted() => zmachine.Stop();

    private bool AboutGameCanExecute() => storyService.HasGameInfo;

    private void AboutGameExecuted() => gameInfoDialogViewModel.ShowDialog(owner: View);

    public string Title => "Z-Terp";

    private void StoryService_StoryOpened(object sender, StoryOpenedEventArgs e)
    {
        //profilerService.Create();
        PropertyChanged("Profiling");

        e.Story.RegisterInterpreter(new Interpreter());
        zmachine = new CompiledZMachine(e.Story, profiler: profilerService.Profiler);
        zmachine.SetRandomSeed(42);

        zmachine.RegisterScreen(screen);
        zmachine.RegisterSoundEngine(this);

        zmachineThread = new Thread(new ThreadStart(Run));
        zmachineThread.Start();

        PropertyChanged("Title");
    }

    private void StoryService_StoryClosing(object sender, StoryClosingEventArgs e)
    {
        if (zmachineThread != null)
        {
            zmachineThread.Abort();
        }

        profilerService.Destroy();

        zmachineThread = null;
        zmachine = null;

        PropertyChanged("Title");
    }

    private void Run()
    {
        profilerService.Start();
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

        updateTimer.Stop();
        profilerService.Stop();
    }

    void ISoundEngine.HighBeep() => SystemSounds.Asterisk.Play();

    void ISoundEngine.LowBeep() => SystemSounds.Beep.Play();

    private void UpdateProfilerStatistics()
    {
        profilerService.UpdateProfilerStatistics();

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

    public bool Profiling => profilerService.Profiling;

    public TimeSpan CompileTime => profilerService.CompileTime;

    public int RoutinesCompiled => profilerService.RoutinesCompiled;

    public double ZCodeToILRatio => profilerService.ZCodeToILRatio;

    public double ZCodeToILRatioPercent => profilerService.ZCodeToILRatio * 100;

    public int RoutinesExecuted => profilerService.RoutinesExecuted;

    public int InstructionsExecuted => profilerService.InstructionsExecuted;

    public int CalculatedVariableLoads => profilerService.CalculatedVariableLoads;

    public int CalculatedVariableStores => profilerService.CalculatedVariableStores;

    public int DirectCalls => profilerService.DirectCalls;

    public int CalculatedCalls => profilerService.CalculatedCalls;
}
