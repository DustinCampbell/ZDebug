using NUnit.Framework;
using ZDebug.Core.Basics;
using ZDebug.Core.Tests.Utilities;

namespace ZDebug.Core.Tests
{
    [TestFixture]
    public partial class MemoryExtensionsTests
    {
        [Test, Category(Categories.Memory)]
        public void ReadVersion()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadVersion(memory.Bytes), Is.EqualTo(5));
        }

        [Test, Category(Categories.Memory)]
        public void ReadReleaseNumber()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadReleaseNumber(memory.Bytes), Is.EqualTo(1));
        }

        [Test, Category(Categories.Memory)]
        public void ReadSerialNumber()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadSerialNumberText(memory.Bytes), Is.EqualTo("031102"));
        }

        [Test, Category(Categories.Memory)]
        public void ReadHighMemoryBase()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadHighMemoryBase(memory.Bytes), Is.EqualTo(0x07dc));
        }

        [Test, Category(Categories.Memory)]
        public void ReadInitialPC()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadInitialPC(memory.Bytes), Is.EqualTo(0x07dd));
        }

        [Test, Category(Categories.Memory)]
        public void ReadDictionaryAddress()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadDictionaryAddress(memory.Bytes), Is.EqualTo(0x07d3));
        }

        [Test, Category(Categories.Memory)]
        public void ReadObjectTableAddress()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadObjectTableAddress(memory.Bytes), Is.EqualTo(0x010e));
        }

        [Test, Category(Categories.Memory)]
        public void ReadGlobalVariableTableAddress()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadGlobalVariableTableAddress(memory.Bytes), Is.EqualTo(0x04f0));
        }

        [Test, Category(Categories.Memory)]
        public void ReadStaticMemoryBase()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadStaticMemoryBase(memory.Bytes), Is.EqualTo(0x07d1));
        }

        [Test, Category(Categories.Memory)]
        public void ReadAbbreviationsTableAddress()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadAbbreviationsTableAddress(memory.Bytes), Is.EqualTo(0x0046));
        }

        [Test, Category(Categories.Memory)]
        public void ReadFileSize()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadFileSize(memory.Bytes), Is.EqualTo(0x0333c));
        }

        [Test, Category(Categories.Memory)]
        public void ReadChecksum()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadChecksum(memory.Bytes), Is.EqualTo(0xbaaf));
        }

        [Test, Category(Categories.Memory)]
        public void ReadTerminatingCharactersTableAddress()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadTerminatingCharactersTableAddress(memory.Bytes), Is.EqualTo(0x07d0));
        }

        [Test, Category(Categories.Memory)]
        public void ReadHeaderExtensionTableAddress()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadHeaderExtensionTableAddress(memory.Bytes), Is.EqualTo(0x0106));
        }

        [Test, Category(Categories.Memory)]
        public void ReadInformVersionNumber()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadInformVersionNumber(memory.Bytes), Is.EqualTo(621));
            Assert.That(Header.ReadInformVersionText(memory.Bytes), Is.EqualTo("6.21"));
        }
    }
}
