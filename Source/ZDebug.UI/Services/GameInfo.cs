using System.Xml.Linq;

namespace ZDebug.UI.Services
{
    internal sealed class GameInfo
    {
        private readonly string title;
        private readonly string headline;
        private readonly string author;
        private readonly string firstPublished;
        private readonly string description;

        public GameInfo(XElement metadata)
        {
            XNamespace xmlns = "http://babel.ifarchive.org/protocol/iFiction/";
            var storyElement = metadata.Element(xmlns + "story");
            var biblioElement = storyElement.Element(xmlns + "bibliographic");
            var titleElement = biblioElement.Element(xmlns + "title");
            var headlineElement = biblioElement.Element(xmlns + "headline");
            var authorElement = biblioElement.Element(xmlns + "author");
            var firstPublishedElement = biblioElement.Element(xmlns + "firstpublished");
            var descriptionElement = biblioElement.Element(xmlns + "description");
            title = titleElement != null ? titleElement.Value : string.Empty;
            headline = headlineElement != null ? headlineElement.Value : string.Empty;
            author = authorElement != null ? authorElement.Value : string.Empty;
            firstPublished = firstPublishedElement != null ? firstPublishedElement.Value : string.Empty;
            description = descriptionElement != null ? descriptionElement.Value : string.Empty;
        }

        public string Title
        {
            get { return title; }
        }

        public string Headline
        {
            get { return headline; }
        }

        public string Author
        {
            get { return author; }
        }

        public string FirstPublished
        {
            get { return firstPublished; }
        }

        public string Description
        {
            get { return description; }
        }
    }
}
