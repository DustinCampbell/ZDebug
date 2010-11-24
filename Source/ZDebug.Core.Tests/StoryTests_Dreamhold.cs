using System.Linq;
using NUnit.Framework;
using ZDebug.Core.Tests.Utilities;

namespace ZDebug.Core.Tests
{
    [TestFixture]
    public class StoryTests_Dreamhold
    {
        private Story LoadStory()
        {
            using (var stream = ZCode.LoadDreamhold())
            {
                return Story.FromStream(stream);
            }
        }

        [Test, Category(Categories.Story)]
        public void CheckVersion()
        {
            var story = LoadStory();
            Assert.That(story.Version, Is.EqualTo(8));
        }

        [Test, Category(Categories.Story)]
        public void ObjectTable_Count()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.Count, Is.EqualTo(656));
        }

        [Test, Category(Categories.Story)]
        public void ObjectTable_484_Attributes()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(484).HasAttribute(21), Is.True);
            Assert.That(story.ObjectTable.GetByNumber(484).HasAttribute(25), Is.True);

            foreach (var n in Enumerable.Range(0, 48).Where(n => n != 21 && n != 25))
            {
                Assert.That(story.ObjectTable.GetByNumber(484).HasAttribute(n), Is.False);
            }
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_484_Parent_Number()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(484).Parent.Number, Is.EqualTo(483));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_484_Sibling_Number()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(484).Sibling.Number, Is.EqualTo(486));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_484_Child_Number()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(484).Child.Number, Is.EqualTo(485));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_484_PropertyTable_Address()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(484).PropertyTable.Address, Is.EqualTo(0x6a2b));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_484_PropertyTable_Count()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(484).PropertyTable.Count, Is.EqualTo(5));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_484_PropertyTable_36_DataLength()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(484).PropertyTable.GetByNumber(36).DataLength, Is.EqualTo(2));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_484_PropertyTable_31_DataLength()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(484).PropertyTable.GetByNumber(31).DataLength, Is.EqualTo(2));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_484_PropertyTable_4_DataLength()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(484).PropertyTable.GetByNumber(4).DataLength, Is.EqualTo(4));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_484_PropertyTable_2_DataLength()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(484).PropertyTable.GetByNumber(2).DataLength, Is.EqualTo(2));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_484_PropertyTable_1_DataLength()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(484).PropertyTable.GetByNumber(1).DataLength, Is.EqualTo(18));
        }
    }
}
