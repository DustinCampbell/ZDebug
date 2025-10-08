using System;
using System.Windows;
using System.Windows.Controls;

namespace ZDebug.IO.Windows;

internal sealed class ZPairWindow : ZWindow
{
    private ZWindow child1;
    private ZWindow child2;

    internal ZPairWindow(ZWindowManager manager, ZWindow child1, ZWindow child2, ZWindowPosition child2Position, GridLength child2Size)
        : base(manager)
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
                    var col1 = new ColumnDefinition
                    {
                        Width = child2Size
                    };
                    var col2 = new ColumnDefinition();
                    ColumnDefinitions.Add(col1);
                    ColumnDefinitions.Add(col2);
                    Grid.SetColumn(child2, 0);
                    Grid.SetColumn(child1, 1);
                    break;
                }

            case ZWindowPosition.Right:
                {
                    var col1 = new ColumnDefinition();
                    var col2 = new ColumnDefinition
                    {
                        Width = child2Size
                    };
                    ColumnDefinitions.Add(col1);
                    ColumnDefinitions.Add(col2);
                    Grid.SetColumn(child1, 0);
                    Grid.SetColumn(child2, 1);
                    break;
                }

            case ZWindowPosition.Above:
                {
                    var row1 = new RowDefinition
                    {
                        Height = child2Size
                    };
                    var row2 = new RowDefinition();
                    RowDefinitions.Add(row1);
                    RowDefinitions.Add(row2);
                    Grid.SetRow(child2, 0);
                    Grid.SetRow(child1, 1);
                    break;
                }

            case ZWindowPosition.Below:
                {
                    var row1 = new RowDefinition();
                    var row2 = new RowDefinition
                    {
                        Height = child2Size
                    };
                    RowDefinitions.Add(row1);
                    RowDefinitions.Add(row2);
                    Grid.SetRow(child1, 0);
                    Grid.SetRow(child2, 1);
                    break;
                }

            default:
                throw new ArgumentException("Invalid ZWindowPosition: " + child2Position, "child2Position");
        }

        child1.SetWindowParent(this);
        child2.SetWindowParent(this);

        Children.Add(child1);
        Children.Add(child2);
    }

    public void Replace(ZWindow child, ZWindow newChild)
    {
        if (child1 == child)
        {
            child1 = newChild;
            Children[0] = newChild;
            newChild.SetWindowParent(this);
        }
        else if (child2 == child)
        {
            child2 = newChild;
            Children[1] = newChild;
            newChild.SetWindowParent(this);
        }
    }

    public ZWindow Child1 => child1;

    public ZWindow Child2 => child2;

    public override void Clear()
    {
    }

    public override void PutString(string text)
    {
    }

    public override void PutChar(char ch)
    {
    }

    public override int RowHeight => 0;

    public override int ColumnWidth => 0;

    public override ZWindowType WindowType => ZWindowType.Pair;
}
