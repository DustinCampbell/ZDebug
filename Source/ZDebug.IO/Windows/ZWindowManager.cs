using System;
using System.Windows;

namespace ZDebug.IO.Windows
{
    public sealed class ZWindowManager
    {
        private ZWindow root;

        private ZWindow CreateNewWindow(ZWindowType windowType)
        {
            switch (windowType)
            {
                case ZWindowType.Blank:
                    return new ZBlankWindow(this);

                case ZWindowType.TextBuffer:
                    return new ZTextBufferWindow(this);

                case ZWindowType.TextGrid:
                    return new ZTextGridWindow(this);

                default:
                    throw new ArgumentException("Invalid ZWindowType: " + windowType, "windowType");
            }
        }

        public ZWindow Open(
            ZWindowType windowType,
            ZWindow split = null,
            ZWindowPosition position = 0,
            ZWindowSizeType sizeType = 0,
            int size = 0)
        {
            if (windowType == ZWindowType.Pair)
            {
                throw new InvalidOperationException("ZWindows of type Pair cannot be created directly.");
            }

            if (root == null)
            {
                if (split != null)
                {
                    throw new ArgumentException("'split' must be null if Root has not yet been created.", "split");
                }

                root = CreateNewWindow(windowType);
                return root;
            }
            else
            {
                if (split == null)
                {
                    throw new ArgumentNullException("split", "'split' cannot be null if the Root has already been created.");
                }

                var newWindow = CreateNewWindow(windowType);

                var parent = split.WindowParent;

                GridLength splitSize;
                switch (sizeType)
                {
                    case ZWindowSizeType.Fixed:
                        var pixels = position == ZWindowPosition.Above || position == ZWindowPosition.Below
                            ? size * newWindow.RowHeight
                            : size * newWindow.ColumnWidth;
                        splitSize = new GridLength(pixels, GridUnitType.Pixel);
                        break;

                    case ZWindowSizeType.Proportional:
                        splitSize = new GridLength((double)size / 100, GridUnitType.Star);
                        break;

                    default:
                        throw new ArgumentException("Invalid ZWindowSizeType: " + sizeType, "sizeType");
                }

                var newPair = new ZPairWindow(this, split, newWindow, position, splitSize);

                if (parent != null)
                {
                    parent.Replace(split, newPair);
                }
                else
                {
                    root = newPair;
                }

                return newWindow;
            }
        }

        public ZWindow Root
        {
            get { return root; }
        }
    }
}
