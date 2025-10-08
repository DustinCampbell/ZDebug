#nullable enable

using AvalonDock;
using AvalonDock.Layout.Serialization;
using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using ZDebug.Core;

namespace ZDebug.UI.Utilities;

public static partial class Storage
{
    private const string DockLayoutFileName = "dock_layout.xml";
    private const string WindowLayoutFileName = "window_layout.xml";

    private static string GetDockLayoutFileName(string? prefix)
        => prefix != null
            ? $"{prefix}_{DockLayoutFileName}"
            : DockLayoutFileName;

    private static string GetWindowLayoutFileName(Window window)
        => $"{window.Name}_{WindowLayoutFileName}";

    private static string GetStorySettingsFileName(Story story)
        => $"{story.SerialNumber:d6}_{story.ReleaseNumber}_{story.Version}_{story.Checksum:x4}";

    private static IsolatedStorageFile GetStorageFile()
        => IsolatedStorageFile.GetUserStoreForDomain();

    private static void RestoreFromXmlFile(string fileName, Action<XmlTextReader> readAction)
    {
        var storage = GetStorageFile();
        if (!storage.FileExists(fileName))
        {
            return;
        }

        using var fileStream = storage.OpenFile(fileName, FileMode.Open, FileAccess.Read);

        var reader = new XmlTextReader(fileStream)
        {
            WhitespaceHandling = WhitespaceHandling.None
        };

        readAction(reader);
    }

    private static void SaveToXmlFile(string fileName, Action<XmlTextWriter> writeAction)
    {
        var storage = GetStorageFile();
        using var fileStream = storage.CreateFile(fileName);

        var writer = new XmlTextWriter(fileStream, Encoding.UTF8)
        {
            Formatting = Formatting.Indented
        };

        writeAction(writer);
        writer.Flush();
    }

    public static void RestoreDockingLayout(DockingManager dockManager, string? prefix = null)
    {
        var fileName = GetDockLayoutFileName(prefix);

        RestoreFromXmlFile(fileName, reader =>
        {
            var serializer = new XmlLayoutSerializer(dockManager);
            serializer.Deserialize(reader);
        });
    }

    public static void SaveDockingLayout(DockingManager dockManager, string? prefix = null)
    {
        var fileName = GetDockLayoutFileName(prefix);

        SaveToXmlFile(fileName, writer =>
        {
            var serializer = new XmlLayoutSerializer(dockManager);
            serializer.Serialize(writer);
        });
    }

    public static void RestoreWindowLayout(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentException.ThrowIfNullOrWhiteSpace(window.Name);

        var fileName = GetWindowLayoutFileName(window);

        RestoreFromXmlFile(fileName, reader =>
        {
            reader.MoveToContent();
            var xml = XElement.Load(reader);

            WindowPlacement.Restore(window, xml);
        });
    }

    public static void SaveWindowLayout(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentException.ThrowIfNullOrWhiteSpace(window.Name);

        var fileName = GetWindowLayoutFileName(window);

        SaveToXmlFile(fileName, writer =>
        {
            var xml = WindowPlacement.Save(window);
            xml.Save(writer);
        });
    }

    public static XElement RestoreStorySettings(Story story)
    {
        var fileName = GetStorySettingsFileName(story);

        XElement? xml = null;

        RestoreFromXmlFile(fileName, reader =>
        {
            reader.MoveToContent();
            xml = XElement.Load(reader);
        });

        return xml ?? new XElement("settings");
    }

    public static void SaveStorySettings(Story story, XElement xml)
    {
        var fileName = GetStorySettingsFileName(story);

        SaveToXmlFile(fileName, xml.Save);
    }
}
