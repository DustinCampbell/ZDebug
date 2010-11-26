using NUnit.Framework;
using ZDebug.Core.Tests.Utilities;
using ZDebug.Core.Text;

namespace ZDebug.Core.Tests
{
    [TestFixture]
    public class StoryTests_CZech
    {
        private Story LoadStory()
        {
            using (var stream = ZCode.LoadCZech())
            {
                return Story.FromStream(stream);
            }
        }

        [Test, Category(Categories.Story)]
        public void CheckVersion()
        {
            var story = LoadStory();
            Assert.That(story.Version, Is.EqualTo(5));
        }

        [Test, Category(Categories.Story)]
        public void CheckIsInformStory()
        {
            var story = LoadStory();
            Assert.That(story.IsInformStory, Is.True);
        }

        [Test, Category(Categories.Story)]
        public void CheckInformVersion()
        {
            var story = LoadStory();
            Assert.That(story.InformVersion, Is.EqualTo(621));
        }

        [Test, Category(Categories.Story)]
        public void ObjectTable_Count()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.Count, Is.EqualTo(10));
        }

        [Test, Category(Categories.Story)]
        public void ObjectTable_6_ShortName()
        {
            var story = LoadStory();
            var shortNameZWords = story.ObjectTable.GetByNumber(6).PropertyTable.GetShortNameZWords();
            var shortName = ZText.ZWordsAsString(shortNameZWords, ZTextFlags.All, story.Memory);
            Assert.That(shortName, Is.EqualTo("Test Object #2"));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_ParentNumberOf6()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(6).Parent.Number, Is.EqualTo(5));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_SiblingNumberOf6()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(6).Sibling.Number, Is.EqualTo(7));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_ChildNumberOf7()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(7).Child.Number, Is.EqualTo(8));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_PropertyTableAddressOf7()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(7).PropertyTable.Address, Is.EqualTo(0x028e));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_PropertyTableCountOf7()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(7).PropertyTable.Count, Is.EqualTo(2));
        }
    }
}
