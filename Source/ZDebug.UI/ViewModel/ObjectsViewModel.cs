using System.Windows.Controls;
using ZDebug.UI.Services;
using ZDebug.UI.Utilities;

namespace ZDebug.UI.ViewModel
{
    internal sealed class ObjectsViewModel : ViewModelWithViewBase<UserControl>
    {
        private readonly BulkObservableCollection<ObjectViewModel> objects;

        public ObjectsViewModel()
            : base("ObjectsView")
        {
            objects = new BulkObservableCollection<ObjectViewModel>();
        }

        private void DebuggerService_StoryOpened(object sender, StoryEventArgs e)
        {
            objects.BeginBulkOperation();
            try
            {
                foreach (var obj in e.Story.ObjectTable)
                {
                    objects.Add(new ObjectViewModel(obj));
                }
            }
            finally
            {
                objects.EndBulkOperation();
            }

            PropertyChanged("HasStory");
        }

        private void DebuggerService_StoryClosed(object sender, StoryEventArgs e)
        {
            objects.Clear();

            PropertyChanged("HasStory");
        }

        protected internal override void Initialize()
        {
            DebuggerService.StoryOpened += DebuggerService_StoryOpened;
            DebuggerService.StoryClosed += DebuggerService_StoryClosed;
        }

        public bool HasStory
        {
            get { return DebuggerService.HasStory; }
        }

        public BulkObservableCollection<ObjectViewModel> Objects
        {
            get { return objects; }
        }

    }
}
