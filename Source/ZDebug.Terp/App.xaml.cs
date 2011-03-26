using System.ComponentModel.Composition.Hosting;
using System.Windows;
using ZDebug.Terp.ViewModel;
using ZDebug.UI.Services;

namespace ZDebug.Terp
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
