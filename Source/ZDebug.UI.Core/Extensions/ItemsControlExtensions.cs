using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using ZDebug.Core.Utilities;

namespace ZDebug.UI.Extensions
{
    public static class ItemsControlExtensions
    {
        // TODO: Clean up reflection hacks!

        private static MethodInfo itemsHostMethodInfo;
        private static MethodInfo bringIndexIntoView;

        private static Panel ItemsHost(this ItemsControl itemsControl)
        {
            if (itemsHostMethodInfo == null)
            {
                itemsHostMethodInfo = Reflection<ItemsControl>.GetMethod("get_ItemsHost", @public: false);
            }

            return (Panel)itemsHostMethodInfo.Invoke(itemsControl, new object[0]);
        }

        private static void BringIndexIntoView(this VirtualizingPanel virtualizingPanel, int index)
        {
            if (bringIndexIntoView == null)
            {
                bringIndexIntoView = Reflection<VirtualizingPanel>.GetMethod("BringIndexIntoView", @public: false);
            }

            bringIndexIntoView.Invoke(virtualizingPanel, new object[] { index });
        }

        private static DependencyObject FirstVisualChild(Visual visual)
        {
            if (visual == null)
            {
                return null;
            }

            if (VisualTreeHelper.GetChildrenCount(visual) == 0)
            {
                return null;
            }

            return VisualTreeHelper.GetChild(visual, 0);
        }

        private static double CenteringOffset(double center, double viewPoint, double extent)
        {
            return Math.Min(extent - viewPoint, Math.Max(0, center - (viewPoint / 2)));
        }

        private static bool TryScrollToCenterOfView(this ItemsControl itemsControl, int index)
        {
            var container = itemsControl.ItemContainerGenerator.ContainerFromIndex(index) as UIElement;
            if (container == null)
            {
                return false;
            }

            ScrollContentPresenter presenter = null;
            for (Visual vis = container; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
            {
                if ((presenter = vis as ScrollContentPresenter) != null)
                {
                    break;
                }
            }

            if (presenter == null)
            {
                return false;
            }

            var scrollInfo = !presenter.CanContentScroll
                ? presenter
                : presenter.Content as IScrollInfo ?? FirstVisualChild(presenter.Content as ItemsPresenter) as IScrollInfo ?? presenter;

            if (scrollInfo.ViewportHeight == 0.0 || scrollInfo.ViewportWidth == 0.0)
            {
                return false;
            }


            Point center;
            if (scrollInfo is StackPanel || scrollInfo is VirtualizingStackPanel)
            {
                var logicalCenter = itemsControl.ItemContainerGenerator.IndexFromContainer(container) + 0.5;
                var orientation = scrollInfo is StackPanel ? ((StackPanel)scrollInfo).Orientation : ((VirtualizingStackPanel)scrollInfo).Orientation;
                if (orientation == Orientation.Horizontal)
                {
                    center = new Point(logicalCenter, 0.0);
                }
                else
                {
                    center = new Point(0.0, logicalCenter);
                }
            }
            else
            {
                var size = container.RenderSize;
                center = container.TransformToAncestor((Visual)scrollInfo).Transform(new Point(size.Width / 2, size.Height / 2));
                center.Y += scrollInfo.VerticalOffset;
                center.X += scrollInfo.HorizontalOffset;
            }

            if (scrollInfo.CanVerticallyScroll)
            {
                scrollInfo.SetVerticalOffset(CenteringOffset(center.Y, scrollInfo.ViewportHeight, scrollInfo.ExtentHeight));
            }

            if (scrollInfo.CanHorizontallyScroll)
            {
                scrollInfo.SetHorizontalOffset(CenteringOffset(center.X, scrollInfo.ViewportWidth, scrollInfo.ExtentWidth));
            }

            return true;
        }

        public static void BringIntoView(this ItemsControl itemsControl, object item)
        {
            var element = itemsControl.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
            if (element != null)
            {
                element.BringIntoView();
            }
            else if (!itemsControl.IsGrouping)
            {
                var index = itemsControl.Items.IndexOf(item);
                if (index >= 0)
                {
                    var itemsHost = itemsControl.ItemsHost() as VirtualizingPanel;
                    if (itemsHost != null)
                    {
                        itemsHost.BringIndexIntoView(index);

                        if (!itemsControl.TryScrollToCenterOfView(index))
                        {
                            itemsControl.Dispatcher.BeginInvoke(
                                new Action(() => itemsControl.TryScrollToCenterOfView(index)),
                                DispatcherPriority.Loaded);
                        }
                    }
                }
            }
        }
    }
}
