using System;
using System.Windows;
using System.Windows.Controls;

namespace ZDebug.UI.Windows
{
    internal sealed class ZPairWindow : ZWindow
    {
        private ZWindow child1;
        private ZWindow child2;

        public ZPairWindow(ZWindow child1, ZWindow child2, ZWindowPosition child2Position, GridLength child2Size)
        {
            if (child1 == null)
            {
                throw new ArgumentNullException("child1");
            }

            if (child2 == null)
            {
                throw new ArgumentNullException("child2");
            }

            this.child1 = child1;
            this.child2 = child2;

            switch (child2Position)
            {
                case ZWindowPosition.Left:
                    {
                        var col1 = new ColumnDefinition();
                        col1.Width = child2Size;
                        var col2 = new ColumnDefinition();
                        this.ColumnDefinitions.Add(col1);
                        this.ColumnDefinitions.Add(col2);
                        Grid.SetColumn(child2, 0);
                        Grid.SetColumn(child1, 1);
                        break;
                    }

                case ZWindowPosition.Right:
                    {
                        var col1 = new ColumnDefinition();
                        var col2 = new ColumnDefinition();
                        col2.Width = child2Size;
                        this.ColumnDefinitions.Add(col1);
                        this.ColumnDefinitions.Add(col2);
                        Grid.SetColumn(child1, 0);
                        Grid.SetColumn(child2, 1);
                        break;
                    }

                case ZWindowPosition.Above:
                    {
                        var row1 = new RowDefinition();
                        row1.Height = child2Size;
                        var row2 = new RowDefinition();
                        this.RowDefinitions.Add(row1);
                        this.RowDefinitions.Add(row2);
                        Grid.SetRow(child2, 0);
                        Grid.SetRow(child1, 1);
                        break;
                    }

                case ZWindowPosition.Below:
                    {
                        var row1 = new RowDefinition();
                        var row2 = new RowDefinition();
                        row2.Height = child2Size;
                        this.RowDefinitions.Add(row1);
                        this.RowDefinitions.Add(row2);
                        Grid.SetRow(child1, 0);
                        Grid.SetRow(child2, 1);
                        break;
                    }

                default:
                    throw new ArgumentException("Invalid ZWindowPosition: " + child2Position, "child2Position");
            }

            this.Children.Add(child1);
            this.Children.Add(child2);
        }

        public void Replace(ZWindow child, ZWindow newChild)
        {
            if (child1 == child)
            {
                child1 = newChild;
                this.Children[0] = newChild;
                newChild.SetWindowParent(this);
            }
            else if (child2 == child)
            {
                child2 = newChild;
                this.Children[1] = newChild;
                newChild.SetWindowParent(this);
            }
        }

        public ZWindow Child1
        {
            get { return child1; }
        }

        public ZWindow Child2
        {
            get { return child2; }
        }

        public override void Clear()
        {
        }

        public override int RowHeight
        {
            get { return 0; }
        }

        public override int ColumnWidth
        {
            get { return 0; }
        }

        public override ZWindowType WindowType
        {
            get { return ZWindowType.Pair; }
        }
    }
}
