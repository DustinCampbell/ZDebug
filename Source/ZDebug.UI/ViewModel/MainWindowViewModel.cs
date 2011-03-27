using System.ComponentModel.Composition;
using System.IO;
using System.Windows;
using System.Windows.Input;
using AvalonDock;
using Microsoft.Win32;
using ZDebug.UI.Extensions;
using ZDebug.UI.Services;
using ZDebug.UI.Utilities;

namespace ZDebug.UI.ViewModel
{
    [Export]
    internal class MainWindowViewModel : ViewModelWithViewBase<Window>
    {
        private readonly StoryService storyService;
        private readonly GameScriptService gameScriptService;
        private readonly DebuggerService debuggerService;
        private readonly NavigationService navigationService;

        private readonly StoryInfoViewModel storyInfoViewModel;
        private readonly MemoryMapViewModel memoryMapViewModel;
        private readonly GlobalsViewModel globalsViewModel;
        private readonly DisassemblyViewModel disassemblyViewModel;
        private readonly ObjectsViewModel objectsViewModel;
        private readonly LocalsViewModel localsViewModel;
        private readonly CallStackViewModel callStackViewModel;
        private readonly OutputViewModel outputViewModel;
        private readonly MessageLogViewModel messageLogViewModel;
        private readonly GoToAddressDialogViewModel goToAddressDialogViewModel;
        private readonly GameInfoDialogViewModel gameInfoDialogViewModel;
        private readonly GameScriptDialogViewModel gameScriptDialogViewModel;

        [ImportingConstructor]
        private MainWindowViewModel(
            StoryService storyService,
            GameScriptService gameScriptService,
            DebuggerService debuggerService,
            NavigationService navigationService,
            StoryInfoViewModel storyInfoViewModel,
            MemoryMapViewModel memoryMapViewModel,
            GlobalsViewModel globalsViewModel,
            DisassemblyViewModel disassemblyViewModel,
            ObjectsViewModel objectsViewModel,
            LocalsViewModel localsViewModel,
            CallStackViewModel callStackViewModel,
            OutputViewModel outputViewModel,
            MessageLogViewModel messageLogViewModel,
            GoToAddressDialogViewModel goToAddressDialogViewModel,
            GameInfoDialogViewModel gameInfoDialogViewModel,
            GameScriptDialogViewModel gameScriptDialogViewModel)
            : base("MainWindowView")
        {
            this.storyService = storyService;
            this.storyService.StoryOpened += StoryService_StoryOpened;
            this.storyService.StoryClosing += StoryService_StoryClosing;

            this.gameScriptService = gameScriptService;
            this.debuggerService = debuggerService;
            this.navigationService = navigationService;

            this.storyInfoViewModel = storyInfoViewModel;
            this.memoryMapViewModel = memoryMapViewModel;
            this.globalsViewModel = globalsViewModel;
            this.disassemblyViewModel = disassemblyViewModel;
            this.objectsViewModel = objectsViewModel;
            this.localsViewModel = localsViewModel;
            this.callStackViewModel = callStackViewModel;
            this.outputViewModel = outputViewModel;
            this.messageLogViewModel = messageLogViewModel;
            this.goToAddressDialogViewModel = goToAddressDialogViewModel;
            this.gameInfoDialogViewModel = gameInfoDialogViewModel;
            this.gameScriptDialogViewModel = gameScriptDialogViewModel;

            this.OpenStoryCommand = RegisterCommand(
                text: "Open",
                name: "Open",
                executed: OpenStoryExecuted,
                canExecute: CanOpenStoryExecute,
                inputGestures: new KeyGesture(Key.O, ModifierKeys.Control));

            this.EditGameScriptCommand = RegisterCommand(
                text: "EditGameScript",
                name: "Edit Game Script",
                executed: EditGameScriptExecuted,
                canExecute: CanEditGameScriptExecute);

            this.GoToAddressCommand = RegisterCommand(
                text: "GoToAddress",
                name: "GoToAddress",
                executed: GoToAddressExecuted,
                canExecute: CanGoToAddressExecute,
                inputGestures: new KeyGesture(Key.G, ModifierKeys.Control));

            this.ExitCommand = RegisterCommand(
                text: "Exit",
                name: "Exit",
                executed: ExitExecuted,
                canExecute: CanExitExecute,
                inputGestures: new KeyGesture(Key.F4, ModifierKeys.Alt));

            this.StartDebuggingCommand = RegisterCommand(
                text: "StartDebugging",
                name: "Start Debugging",
                executed: StartDebuggingExecuted,
                canExecute: CanStartDebuggingExecute,
                inputGestures: new KeyGesture(Key.F5));

            this.StopDebuggingCommand = RegisterCommand(
                text: "PauseDebugging",
                name: "Pause Debugging",
                executed: StopDebuggingExecuted,
                canExecute: CanStopDebuggingExecute,
                inputGestures: new KeyGesture(Key.F5, ModifierKeys.Shift));

            this.StepNextCommand = RegisterCommand(
                text: "StepNext",
                name: "Step to Next Instruction",
                executed: StepNextExecuted,
                canExecute: CanStepNextExecute,
                inputGestures: new KeyGesture(Key.F10));

            this.ResetSessionCommand = RegisterCommand(
                text: "ResetSession",
                name: "Reset Debugging Session",
                executed: ResetSessionExecuted,
                canExecute: CanResetSessionExecute);

            this.ResetWindowLayoutCommand = RegisterCommand(
                text: "ResetWindowLayout",
                name: "Reset Window Layout",
                executed: ResetWindowLayoutExecuted,
                canExecute: CanResetWindowLayoutExecute);

            this.AboutGameCommand = RegisterCommand(
                text: "AboutGame",
                name: "About Game",
                executed: AboutGameExecuted,
                canExecute: CanAboutGameExecute);
        }

        private bool CanOpenStoryExecute()
        {
            return debuggerService.State != DebuggerState.Running;
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
        }

        private bool CanEditGameScriptExecute()
        {
            return storyService.IsStoryOpen &&
                debuggerService.State != DebuggerState.Running;
        }

        private void EditGameScriptExecuted()
        {
            gameScriptDialogViewModel.ShowDialog(owner: this.View);
        }

        private bool CanGoToAddressExecute()
        {
            return storyService.IsStoryOpen &&
                debuggerService.State != DebuggerState.Running;
        }

        private void GoToAddressExecuted()
        {
            if (goToAddressDialogViewModel.ShowDialog(owner: this.View) == true)
            {
                navigationService.RequestNavigation(goToAddressDialogViewModel.Address);
            }
        }

        private bool CanExitExecute()
        {
            return debuggerService.State != DebuggerState.Running;
        }

        private void ExitExecuted()
        {
            this.View.Close();
        }

        private bool CanStartDebuggingExecute()
        {
            return debuggerService.CanStartDebugging;
        }

        private void StartDebuggingExecuted()
        {
            debuggerService.StartDebugging();
        }

        private bool CanStopDebuggingExecute()
        {
            return debuggerService.CanStopDebugging;
        }

        private void StopDebuggingExecuted()
        {
            debuggerService.StopDebugging();
        }

        private bool CanStepNextExecute()
        {
            return debuggerService.CanStepNext;
        }

        private void StepNextExecuted()
        {
            debuggerService.StepNext();
        }

        private bool CanResetSessionExecute()
        {
            return debuggerService.CanResetSession;
        }

        private void ResetSessionExecuted()
        {
            debuggerService.ResetSession();
        }

        private bool CanResetWindowLayoutExecute()
        {
            return true;
        }

        private void ResetWindowLayoutExecuted()
        {
            var dockManager = this.View.FindName<DockingManager>("dockManager");
            Storage.RestoreDockingLayout(dockManager, "original");
        }

        private bool CanAboutGameExecute()
        {
            return storyService.HasGameInfo;
        }

        private void AboutGameExecuted()
        {
            gameInfoDialogViewModel.ShowDialog(owner: this.View);
        }

        public ICommand OpenStoryCommand { get; private set; }
        public ICommand EditGameScriptCommand { get; private set; }
        public ICommand GoToAddressCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }
        public ICommand StartDebuggingCommand { get; private set; }
        public ICommand StopDebuggingCommand { get; private set; }
        public ICommand StepNextCommand { get; private set; }
        public ICommand ResetSessionCommand { get; private set; }
        public ICommand ResetWindowLayoutCommand { get; private set; }
        public ICommand AboutGameCommand { get; private set; }

        public string Title
        {
            get
            {
                if (storyService.IsStoryOpen)
                {
                    return "Z-Debug - " + Path.GetFileName(storyService.FileName).ToLower();
                }
                else
                {
                    return "Z-Debug";
                }
            }
        }

        private void StoryService_StoryOpened(object sender, StoryOpenedEventArgs e)
        {
            PropertyChanged("Title");
        }

        private void StoryService_StoryClosing(object sender, StoryClosingEventArgs e)
        {
            PropertyChanged("Title");
        }

        protected override void ViewCreated(Window view)
        {
            var storyInfoContent = this.View.FindName<DockableContent>("storyInfoContent");
            storyInfoContent.Content = this.storyInfoViewModel.CreateView();

            var memoryMapContent = this.View.FindName<DockableContent>("memoryMapContent");
            memoryMapContent.Content = this.memoryMapViewModel.CreateView();

            var globalsContent = this.View.FindName<DockableContent>("globalsContent");
            globalsContent.Content = this.globalsViewModel.CreateView();

            var disassemblyContent = this.View.FindName<DocumentContent>("disassemblyContent");
            disassemblyContent.Content = this.disassemblyViewModel.CreateView();

            var objectsContent = this.View.FindName<DocumentContent>("objectsContent");
            objectsContent.Content = this.objectsViewModel.CreateView();

            //var memoryContent = this.View.FindName<DocumentContent>("memoryContent");
            //memoryContent.Content = ViewModelWithView.Create<MemoryViewModel, UserControl>();

            var localsContent = this.View.FindName<DockableContent>("localsContent");
            localsContent.Content = this.localsViewModel.CreateView();

            var callStackContent = this.View.FindName<DockableContent>("callStackContent");
            callStackContent.Content = this.callStackViewModel.CreateView();

            var outputContent = this.View.FindName<DockableContent>("outputContent");
            outputContent.Content = this.outputViewModel.CreateView();

            var messagesContent = this.View.FindName<DockableContent>("messagesContent");
            messagesContent.Content = this.messageLogViewModel.CreateView();

            this.View.SourceInitialized += (s, e) =>
            {
                Storage.RestoreWindowLayout(this.View);
            };

            var dockManager = this.View.FindName<DockingManager>("dockManager");
            dockManager.Loaded += (s, e) =>
            {
                Storage.SaveDockingLayout(dockManager, "original");
                Storage.RestoreDockingLayout(dockManager);
            };

            this.View.Closing += (s, e) =>
            {
                storyService.CloseStory();
                Storage.SaveDockingLayout(dockManager);
                Storage.SaveWindowLayout(this.View);
            };
        }
    }
}
