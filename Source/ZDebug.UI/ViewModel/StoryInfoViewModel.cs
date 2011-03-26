using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace ZDebug.UI.ViewModel
{
    [Export]
    internal sealed class StoryInfoViewModel : ViewModelWithViewBase<UserControl>
    {
        public StoryInfoViewModel()
            : base("StoryInfoView")
        {
        }
    }
}
