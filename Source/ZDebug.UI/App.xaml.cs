using System.Windows;
using ZDebug.UI.ViewModel;

namespace ZDebug.UI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            this.MainWindow = ViewModelWithView<MainWindowViewModel, Window>.Create();
            this.MainWindow.Show();
        }
    }
}
