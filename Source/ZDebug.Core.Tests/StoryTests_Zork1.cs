using NUnit.Framework;
using ZDebug.Core.Tests.Utilities;

namespace ZDebug.Core.Tests
{
    [TestFixture]
    public class StoryTests_Zork1
    {
        private Story LoadStory()
        {
            using (var stream = ZCode.LoadZork1())
            {
                return Story.FromStream(stream);
            }
        }

        [Test, Category(Categories.Story)]
        public void CheckVersion()
        {
            var story = LoadStory();
            Assert.That(story.Version, Is.EqualTo(3));
        }

        [Test, Category(Categories.Story)]
        public void ObjectTable_Count()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.Count, Is.EqualTo(250));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_172_Parent_Number()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(172).Parent.Number, Is.EqualTo(82));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_172_Sibling_Number()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(172).Sibling.Number, Is.EqualTo(100));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_172_Child_Number()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(172).Child.Number, Is.EqualTo(173));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_172_PropertyTable_Address()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(172).PropertyTable.Address, Is.EqualTo(0x1b75));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_172_PropertyTable_Count()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(172).PropertyTable.Count, Is.EqualTo(5));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_172_PropertyTable_31_DataLength()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(172).PropertyTable.GetByNumber(31).DataLength, Is.EqualTo(1));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_172_PropertyTable_28_DataLength()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(172).PropertyTable.GetByNumber(28).DataLength, Is.EqualTo(4));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_172_PropertyTable_17_DataLength()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(172).PropertyTable.GetByNumber(17).DataLength, Is.EqualTo(2));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_172_PropertyTable_5_DataLength()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(172).PropertyTable.GetByNumber(5).DataLength, Is.EqualTo(2));
        }

        [Test, Category(Categories.Memory)]
        public void ObjectTable_172_PropertyTable_4_DataLength()
        {
            var story = LoadStory();
            Assert.That(story.ObjectTable.GetByNumber(172).PropertyTable.GetByNumber(4).DataLength, Is.EqualTo(4));
        }
    }
}
