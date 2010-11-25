using System.Windows.Controls;
using System.Windows.Input;
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
            this.NavigateCommand = RegisterCommand<int>(
                text: "Navigate",
                name: "Navigate",
                executed: NavigateExecuted,
                canExecute: CanNavigateExecute);

            objects = new BulkObservableCollection<ObjectViewModel>();
        }

        private bool CanNavigateExecute(int number)
        {
            return number > 0;
        }

        private void NavigateExecuted(int number)
        {
            var listObjects = this.View.FindName<ListBox>("listObjects");
            listObjects.SelectedIndex = number - 1; // object indeces are 1-based
            listObjects.ScrollIntoView(listObjects.SelectedItem);
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

        public ICommand NavigateCommand { get; private set; }

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
