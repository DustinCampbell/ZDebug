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
        public void CZech_HasAttributeByObjectNumberOf5()
        {
            var memory = LoadCZech();
            Assert.That(memory.HasAttributeByObjectNumber(5, 0), Is.True);
            Assert.That(memory.HasAttributeByObjectNumber(5, 1), Is.True);

            for (int i = 2; i < 48; i++)
            {
                Assert.That(memory.HasAttributeByObjectNumber(5, i), Is.False);
            }
        }

        [Test, Category(Categories.Memory)]
        public void CZech_HasAttributeByObjectNumberOf6()
        {
            var memory = LoadCZech();
            Assert.That(memory.HasAttributeByObjectNumber(6, 0), Is.False);
            Assert.That(memory.HasAttributeByObjectNumber(6, 1), Is.False);
            Assert.That(memory.HasAttributeByObjectNumber(6, 2), Is.True);
            Assert.That(memory.HasAttributeByObjectNumber(6, 3), Is.True);

            for (int i = 4; i < 48; i++)
            {
                Assert.That(memory.HasAttributeByObjectNumber(6, i), Is.False);
            }
        }

        [Test, Category(Categories.Memory)]
        public void CZech_HasAttributeByObjectNumberOf7()
        {
            var memory = LoadCZech();

            for (int i = 0; i < 48; i++)
            {
                Assert.That(memory.HasAttributeByObjectNumber(7, i), Is.False);
            }
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
