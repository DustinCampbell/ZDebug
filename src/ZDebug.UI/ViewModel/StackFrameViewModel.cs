using System.Linq;
using ZDebug.Core.Execution;
using ZDebug.Core.Extensions;
using ZDebug.Core.Routines;

namespace ZDebug.UI.ViewModel;

internal sealed class StackFrameViewModel : ViewModelBase
{
    private readonly StackFrame stackFrame;
    private readonly ZRoutineTable routineTable;

    public StackFrameViewModel(StackFrame stackFrame, ZRoutineTable routineTable)
    {
        this.stackFrame = stackFrame;
        this.routineTable = routineTable;
    }

    public string Name => routineTable.GetByAddress((int)stackFrame.CallAddress).Name;

    public bool HasName => Name.Length > 0;

    public uint CallAddress => stackFrame.CallAddress;

    public string ArgText => "(" + string.Join(", ", stackFrame.Arguments.ToArray().ConvertAll(arg => arg.ToString("x4"))) + ")";
}
