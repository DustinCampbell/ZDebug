using NUnit.Framework;
using ZDebug.Core.Tests.Utilities;

namespace ZDebug.Core.Tests
{
    [TestFixture]
    public partial class StoryTests
    {
        [Test, Category(Categories.Memory)]
        public void Curses_ObjectTable_478_ParentNumber()
        {
            var story = LoadCurses();
            Assert.That(story.ObjectTable.GetByNumber(478).Parent.Number, Is.EqualTo(477));
        }

        [Test, Category(Categories.Memory)]
        public void Curses_ObjectTable_478_SiblingNumber()
        {
            var story = LoadCurses();
            Assert.That(story.ObjectTable.GetByNumber(478).Sibling.Number, Is.EqualTo(479));
        }

        [Test, Category(Categories.Memory)]
        public void Curses_ObjectTable_478_ChildNumber()
        {
            var story = LoadCurses();
            Assert.That(story.ObjectTable.GetByNumber(478).Child.Number, Is.EqualTo(483));
        }

        [Test, Category(Categories.Memory)]
        public void Curses_ObjectTable_478_PropertyTableAddress()
        {
            var story = LoadCurses();
            Assert.That(story.ObjectTable.GetByNumber(478).PropertyTable.Address, Is.EqualTo(0x5802));
        }

        [Test, Category(Categories.Memory)]
        public void Curses_ObjectTable_478_PropertyTableCount()
        {
            var story = LoadCurses();
            Assert.That(story.ObjectTable.GetByNumber(478).PropertyTable.Count, Is.EqualTo(4));
        }

        [Test, Category(Categories.Memory)]
        public void CZech_ObjectTable_ParentNumberOf6()
        {
            var story = LoadCZech();
            Assert.That(story.ObjectTable.GetByNumber(6).Parent.Number, Is.EqualTo(5));
        }

        [Test, Category(Categories.Memory)]
        public void CZech_ObjectTable_SiblingNumberOf6()
        {
            var story = LoadCZech();
            Assert.That(story.ObjectTable.GetByNumber(6).Sibling.Number, Is.EqualTo(7));
        }

        [Test, Category(Categories.Memory)]
        public void CZech_ObjectTable_ChildNumberOf7()
        {
            var story = LoadCZech();
            Assert.That(story.ObjectTable.GetByNumber(7).Child.Number, Is.EqualTo(8));
        }

        [Test, Category(Categories.Memory)]
        public void CZech_ObjectTable_PropertyTableAddressOf7()
        {
            var story = LoadCZech();
            Assert.That(story.ObjectTable.GetByNumber(7).PropertyTable.Address, Is.EqualTo(0x028e));
        }

        [Test, Category(Categories.Memory)]
        public void CZech_ObjectTable_PropertyTableCountOf7()
        {
            var story = LoadCZech();
            Assert.That(story.ObjectTable.GetByNumber(7).PropertyTable.Count, Is.EqualTo(2));
        }

        [Test, Category(Categories.Story)]
        public void Curses_ObjectTable_Count()
        {
            var story = LoadCurses();
            Assert.That(story.ObjectTable.Count, Is.EqualTo(502));
        }

        [Test, Category(Categories.Story)]
        public void CZech_ObjectTable_Count()
        {
            var story = LoadCZech();
            Assert.That(story.ObjectTable.Count, Is.EqualTo(10));
        }

        [Test, Category(Categories.Story)]
        public void Zork1_ObjectTable_Count()
        {
            var story = LoadZork1();
            Assert.That(story.ObjectTable.Count, Is.EqualTo(250));
        }
    }
}
