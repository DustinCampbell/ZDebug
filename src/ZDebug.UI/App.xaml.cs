using System.Composition.Hosting;
using System.Windows;
using ZDebug.UI.Services;
using ZDebug.UI.ViewModel;

namespace ZDebug.UI
{
    public partial class App : Application
    {
        private CompositionHost compositionHost;

        public T GetService<T>()
            where T : IService
        {
            return this.compositionHost.GetExport<T>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var configuration = new ContainerConfiguration()
                .WithAssembly(typeof(App).Assembly)
                .WithAssembly(typeof(StoryService).Assembly);

            this.compositionHost = configuration.CreateContainer();

            // retrieve StorageService to allow it to be connected properly.
            this.compositionHost.GetExport<StorageService>();

            var mainWindowViewModel = this.compositionHost.GetExport<MainWindowViewModel>();

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
