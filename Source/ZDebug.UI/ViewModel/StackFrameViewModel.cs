using System.Linq;
using ZDebug.Core.Execution;
using ZDebug.Core.Utilities;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    internal sealed class StackFrameViewModel : ViewModelBase
    {
        private readonly StackFrame stackFrame;

        public StackFrameViewModel(StackFrame stackFrame)
        {
            this.stackFrame = stackFrame;
        }

        public string Name
        {
            get { return DebuggerService.RoutineTable.GetByAddress((int)stackFrame.CallAddress).Name; }
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
