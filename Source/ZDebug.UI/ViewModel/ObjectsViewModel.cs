using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Input;
using ZDebug.UI.Collections;
using ZDebug.UI.Extensions;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    [Export]
    internal sealed class ObjectsViewModel : ViewModelWithViewBase<UserControl>
    {
        private readonly StoryService storyService;
        private readonly BulkObservableCollection<ObjectViewModel> objects;

        [ImportingConstructor]
        private ObjectsViewModel(
            StoryService storyService)
            : base("ObjectsView")
        {
            this.storyService = storyService;
            this.storyService.StoryOpened += StoryService_StoryOpened;
            this.storyService.StoryClosing += StoryService_StoryClosing;

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

        private void StoryService_StoryOpened(object sender, StoryOpenedEventArgs e)
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

        private void StoryService_StoryClosing(object sender, StoryClosingEventArgs e)
        {
            objects.Clear();

            PropertyChanged("HasStory");
        }

        public ICommand NavigateCommand { get; private set; }

        public bool HasStory
        {
            get { return storyService.IsStoryOpen; }
        }

        public BulkObservableCollection<ObjectViewModel> Objects
        {
            get { return objects; }
        }
    }
}
