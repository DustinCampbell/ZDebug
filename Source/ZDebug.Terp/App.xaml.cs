using System.Windows;
using ZDebug.Terp.ViewModel;
using ZDebug.UI.ViewModel;

namespace ZDebug.Terp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            this.MainWindow = ViewModelWithView<MainWindowViewModel, Window>.Create();
            this.MainWindow.Show();
        }
    }
}
