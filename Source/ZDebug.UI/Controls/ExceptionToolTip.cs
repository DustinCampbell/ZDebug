using System;
using System.Windows;
using System.Windows.Controls;

namespace ZDebug.UI.Controls
{
    public class ExceptionToolTip : Grid
    {
        public ExceptionToolTip(Exception ex)
        {
            var titleRow = new RowDefinition() { Height = GridLength.Auto };
            var messageRow = new RowDefinition() { Height = new GridLength(1.0, GridUnitType.Star) };

            this.RowDefinitions.Add(titleRow);
            this.RowDefinitions.Add(messageRow);

            var titleBlock = new TextBlock()
            {
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 4),
                Text = ex.GetType().FullName
            };

            var messageBlock = new TextBlock() { Text = ex.Message };

            Grid.SetRow(titleBlock, 0);
            Grid.SetRow(messageBlock, 1);

            this.Children.Add(titleBlock);
            this.Children.Add(messageBlock);
        }

    }
}
