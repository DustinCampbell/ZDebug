using System;
using System.Text;
using ZDebug.Core.Execution;

namespace ZDebug.Compiler.Tests.Mocks;

internal sealed class MockScreen : IScreen
{
    private readonly Action doneAction;
    private readonly string[] scriptCommands;
    private int scriptCommandIndex;
    private StringBuilder output;

    private int activeWindow;

    public MockScreen(string[] scriptCommands, Action doneAction)
    {
        this.doneAction = doneAction;
        this.scriptCommands = scriptCommands;
        this.scriptCommandIndex = 0;
        this.output = new StringBuilder();
    }

    public void Clear(int window)
    {
    }

    public void ClearAll(bool unsplit = false)
    {
    }

    public void Split(int height)
    {
    }

    public void Unsplit()
    {
    }

    public void SetWindow(int window)
    {
        activeWindow = window;
    }

    public int GetCursorLine()
    {
        return 0;
    }

    public int GetCursorColumn()
    {
        return 0;
    }

    public void SetCursor(int line, int column)
    {
    }

    public void SetTextStyle(ZTextStyle style)
    {
    }

    public void SetForegroundColor(ZColor color)
    {
    }

    public void SetBackgroundColor(ZColor color)
    {
    }

    public ZFont SetFont(ZFont font)
    {
        return 0;
    }

    public void ShowStatus()
    {
    }

    public byte ScreenHeightInLines => 36;

    public byte ScreenWidthInColumns => 110;

    public ushort ScreenHeightInUnits => 681;

    public ushort ScreenWidthInUnits => 972;

    public byte FontHeightInUnits => 18;

    public byte FontWidthInUnits => 8;

    public bool SupportsColors => false;

    public bool SupportsBold => false;

    public bool SupportsItalic => false;

    public bool SupportsFixedFont => false;

    public ZColor DefaultBackgroundColor => ZColor.White;

    public ZColor DefaultForegroundColor => ZColor.Black;

    public void Print(string text)
    {
        if (activeWindow == 0)
        {
            output.Append(text);
        }
    }

    public void Print(char ch)
    {
        if (activeWindow == 0)
        {
            output.Append(ch);
        }
    }

    public void ReadChar(Action<char> callback)
    {
        doneAction();
    }

    public void ReadCommand(int maxChars, Action<string> callback)
    {
        if (scriptCommands == null || scriptCommandIndex == scriptCommands.Length)
        {
            doneAction();
        }
        else
        {
            var command = scriptCommands[scriptCommandIndex++];
            output.AppendLine(command);
            callback(command);
        }
    }

    public string Output => output.ToString().Replace("\r\n", "\n");
}
