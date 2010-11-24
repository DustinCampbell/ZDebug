﻿using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

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
                Filter = "Z-Code Files (*.z3,*.z4,*.z5)|*.z3;*.z4;*.z5|All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog(this.View) == true)
            {

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
    }
}
