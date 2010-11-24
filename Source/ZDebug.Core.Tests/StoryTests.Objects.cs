using NUnit.Framework;
using ZDebug.Core.Tests.Utilities;

namespace ZDebug.Core.Tests
{
    [TestFixture]
    public partial class StoryTests
    {
        [Test, Category(Categories.Story)]
        public void Curses_ObjectTable_Count()
        {
            var story = LoadCurses();
            Assert.That(story.ObjectTable.Count, Is.EqualTo(502));
        }

        [Test, Category(Categories.Memory)]
        public void Curses_ObjectTable_478_Parent_Number()
        {
            var story = LoadCurses();
            Assert.That(story.ObjectTable.GetByNumber(478).Parent.Number, Is.EqualTo(477));
        }

        [Test, Category(Categories.Memory)]
        public void Curses_ObjectTable_478_Sibling_Number()
        {
            var story = LoadCurses();
            Assert.That(story.ObjectTable.GetByNumber(478).Sibling.Number, Is.EqualTo(479));
        }

        [Test, Category(Categories.Memory)]
        public void Curses_ObjectTable_478_Child_Number()
        {
            var story = LoadCurses();
            Assert.That(story.ObjectTable.GetByNumber(478).Child.Number, Is.EqualTo(483));
        }

        [Test, Category(Categories.Memory)]
        public void Curses_ObjectTable_478_PropertyTable_Address()
        {
            var story = LoadCurses();
            Assert.That(story.ObjectTable.GetByNumber(478).PropertyTable.Address, Is.EqualTo(0x5802));
        }

        [Test, Category(Categories.Memory)]
        public void Curses_ObjectTable_478_PropertyTable_Count()
        {
            var story = LoadCurses();
            Assert.That(story.ObjectTable.GetByNumber(478).PropertyTable.Count, Is.EqualTo(4));
        }

        [Test, Category(Categories.Memory)]
        public void Curses_ObjectTable_478_PropertyTable_18_DataLength()
        {
            var story = LoadCurses();
            Assert.That(story.ObjectTable.GetByNumber(478).PropertyTable.GetByNumber(18).DataLength, Is.EqualTo(2));
        }

        [Test, Category(Categories.Memory)]
        public void Curses_ObjectTable_478_PropertyTable_17_DataLength()
        {
            var story = LoadCurses();
            Assert.That(story.ObjectTable.GetByNumber(478).PropertyTable.GetByNumber(17).DataLength, Is.EqualTo(2));
        }

        [Test, Category(Categories.Memory)]
        public void Curses_ObjectTable_478_PropertyTable_4_DataLength()
        {
            var story = LoadCurses();
            Assert.That(story.ObjectTable.GetByNumber(478).PropertyTable.GetByNumber(4).DataLength, Is.EqualTo(2));
        }

        [Test, Category(Categories.Memory)]
        public void Curses_ObjectTable_478_PropertyTable_1_DataLength()
        {
            var story = LoadCurses();
            Assert.That(story.ObjectTable.GetByNumber(478).PropertyTable.GetByNumber(1).DataLength, Is.EqualTo(8));
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

        [Test, Category(Categories.Memory)]
        public void Zork1_ObjectTable_172_Parent_Number()
        {
            var story = LoadZork1();
            Assert.That(story.ObjectTable.GetByNumber(172).Parent.Number, Is.EqualTo(82));
        }

        [Test, Category(Categories.Memory)]
        public void Zork1_ObjectTable_172_Sibling_Number()
        {
            var story = LoadZork1();
            Assert.That(story.ObjectTable.GetByNumber(172).Sibling.Number, Is.EqualTo(100));
        }

        [Test, Category(Categories.Memory)]
        public void Zork1_ObjectTable_172_Child_Number()
        {
            var story = LoadZork1();
            Assert.That(story.ObjectTable.GetByNumber(172).Child.Number, Is.EqualTo(173));
        }

        [Test, Category(Categories.Memory)]
        public void Zork1_ObjectTable_172_PropertyTable_Address()
        {
            var story = LoadZork1();
            Assert.That(story.ObjectTable.GetByNumber(172).PropertyTable.Address, Is.EqualTo(0x1b75));
        }

        [Test, Category(Categories.Memory)]
        public void Zork1_ObjectTable_172_PropertyTable_Count()
        {
            var story = LoadZork1();
            Assert.That(story.ObjectTable.GetByNumber(172).PropertyTable.Count, Is.EqualTo(5));
        }

        [Test, Category(Categories.Memory)]
        public void Zork1_ObjectTable_172_PropertyTable_31_DataLength()
        {
            var story = LoadZork1();
            Assert.That(story.ObjectTable.GetByNumber(172).PropertyTable.GetByNumber(31).DataLength, Is.EqualTo(1));
        }

        [Test, Category(Categories.Memory)]
        public void Zork1_ObjectTable_172_PropertyTable_28_DataLength()
        {
            var story = LoadZork1();
            Assert.That(story.ObjectTable.GetByNumber(172).PropertyTable.GetByNumber(28).DataLength, Is.EqualTo(4));
        }

        [Test, Category(Categories.Memory)]
        public void Zork1_ObjectTable_172_PropertyTable_17_DataLength()
        {
            var story = LoadZork1();
            Assert.That(story.ObjectTable.GetByNumber(172).PropertyTable.GetByNumber(17).DataLength, Is.EqualTo(2));
        }

        [Test, Category(Categories.Memory)]
        public void Zork1_ObjectTable_172_PropertyTable_5_DataLength()
        {
            var story = LoadZork1();
            Assert.That(story.ObjectTable.GetByNumber(172).PropertyTable.GetByNumber(5).DataLength, Is.EqualTo(2));
        }

        [Test, Category(Categories.Memory)]
        public void Zork1_ObjectTable_172_PropertyTable_4_DataLength()
        {
            var story = LoadZork1();
            Assert.That(story.ObjectTable.GetByNumber(172).PropertyTable.GetByNumber(4).DataLength, Is.EqualTo(4));
        }
    }
}
