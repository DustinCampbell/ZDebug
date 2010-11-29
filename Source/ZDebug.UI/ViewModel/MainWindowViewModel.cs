using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AvalonDock;
using Microsoft.Win32;
using ZDebug.UI.Services;
using ZDebug.UI.Utilities;

namespace ZDebug.UI.ViewModel
{
    internal class MainWindowViewModel : ViewModelWithViewBase<Window>
    {
        public MainWindowViewModel()
            : base("MainWindowView")
        {
            this.OpenStoryCommand = RegisterCommand(
                text: "Open",
                name: "Open",
                executed: OpenStoryExecuted,
                canExecute: CanOpenStoryExecute,
                inputGestures: new KeyGesture(Key.O, ModifierKeys.Control));

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
        }

        private bool CanOpenStoryExecute()
        {
            return true;
        }

        private void OpenStoryExecuted()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Open Z-Code File",
                Filter = "Z-Code Files (*.z3,*.z4,*.z5,*.z6,*.z7,*.z8)|*.z3;*.z4;*.z5;*.z6;*.z7;*.z8|All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog(this.View) == true)
            {
                DebuggerService.OpenStory(dialog.FileName);
            }
        }

        private bool CanExitExecute()
        {
            return true;
        }

        private void ExitExecuted()
        {
            this.View.Close();
        }

        private bool CanStartDebuggingExecute()
        {
            return false;
        }

        private void StartDebuggingExecuted()
        {
        }

        private bool CanStepNextExecute()
        {
            return false;
        }

        private void StepNextExecuted()
        {
        }

        private bool CanResetSessionExecute()
        {
            return false;
        }

        private void ResetSessionExecuted()
        {
        }

        public ICommand OpenStoryCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }
        public ICommand StartDebuggingCommand { get; private set; }
        public ICommand StepNextCommand { get; private set; }
        public ICommand ResetSessionCommand { get; private set; }

        public string Title
        {
            get
            {
                if (DebuggerService.HasStory)
                {
                    return "Z-Debug - " + Path.GetFileName(DebuggerService.FileName).ToLower();
                }
                else
                {
                    return "Z-Debug";
                }
            }
        }

        private void StoryOpened(object sender, StoryEventArgs e)
        {
            PropertyChanged("Title");
        }

        private void StoryClosed(object sender, StoryEventArgs e)
        {
            PropertyChanged("Title");
        }

        protected internal override void Initialize()
        {
            DebuggerService.StoryOpened += StoryOpened;
            DebuggerService.StoryClosed += StoryClosed;

            var storyInfoContent = this.View.FindName<DockableContent>("storyInfoContent");
            storyInfoContent.Content = ViewModelWithView.Create<StoryInfoViewModel, UserControl>();

            var disassemblyContent = this.View.FindName<DocumentContent>("disassemblyContent");
            disassemblyContent.Content = ViewModelWithView.Create<DisassemblyViewModel, UserControl>();

            var objectsContent = this.View.FindName<DocumentContent>("objectsContent");
            objectsContent.Content = ViewModelWithView.Create<ObjectsViewModel, UserControl>();

            var memoryContent = this.View.FindName<DocumentContent>("memoryContent");
            memoryContent.Content = ViewModelWithView.Create<MemoryViewModel, UserControl>();

            this.View.SourceInitialized += (s, e) =>
            {
                Storage.RestoreWindowLayout(this.View);
            };

            var dockManager = this.View.FindName<DockingManager>("dockManager");
            dockManager.Loaded += (s, e) =>
            {
                Storage.RestoreDockingLayout(dockManager);
            };

            this.View.Closing += (s, e) =>
            {
                DebuggerService.CloseStory();
                Storage.SaveDockingLayout(dockManager);
                Storage.SaveWindowLayout(this.View);
            };
        }
    }
}
