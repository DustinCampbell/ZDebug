using System.IO;
using System.Windows;
using Microsoft.VisualStudio.Profiler;
using Microsoft.Win32;
using ZDebug.Core;
using ZDebug.Core.Blorb;

namespace ZDebug.PerfHarness
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private byte[] ReadStoryBytes(string path)
        {
            if (Path.GetExtension(path) == ".zblorb")
            {
                using (var stream = File.OpenRead(path))
                {
                    var blorb = new BlorbFile(stream);
                    return blorb.GetZCode();
                }
            }
            else
            {
                return File.ReadAllBytes(path);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Open Story File",
                Filter = "Z-Code Files (*.z3,*.z4,*.z5,*.z6,*.z7,*.z8)|*.z3;*.z4;*.z5;*.z6;*.z7;*.z8|Blorb Files (*.zblorb)|*.zblorb|All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            DataCollection.CommentMarkProfile(1, "Reading " + Path.GetFileName(dialog.FileName) + " bytes");

            var bytes = ReadStoryBytes(dialog.FileName);

            DataCollection.CommentMarkProfile(2, "Creating story");

            var story = Story.FromBytes(bytes);
        }
    }
}
