using System.Linq;
using NUnit.Framework;
using ZDebug.Core.Tests.Utilities;
using ZDebug.Core.Text;

namespace ZDebug.Core.Tests
{
    [TestFixture]
    public class StoryTests_Curses
    {
        private Story LoadStory()
        {
            using (var stream = ZCode.LoadCurses())
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
            Assert.That(story.InformData.Version, Is.EqualTo(0));
        }

        [Test, Category(Categories.Story)]
        public void ObjectTable_Count()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.Count, Is.EqualTo(502));
        }

        [Test, Category(Categories.Story)]
        public void ObjectTable_478_ShortName()
        {
            var story = LoadStory();
            var shortNameZWords = story.ObjectTable.GetByNumber(478).PropertyTable.GetShortNameZWords();
            var shortName = ZText.ZWordsAsString(shortNameZWords, ZTextFlags.All, story.Memory);
            Assert.That(shortName, Is.EqualTo("Old Evans"));
        }

        [Test, Category(Categories.Story)]
        public void ObjectTable_478_Attributes()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(478).HasAttribute(0), Is.True);
            Assert.That(story.ObjectTable.GetByNumber(478).HasAttribute(16), Is.True);
            Assert.That(story.ObjectTable.GetByNumber(478).HasAttribute(23), Is.True);

            foreach (var n in Enumerable.Range(0, 48).Where(n => n != 0 && n != 16 && n != 23))
            {
                Assert.That(story.ObjectTable.GetByNumber(478).HasAttribute(n), Is.False);
            }
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_478_Parent_Number()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(478).Parent.Number, Is.EqualTo(477));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_478_Sibling_Number()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(478).Sibling.Number, Is.EqualTo(479));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_478_Child_Number()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(478).Child.Number, Is.EqualTo(483));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_478_PropertyTable_Address()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(478).PropertyTable.Address, Is.EqualTo(0x5802));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_478_PropertyTable_Count()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(478).PropertyTable.Count, Is.EqualTo(4));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_478_PropertyTable_18_DataLength()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(478).PropertyTable.GetByNumber(18).DataLength, Is.EqualTo(2));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_478_PropertyTable_17_DataLength()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(478).PropertyTable.GetByNumber(17).DataLength, Is.EqualTo(2));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_478_PropertyTable_4_DataLength()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(478).PropertyTable.GetByNumber(4).DataLength, Is.EqualTo(2));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_478_PropertyTable_1_DataLength()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(478).PropertyTable.GetByNumber(1).DataLength, Is.EqualTo(8));
        }
    }
}
