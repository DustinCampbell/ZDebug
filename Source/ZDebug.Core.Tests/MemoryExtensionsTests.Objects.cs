using NUnit.Framework;
using ZDebug.Core.Basics;
using ZDebug.Core.Tests.Utilities;

namespace ZDebug.Core.Tests
{
    [TestFixture]
    public partial class MemoryExtensionsTests
    {
        [Test, Category(Categories.Memory)]
        public void CZech_ReadObjectParentNumberOf6()
        {
            var memory = LoadCZech();
            Assert.That(memory.ReadParentNumberByObjectNumber(6), Is.EqualTo(5));
        }

        [Test, Category(Categories.Memory)]
        public void CZech_ReadObjectSiblingNumberOf6()
        {
            var memory = LoadCZech();
            Assert.That(memory.ReadSiblingNumberByObjectNumber(6), Is.EqualTo(7));
        }

        [Test, Category(Categories.Memory)]
        public void CZech_ReadObjectChildNumberOf7()
        {
            var memory = LoadCZech();
            Assert.That(memory.ReadChildNumberByObjectNumber(7), Is.EqualTo(8));
        }

        [Test, Category(Categories.Memory)]
        public void CZech_ReadObjectPropertyTableAddressOf7()
        {
            var memory = LoadCZech();
            Assert.That(memory.ReadPropertyTableAddressByObjectNumber(7), Is.EqualTo(0x028e));
        }

        [Test, Category(Categories.Memory)]
        public void Curses_GetObjectCount()
        {
            var memory = LoadCurses();
            Assert.That(memory.GetObjectCount(), Is.EqualTo(502));
        }

        [Test, Category(Categories.Memory)]
        public void CZech_GetObjectCount()
        {
            var memory = LoadCZech();
            Assert.That(memory.GetObjectCount(), Is.EqualTo(10));
        }

        [Test, Category(Categories.Memory)]
        public void Zork1_GetObjectCount()
        {
            var memory = LoadZork1();
            Assert.That(memory.GetObjectCount(), Is.EqualTo(250));
        }
    }
}
