using System.Xml.Linq;
using ZDebug.Core;
using ZDebug.UI.Utilities;

namespace ZDebug.Debugger.Utilities
{
    internal static class GameStorage
    {
        private static string GetStorySettingsFileName(Story story)
        {
            return string.Format("{0:d6}_{1}_{2}_{3:x4}", story.SerialNumber, story.ReleaseNumber, story.Version, story.Checksum);
        }

        public static XElement RestoreStorySettings(Story story)
        {
            var fileName = GetStorySettingsFileName(story);
            using (var reader = Storage.OpenXmlFile(fileName))
            {
                if (reader != null)
                {
                    reader.MoveToContent();
                    return XElement.Load(reader);
                }
                else
                {
                    return new XElement("settings");
                }
            }
        }

        public static void SaveStorySettings(Story story, XElement xml)
        {
            var fileName = GetStorySettingsFileName(story);
            using (var writer = Storage.CreateXmlFile(fileName))
            {
                xml.Save(writer);
            }
        }
    }
}
