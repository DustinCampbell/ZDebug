using System.Windows.Controls;
using ZDebug.Core.Execution;

namespace ZDebug.UI.Controls
{
    public class CallToolTip : TextBlock
    {
        public CallToolTip(StackFrame frame)
        {
            this.Text = "Execution will return here when " + frame.Address.ToString("x4") + " returns.";
        }
    }
}
