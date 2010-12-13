using System.Windows;
using System.Windows.Media;

namespace ZDebug.IO.Windows
{
    internal sealed class ZTextGrid : FrameworkElement
    {
        private readonly VisualCollection visuals;

        private int cursorX;
        private int cursorY;

        public ZTextGrid()
        {
            visuals = new VisualCollection(this);
        }

        protected override Visual GetVisualChild(int index)
        {
            return visuals[index];
        }

        protected override int VisualChildrenCount
        {
            get { return visuals.Count; }
        }
    }
}
