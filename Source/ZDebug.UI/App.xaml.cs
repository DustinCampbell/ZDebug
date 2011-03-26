using System.ComponentModel.Composition.Hosting;
using System.Windows;
using ZDebug.UI.Services;
using ZDebug.UI.ViewModel;

namespace ZDebug.UI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var catelog = new AggregateCatalog(
                new AssemblyCatalog(typeof(App).Assembly),
                new AssemblyCatalog(typeof(StoryService).Assembly));

            var container = new CompositionContainer(catelog);

            var mainWindowViewModel = container.GetExportedValue<MainWindowViewModel>();

            this.MainWindow = mainWindowViewModel.CreateView();
            this.MainWindow.Show();
        }
    }
}
