using NUnit.Framework;
using ZDebug.Core.Basics;
using ZDebug.Core.Extensions;
using ZDebug.Core.Tests.Utilities;

namespace ZDebug.Core.Tests
{
    [TestFixture]
    public class HeaderTests
    {
        private byte[] LoadCurses()
        {
            using (var stream = ZCode.LoadCurses())
            {
                return stream.ReadFully();
            }
        }

        private byte[] LoadCZech()
        {
            using (var stream = ZCode.LoadCZech())
            {
                return stream.ReadFully();
            }
        }

        private byte[] LoadZork1()
        {
            using (var stream = ZCode.LoadZork1())
            {
                return stream.ReadFully();
            }
        }

        [Test, Category(Categories.Header)]
        public void ReadVersion()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadVersion(memory), Is.EqualTo(5));
        }

        [Test, Category(Categories.Header)]
        public void ReadReleaseNumber()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadReleaseNumber(memory), Is.EqualTo(1));
        }

        [Test, Category(Categories.Header)]
        public void ReadSerialNumber()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadSerialNumberText(memory), Is.EqualTo("031102"));
        }

        [Test, Category(Categories.Header)]
        public void ReadHighMemoryBase()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadHighMemoryBase(memory), Is.EqualTo(0x07dc));
        }

        [Test, Category(Categories.Header)]
        public void ReadInitialPC()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadInitialPC(memory), Is.EqualTo(0x07dd));
        }

        [Test, Category(Categories.Header)]
        public void ReadDictionaryAddress()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadDictionaryAddress(memory), Is.EqualTo(0x07d3));
        }

        [Test, Category(Categories.Header)]
        public void ReadObjectTableAddress()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadObjectTableAddress(memory), Is.EqualTo(0x010e));
        }

        [Test, Category(Categories.Header)]
        public void ReadGlobalVariableTableAddress()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadGlobalVariableTableAddress(memory), Is.EqualTo(0x04f0));
        }

        [Test, Category(Categories.Header)]
        public void ReadStaticMemoryBase()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadStaticMemoryBase(memory), Is.EqualTo(0x07d1));
        }

        [Test, Category(Categories.Header)]
        public void ReadAbbreviationsTableAddress()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadAbbreviationsTableAddress(memory), Is.EqualTo(0x0046));
        }

        [Test, Category(Categories.Header)]
        public void ReadFileSize()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadFileSize(memory), Is.EqualTo(0x0333c));
        }

        [Test, Category(Categories.Header)]
        public void ReadChecksum()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadChecksum(memory), Is.EqualTo(0xbaaf));
        }

        [Test, Category(Categories.Header)]
        public void ReadTerminatingCharactersTableAddress()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadTerminatingCharactersTableAddress(memory), Is.EqualTo(0x07d0));
        }

        [Test, Category(Categories.Header)]
        public void ReadHeaderExtensionTableAddress()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadHeaderExtensionTableAddress(memory), Is.EqualTo(0x0106));
        }

        [Test, Category(Categories.Header)]
        public void ReadInformVersionNumber()
        {
            var memory = LoadCZech();
            Assert.That(Header.ReadInformVersionNumber(memory), Is.EqualTo(621));
            Assert.That(Header.ReadInformVersionText(memory), Is.EqualTo("6.21"));
        }
    }
}
