using System.Composition;
using System.Windows.Controls;

namespace ZDebug.UI.ViewModel
{
    [Export, Shared]
    internal sealed class StoryInfoViewModel : ViewModelWithViewBase<UserControl>
    {
        public StoryInfoViewModel()
            : base("StoryInfoView")
        {
        }
    }
}
