using System.Composition.Hosting;
using System.Windows;
using ZDebug.Terp.ViewModel;
using ZDebug.UI.Services;

namespace ZDebug.Terp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var configuration = new ContainerConfiguration()
                .WithAssembly(typeof(App).Assembly)
                .WithAssembly(typeof(StoryService).Assembly);

            var compositionHost = configuration.CreateContainer();

            // retrieve StorageService to allow it to be connected properly.
            compositionHost.GetExport<StorageService>();

            var mainWindowViewModel = compositionHost.GetExport<MainWindowViewModel>();

            this.MainWindow = mainWindowViewModel.CreateView();
            this.MainWindow.Show();
        }
    }
}
