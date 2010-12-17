using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ZDebug.UI.ViewModel
{
    internal static class DialogBehaviors
    {
        private static void OnClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            var parent = VisualTreeHelper.GetParent(button);
            while (parent != null && !(parent is Window))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            if (parent != null)
            {
                ((Window)parent).DialogResult = true;
            }
        }

        private static void IsAcceptChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var button = (Button)obj;
            var enabled = (bool)e.NewValue;

            if (button != null)
            {
                if (enabled)
                {
                    button.Click += OnClick;
                }
                else
                {
                    button.Click -= OnClick;
                }
            }
        }

        public static readonly DependencyProperty IsAcceptProperty =
            DependencyProperty.RegisterAttached(
                name: "IsAccept",
                propertyType: typeof(bool),
                ownerType: typeof(Button),
                defaultMetadata: new UIPropertyMetadata(
                    defaultValue: false,
                    propertyChangedCallback: IsAcceptChanged));

        public static bool GetIsAccept(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsAcceptProperty);
        }

        public static void SetIsAccept(DependencyObject obj, bool value)
        {
            obj.SetValue(IsAcceptProperty, value);
        }
    }
}
