using System;
using System.Windows;
using System.Windows.Input;

namespace ZDebug.UI.ViewModel
{
    internal abstract partial class ViewModelBase
    {
        private readonly CommandBindingCollection commandBindings = new CommandBindingCollection();

        private void AddCommandBinding(CommandBinding binding)
        {
            CommandManager.RegisterClassCommandBinding(this.GetType(), binding);
            commandBindings.Add(binding);
        }

        private ICommand RegisterCommand(string text, string name, InputGesture[] inputGestures, ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute)
        {
            var command = new RoutedUICommand(text, name, this.GetType(), new InputGestureCollection(inputGestures));
            var binding = new CommandBinding(command, executed, canExecute);

            AddCommandBinding(binding);

            return command;
        }

        protected ICommand RegisterCommand(string text, string name, Action executed, Func<bool> canExecute, params InputGesture[] inputGestures)
        {
            return RegisterCommand(text, name, inputGestures,
                executed: (s, e) => executed(),
                canExecute: (s, e) => e.CanExecute = canExecute());
        }

        protected ICommand RegisterCommand<T>(string text, string name, Action<T> executed, Func<T, bool> canExecute, params InputGesture[] inputGestures)
        {
            Func<object, T> cast = x => x != null ? (T)x : default(T);

            return RegisterCommand(text, name, inputGestures,
                executed: (s, e) => executed(cast(e.Parameter)),
                canExecute: (s, e) => e.CanExecute = canExecute(cast(e.Parameter)));
        }

        public static readonly DependencyProperty RegisterViewModelCommandsProperty =
            DependencyProperty.RegisterAttached(
                name: "RegisterViewModelCommands",
                propertyType: typeof(ViewModelBase),
                ownerType: typeof(ViewModelBase),
                defaultMetadata: new PropertyMetadata(
                    defaultValue: null,
                    propertyChangedCallback: (dp, e) =>
                    {
                        var element = dp as UIElement;
                        if (element != null)
                        {
                            var viewModel = e.NewValue as ViewModelBase;
                            if (viewModel != null && viewModel.commandBindings != null)
                            {
                                element.CommandBindings.AddRange(viewModel.commandBindings);
                            }
                        }
                    }));

        public static ViewModelBase GetRegisterViewModelCommands(UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return element.GetValue(RegisterViewModelCommandsProperty) as ViewModelBase;
        }

        public static void SetRegisterViewModelCommands(UIElement element, ViewModelBase value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(RegisterViewModelCommandsProperty, value);
        }
    }
}
