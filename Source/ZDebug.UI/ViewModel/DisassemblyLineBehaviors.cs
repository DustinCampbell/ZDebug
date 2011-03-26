using System.Windows;
using System.Windows.Input;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    internal static class DisassemblyLineBehaviors
    {
        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = (FrameworkElement)sender;
            if (element != null)
            {
                element.CaptureMouse();
            }
        }

        private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var element = (FrameworkElement)sender;
            if (element != null && element.IsMouseCaptured)
            {
                element.ReleaseMouseCapture();

                if (element.IsMouseDirectlyOver)
                {
                    var line = element.DataContext as DisassemblyInstructionLineViewModel;
                    if (line != null)
                    {
                        var breakpointService = App.Current.GetService<BreakpointService>();
                        breakpointService.Toggle(line.Address);
                    }
                }
            }
        }

        private static void OnTogglesBreakpointOnClickChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var element = (FrameworkElement)obj;
            var enabled = (bool)e.NewValue;

            if (element != null)
            {
                if (enabled)
                {
                    element.MouseLeftButtonDown += OnMouseLeftButtonDown;
                    element.MouseLeftButtonUp += OnMouseLeftButtonUp;
                }
                else
                {
                    element.MouseLeftButtonDown -= OnMouseLeftButtonDown;
                    element.MouseLeftButtonUp -= OnMouseLeftButtonUp;
                }
            }
        }

        public static readonly DependencyProperty TogglesBreakpointOnClickProperty =
            DependencyProperty.RegisterAttached(
                name: "TogglesBreakpointOnClick",
                propertyType: typeof(bool),
                ownerType: typeof(DisassemblyLineBehaviors),
                defaultMetadata: new UIPropertyMetadata(
                    defaultValue: false,
                    propertyChangedCallback: OnTogglesBreakpointOnClickChanged));

        public static bool GetTogglesBreakpointOnClick(DependencyObject obj)
        {
            return (bool)obj.GetValue(TogglesBreakpointOnClickProperty);
        }

        public static void SetTogglesBreakpointOnClick(DependencyObject obj, bool value)
        {
            obj.SetValue(TogglesBreakpointOnClickProperty, value);
        }
    }
}
