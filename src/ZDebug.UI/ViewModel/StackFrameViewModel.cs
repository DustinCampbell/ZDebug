using System.Linq;
using ZDebug.Core.Execution;
using ZDebug.Core.Extensions;
using ZDebug.Core.Routines;

namespace ZDebug.UI.ViewModel
{
    internal sealed class StackFrameViewModel : ViewModelBase
    {
        private readonly StackFrame stackFrame;
        private readonly ZRoutineTable routineTable;

        public StackFrameViewModel(StackFrame stackFrame, ZRoutineTable routineTable)
        {
            this.stackFrame = stackFrame;
            this.routineTable = routineTable;
        }

        public string Name
        {
            get { return routineTable.GetByAddress((int)stackFrame.CallAddress).Name; }
        }

        public bool HasName
        {
            get { return Name.Length > 0; }
        }

        public uint CallAddress
        {
            get { return stackFrame.CallAddress; }
        }

        public string ArgText
        {
            get
            {
                return "(" + string.Join(", ", stackFrame.Arguments.ToArray().ConvertAll(arg => arg.ToString("x4"))) + ")";
            }
        }
    }
}
