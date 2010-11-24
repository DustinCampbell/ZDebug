using NUnit.Framework;
using ZDebug.Core.Basics;
using ZDebug.Core.Tests.Utilities;

namespace ZDebug.Core.Tests
{
    [TestFixture]
    public class MemoryExtensionsTests
    {
        private Memory LoadCZech()
        {
            using (var stream = ZCode.LoadCZech())
            {
                return new Memory(stream);
            }
        }

        ///////////////////////////////////////////////////////////////////////
        // Header
        ///////////////////////////////////////////////////////////////////////

        [Test, Category(Categories.Memory)]
        public void ReadVersion()
        {
            var memory = LoadCZech();
            Assert.That(memory.ReadVersion(), Is.EqualTo(5));
        }

        [Test, Category(Categories.Memory)]
        public void ReadReleaseNumber()
        {
            var memory = LoadCZech();
            Assert.That(memory.ReadReleaseNumber(), Is.EqualTo(1));
        }

        [Test, Category(Categories.Memory)]
        public void ReadSerialNumber()
        {
            var memory = LoadCZech();
            Assert.That(memory.ReadSerialNumber(), Is.EqualTo("031102"));
        }

        [Test, Category(Categories.Memory)]
        public void ReadHighMemoryBase()
        {
            var memory = LoadCZech();
            Assert.That(memory.ReadHighMemoryBase(), Is.EqualTo(0x07dc));
        }

        [Test, Category(Categories.Memory)]
        public void ReadInitialPC()
        {
            var memory = LoadCZech();
            Assert.That(memory.ReadInitialPC(), Is.EqualTo(0x07dd));
        }

        [Test, Category(Categories.Memory)]
        public void ReadDictionaryAddress()
        {
            var memory = LoadCZech();
            Assert.That(memory.ReadDictionaryAddress(), Is.EqualTo(0x07d3));
        }

        [Test, Category(Categories.Memory)]
        public void ReadObjectTableAddress()
        {
            var memory = LoadCZech();
            Assert.That(memory.ReadObjectTableAddress(), Is.EqualTo(0x010e));
        }

        [Test, Category(Categories.Memory)]
        public void ReadGlobalVariableTableAddress()
        {
            var memory = LoadCZech();
            Assert.That(memory.ReadGlobalVariableTableAddress(), Is.EqualTo(0x04f0));
        }

        [Test, Category(Categories.Memory)]
        public void ReadStaticMemoryBase()
        {
            var memory = LoadCZech();
            Assert.That(memory.ReadStaticMemoryBase(), Is.EqualTo(0x07d1));
        }

        [Test, Category(Categories.Memory)]
        public void ReadAbbreviationsTableAddress()
        {
            var memory = LoadCZech();
            Assert.That(memory.ReadAbbreviationsTableAddress(), Is.EqualTo(0x0046));
        }

        [Test, Category(Categories.Memory)]
        public void ReadFileSize()
        {
            var memory = LoadCZech();
            Assert.That(memory.ReadFileSize(), Is.EqualTo(0x0333c));
        }

        [Test, Category(Categories.Memory)]
        public void ReadChecksum()
        {
            var memory = LoadCZech();
            Assert.That(memory.ReadChecksum(), Is.EqualTo(0xbaaf));
        }

        [Test, Category(Categories.Memory)]
        public void ReadTerminatingCharactersTableAddress()
        {
            var memory = LoadCZech();
            Assert.That(memory.ReadTerminatingCharactersTableAddress(), Is.EqualTo(0x07d0));
        }

        [Test, Category(Categories.Memory)]
        public void ReadHeaderExtensionTableAddress()
        {
            var memory = LoadCZech();
            Assert.That(memory.ReadHeaderExtensionTableAddress(), Is.EqualTo(0x0106));
        }

        [Test, Category(Categories.Memory)]
        public void ReadInformVersionNumber()
        {
            var memory = LoadCZech();
            Assert.That(memory.ReadInformVersionNumber(), Is.EqualTo("6.21"));
        }

        ///////////////////////////////////////////////////////////////////////
        // Objects
        ///////////////////////////////////////////////////////////////////////

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
    }
}
