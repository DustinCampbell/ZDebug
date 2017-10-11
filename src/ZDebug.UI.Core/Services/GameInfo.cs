using System;
using System.Text;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;
using ZDebug.Core.Blorb;

namespace ZDebug.UI.Services
{
    public sealed class GameInfo
    {
        private readonly string title;
        private readonly string headline;
        private readonly string author;
        private readonly string firstPublished;
        private readonly string description;

        private readonly string coverFormat;
        private readonly int coverHeight;
        private readonly int coverWidth;

        private readonly int coverId;

        private BitmapSource cover;

        public GameInfo(BlorbFile blorb)
        {
            var metadata = blorb.LoadMetadata();

            XNamespace xmlns = "http://babel.ifarchive.org/protocol/iFiction/";
            var storyElement = metadata.Element(xmlns + "story");
            var biblioElement = storyElement.Element(xmlns + "bibliographic");
            var titleElement = biblioElement.Element(xmlns + "title");
            var headlineElement = biblioElement.Element(xmlns + "headline");
            var authorElement = biblioElement.Element(xmlns + "author");
            var firstPublishedElement = biblioElement.Element(xmlns + "firstpublished");
            var descriptionElement = biblioElement.Element(xmlns + "description");

            title = titleElement != null ? (string)titleElement : string.Empty;
            headline = headlineElement != null ? (string)headlineElement : string.Empty;
            author = authorElement != null ? (string)authorElement : string.Empty;
            firstPublished = firstPublishedElement != null ? (string)firstPublishedElement : string.Empty;

            if (descriptionElement != null)
            {
                var descriptionBuilder = new StringBuilder();
                foreach (var descendent in descriptionElement.DescendantNodes())
                {
                    switch (descendent.NodeType)
                    {
                        case XmlNodeType.Text:
                            var tokens = ((XText)descendent).Value.Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                            descriptionBuilder.Append(string.Join(" ", tokens));

                            break;

                        case XmlNodeType.Element:
                            if (((XElement)descendent).Name == xmlns + "br")
                            {
                                descriptionBuilder.AppendLine();
                                descriptionBuilder.AppendLine();
                            }
                            break;
                    }
                }
                description = descriptionBuilder.ToString();
            }
            else
            {
                description = string.Empty;
            }

            var coverElement = storyElement.Element(xmlns + "cover");
            if (coverElement != null)
            {
                var coverFormatElement = coverElement.Element(xmlns + "format");
                var coverHeightElement = coverElement.Element(xmlns + "height");
                var coverWidthElement = coverElement.Element(xmlns + "width");

                coverFormat = coverFormatElement != null ? (string)coverFormatElement : string.Empty;
                coverHeight = coverHeightElement != null ? (int)coverHeightElement : -1;
                coverWidth = coverWidthElement != null ? (int)coverWidthElement : -1;
            }

            var zcodeElement = storyElement.Element(xmlns + "zcode");
            if (zcodeElement != null)
            {
                var coverPictureElement = zcodeElement.Element(xmlns + "coverpicture");
                if (coverPictureElement != null)
                {
                    coverId = coverPictureElement != null ? (int)coverPictureElement : -1;
                }
            }

            if (coverId >= 0)
            {
                var pictureKind = blorb.GetPictureKind(coverId);
                if (pictureKind != PictureKind.Unknown)
                {
                    using (var pictureStream = blorb.LoadPictureStream(coverId))
                    {
                        BitmapDecoder decoder;
                        if (pictureKind == PictureKind.Jpeg)
                        {
                            decoder = new JpegBitmapDecoder(pictureStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                        }
                        else // PictureKind.Png
                        {
                            decoder = new PngBitmapDecoder(pictureStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                        }

                        cover = decoder.Frames[0];
                    }
                }
            }
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

        public string CoverFormat
        {
            get { return coverFormat; }
        }

        public int CoverHeight
        {
            get { return coverHeight; }
        }

        public int CoverWidth
        {
            get { return coverWidth; }
        }

        public int CoverId
        {
            get { return coverId; }
        }

        public BitmapSource Cover
        {
            get { return cover; }
        }
    }
}
