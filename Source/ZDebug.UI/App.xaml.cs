using System.Windows;
using ZDebug.UI.ViewModel;

namespace ZDebug.UI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            this.MainWindow = View.Create<MainWindowViewModel, Window>();
            this.MainWindow.Show();
        }
    }
}
