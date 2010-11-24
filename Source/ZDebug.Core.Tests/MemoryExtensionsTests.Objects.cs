using NUnit.Framework;
using ZDebug.Core.Basics;
using ZDebug.Core.Tests.Utilities;

namespace ZDebug.Core.Tests
{
    [TestFixture]
    public partial class MemoryExtensionsTests
    {
        [Test, Category(Categories.Memory)]
        public void ReadObjectParentIndex()
        {
            var memory = LoadCZech();
            Assert.That(memory.ReadObjectParentIndex(6), Is.EqualTo(5));
        }

        [Test, Category(Categories.Memory)]
        public void ReadObjectSiblingIndex()
        {
            var memory = LoadCZech();
            Assert.That(memory.ReadObjectSiblingIndex(6), Is.EqualTo(7));
        }

        [Test, Category(Categories.Memory)]
        public void ReadObjectChildIndex()
        {
            var memory = LoadCZech();
            Assert.That(memory.ReadObjectChildIndex(7), Is.EqualTo(8));
        }

        [Test, Category(Categories.Memory)]
        public void ReadObjectPropertyTableAddress()
        {
            var memory = LoadCZech();
            Assert.That(memory.ReadObjectPropertyTableAddress(7), Is.EqualTo(0x028e));
        }

        [Test, Category(Categories.Memory)]
        public void GetObjectCount()
        {
            var memory = LoadCZech();
            Assert.That(memory.GetObjectCount(), Is.EqualTo(10));
        }
    }
}
