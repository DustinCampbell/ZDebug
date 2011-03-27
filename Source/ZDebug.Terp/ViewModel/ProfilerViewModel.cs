using System.ComponentModel.Composition;
using System.Windows.Controls;
using ZDebug.UI.ViewModel;

namespace ZDebug.Terp.ViewModel
{
    [Export]
    internal sealed class ProfilerViewModel : ViewModelWithViewBase<UserControl>
    {
        private ProfilerViewModel()
            : base("ProfilerView")
        {
        }
    }
}
