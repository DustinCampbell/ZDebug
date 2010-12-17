using System.Xml.Linq;

namespace ZDebug.UI.Services
{
    internal sealed class GameInfo
    {
        private readonly string title;

        public GameInfo(XElement metadata)
        {
            XNamespace xmlns = "http://babel.ifarchive.org/protocol/iFiction/";
            var storyElement = metadata.Element(xmlns + "story");
            var biblioElement = storyElement.Element(xmlns + "bibliographic");
            var titleElement = biblioElement.Element(xmlns + "title");
            title = titleElement.Value;
        }

        public string Title
        {
            get { return title; }
        }
    }
}
