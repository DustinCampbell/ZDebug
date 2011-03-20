using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using AvalonDock;

namespace ZDebug.UI.Utilities
{
    public static partial class Storage
    {
        private const string DockLayoutFileName = "dock_layout.xml";
        private const string WindowLayoutFileName = "window_layout.xml";

        private static string GetDockLayoutFileName(string prefix)
        {
            return prefix != null
                ? prefix + "_" + DockLayoutFileName
                : DockLayoutFileName;
        }

        private static string GetWindowLayoutFileName(Window window)
        {
            return window.Name + "_" + WindowLayoutFileName;
        }

        private static IsolatedStorageFile GetStorageFile()
        {
            return IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
        }

        public static XmlTextReader OpenXmlFile(string fileName)
        {
            var storage = GetStorageFile();
            if (!storage.FileExists(fileName))
            {
                return null;
            }

            var fileStream = storage.OpenFile(fileName, FileMode.Open, FileAccess.Read);
            var reader = new XmlTextReader(fileStream);
            reader.WhitespaceHandling = WhitespaceHandling.None;

            return reader;
        }

        public static XmlTextWriter CreateXmlFile(string fileName)
        {
            var storage = GetStorageFile();
            var fileStream = storage.CreateFile(fileName);
            var writer = new XmlTextWriter(fileStream, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            return writer;
        }

        public static void RestoreDockingLayout(DockingManager dockManager, string prefix = null)
        {
            using (var dockLayoutReader = OpenXmlFile(GetDockLayoutFileName(prefix)))
            {
                if (dockLayoutReader != null)
                {
                    dockManager.RestoreLayout(dockLayoutReader);
                }
            }
        }

        public static void SaveDockingLayout(DockingManager dockManager, string prefix = null)
        {
            using (var dockLayoutWriter = CreateXmlFile(GetDockLayoutFileName(prefix)))
            {
                dockManager.SaveLayout(dockLayoutWriter);
            }
        }

        public static void RestoreWindowLayout(Window window)
        {
            if (window == null)
            {
                throw new ArgumentNullException("window");
            }
            if (string.IsNullOrWhiteSpace(window.Name))
            {
                throw new ArgumentException("Name is not set.", "window");
            }

            using (var windowLayoutReader = OpenXmlFile(GetWindowLayoutFileName(window)))
            {
                if (windowLayoutReader != null)
                {
                    windowLayoutReader.MoveToContent();

                    var xml = XElement.Load(windowLayoutReader);

                    WindowPlacement.Restore(window, xml);
                }
            }
        }

        public static void SaveWindowLayout(Window window)
        {
            if (window == null)
            {
                throw new ArgumentNullException("window");
            }
            if (string.IsNullOrWhiteSpace(window.Name))
            {
                throw new ArgumentException("Name is not set.", "window");
            }

            using (var windowLayoutWriter = CreateXmlFile(GetWindowLayoutFileName(window)))
            {
                var xml = WindowPlacement.Save(window);
                xml.Save(windowLayoutWriter);
            }
        }
    }
}
