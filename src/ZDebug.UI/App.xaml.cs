using System.Composition.Hosting;
using System.Windows;
using ZDebug.UI.Services;
using ZDebug.UI.ViewModel;

namespace ZDebug.UI;

public partial class App : Application
{
    private CompositionHost compositionHost;

    public T GetService<T>()
        where T : IService => compositionHost.GetExport<T>();

    protected override void OnStartup(StartupEventArgs e)
    {
        var configuration = new ContainerConfiguration()
            .WithAssembly(typeof(App).Assembly)
            .WithAssembly(typeof(StoryService).Assembly);

        compositionHost = configuration.CreateContainer();

        // retrieve StorageService to allow it to be connected properly.
        compositionHost.GetExport<StorageService>();

        var mainWindowViewModel = compositionHost.GetExport<MainWindowViewModel>();

        MainWindow = mainWindowViewModel.CreateView();
        MainWindow.Show();
    }

    public static new App Current => (App)Application.Current;
}
