using System.ComponentModel.Composition.Hosting;
using System.Windows;
using ZDebug.UI.Services;
using ZDebug.UI.ViewModel;

namespace ZDebug.UI
{
    public partial class App : Application
    {
        private CompositionContainer container;

        public T GetService<T>()
            where T : IService
        {
            return container.GetExportedValue<T>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var catelog = new AggregateCatalog(
                new AssemblyCatalog(typeof(App).Assembly),
                new AssemblyCatalog(typeof(StoryService).Assembly));

            this.container = new CompositionContainer(catelog);

            var mainWindowViewModel = container.GetExportedValue<MainWindowViewModel>();

            this.MainWindow = mainWindowViewModel.CreateView();
            this.MainWindow.Show();
        }

        public new static App Current
        {
            get
            {
                return ((App)Application.Current);
            }
        }
    }
}
