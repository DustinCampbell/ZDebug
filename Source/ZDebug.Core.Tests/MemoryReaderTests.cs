using System;
using NUnit.Framework;
using ZDebug.Core.Basics;
using ZDebug.Core.Tests.Utilities;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Tests
{
    [TestFixture]
    public class MemoryReaderTests
    {
        [Test, Category(Categories.Memory)]
        public void NextByteWorksAsExpectedFromIndexZero()
        {
            var bytes = ArrayEx.Create(1024, i => (byte)(i % 255));
            var reader = new MemoryReader(bytes, 0);

            for (int i = 0; i < bytes.Length; i++)
            {
                var b = reader.NextByte();
                Assert.That(b, Is.EqualTo(bytes[i]));
            }

            Assert.That(reader.Address, Is.EqualTo(reader.Size));

            Assert.That(() =>
                reader.NextByte(),
                Throws.InstanceOf<InvalidOperationException>());
        }

        [Test, Category(Categories.Memory)]
        public void NextByteWorksAsExpectedFromDifferentIndex()
        {
            var bytes = ArrayEx.Create(1024, i => (byte)(i % 255));
            var reader = new MemoryReader(bytes, 512);

            for (int i = 512; i < bytes.Length; i++)
            {
                var b = reader.NextByte();
                Assert.That(b, Is.EqualTo(bytes[i]));
            }

            Assert.That(reader.Address, Is.EqualTo(reader.Size));

            Assert.That(() =>
                reader.NextByte(),
                Throws.InstanceOf<InvalidOperationException>());
        }

        [Test, Category(Categories.Memory)]
        public void NextBytesWorksAsExpectedFromIndexZero()
        {
            byte[] bytes = { 0xff, 0x00, 0x0f, 0xf0, 0xff, 0x00, 0x0f, 0xf0 };
            var reader = new MemoryReader(bytes, 0);

            byte[][] expected =
            { 
                new byte[] { 0xff, 0x00 }, 
                new byte[] { 0x0f, 0xf0 }, 
                new byte[] { 0xff, 0x00 },
                new byte[] { 0x0f, 0xf0 } 
            };

            for (int i = 0; i < expected.Length; i++)
            {
                var b = reader.NextBytes(2);
                Assert.That(b, Is.EqualTo(expected[i]));
            }

            Assert.That(reader.Address, Is.EqualTo(reader.Size));

            Assert.That(() =>
                reader.NextBytes(2),
                Throws.InstanceOf<InvalidOperationException>());
        }

        [Test, Category(Categories.Memory)]
        public void NextBytesWorksAsExpectedFromDifferentIndex()
        {
            byte[] bytes = { 0xff, 0x00, 0x0f, 0xf0, 0xff, 0x00, 0x0f, 0xf0 };
            var reader = new MemoryReader(bytes, 4);

            byte[][] expected =
            { 
                new byte[] { 0xff, 0x00 }, 
                new byte[] { 0x0f, 0xf0 }, 
                new byte[] { 0xff, 0x00 },
                new byte[] { 0x0f, 0xf0 } 
            };

            for (int i = 2; i < expected.Length; i++)
            {
                var b = reader.NextBytes(2);
                Assert.That(b, Is.EqualTo(expected[i]));
            }

            Assert.That(reader.Address, Is.EqualTo(reader.Size));

            Assert.That(() =>
                reader.NextBytes(2),
                Throws.InstanceOf<InvalidOperationException>());
        }

        [Test, Category(Categories.Memory)]
        public void NextWordWorksAsExpectedFromIndexZero()
        {
            byte[] bytes = { 0xff, 0x00, 0x0f, 0xf0, 0xff, 0x00, 0x0f, 0xf0,
                             0xff, 0x00, 0x0f, 0xf0, 0xff, 0x00, 0x0f, 0xf0 };
            var reader = new MemoryReader(bytes, 0);

            ushort[] expected = { 0xff00, 0x0ff0, 0xff00, 0x0ff0, 0xff00, 0x0ff0, 0xff00, 0x0ff0 };

            for (int i = 0; i < expected.Length; i++)
            {
                var w = reader.NextWord();
                Assert.That(w, Is.EqualTo(expected[i]));
            }

            Assert.That(reader.Address, Is.EqualTo(reader.Size));

            Assert.That(() =>
                reader.NextWord(),
                Throws.InstanceOf<InvalidOperationException>());
        }

        [Test, Category(Categories.Memory)]
        public void NextWordWorksAsExpectedFromDifferentIndex()
        {
            byte[] bytes = { 0xff, 0x00, 0x0f, 0xf0, 0xff, 0x00, 0x0f, 0xf0,
                             0xff, 0x00, 0x0f, 0xf0, 0xff, 0x00, 0x0f, 0xf0 };
            var reader = new MemoryReader(bytes, 8);

            ushort[] expected = { 0xff00, 0x0ff0, 0xff00, 0x0ff0, 0xff00, 0x0ff0, 0xff00, 0x0ff0 };

            for (int i = 4; i < expected.Length; i++)
            {
                var w = reader.NextWord();
                Assert.That(w, Is.EqualTo(expected[i]));
            }

            Assert.That(reader.Address, Is.EqualTo(reader.Size));

            Assert.That(() =>
                reader.NextWord(),
                Throws.InstanceOf<InvalidOperationException>());
        }

        [Test, Category(Categories.Memory)]
        public void NextWordsWorksAsExpectedFromIndexZero()
        {
            byte[] bytes = { 0xff, 0x00, 0x0f, 0xf0, 0xff, 0x00, 0x0f, 0xf0,
                             0xff, 0x00, 0x0f, 0xf0, 0xff, 0x00, 0x0f, 0xf0 };
            var reader = new MemoryReader(bytes, 0);

            ushort[][] expected =
            { 
                new ushort[] { 0xff00, 0x0ff0 }, 
                new ushort[] { 0xff00, 0x0ff0 }, 
                new ushort[] { 0xff00, 0x0ff0 },
                new ushort[] { 0xff00, 0x0ff0 } 
            };

            for (int i = 0; i < expected.Length; i++)
            {
                var w = reader.NextWords(2);
                Assert.That(w, Is.EqualTo(expected[i]));
            }

            Assert.That(reader.Address, Is.EqualTo(reader.Size));

            Assert.That(() =>
                reader.NextWords(2),
                Throws.InstanceOf<InvalidOperationException>());
        }

        [Test, Category(Categories.Memory)]
        public void NextWordsWorksAsExpectedFromDifferentIndex()
        {
            byte[] bytes = { 0xff, 0x00, 0x0f, 0xf0, 0xff, 0x00, 0x0f, 0xf0,
                             0xff, 0x00, 0x0f, 0xf0, 0xff, 0x00, 0x0f, 0xf0 };
            var reader = new MemoryReader(bytes, 0);
            ushort[][] expected =
            { 
                new ushort[] { 0xff00, 0x0ff0 }, 
                new ushort[] { 0xff00, 0x0ff0 }, 
                new ushort[] { 0xff00, 0x0ff0 },
                new ushort[] { 0xff00, 0x0ff0 } 
            };

            for (int i = 2; i < expected.Length; i++)
            {
                var w = reader.NextWords(2);
                Assert.That(w, Is.EqualTo(expected[i]));
            }

            Assert.That(reader.Address, Is.EqualTo(reader.Size));

            Assert.That(() =>
                reader.NextWords(2),
                Throws.InstanceOf<InvalidOperationException>());
        }
    }
}
