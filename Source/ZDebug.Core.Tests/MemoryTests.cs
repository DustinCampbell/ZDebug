using System;
using System.IO;
using NUnit.Framework;
using ZDebug.Core.Basics;
using ZDebug.Core.Tests.Utilities;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Tests
{
    [TestFixture]
    public class MemoryTests
    {
        [Test, Category(Categories.Memory)]
        public void CreateWithNullArrayThrows()
        {
            byte[] array = null;
            Assert.That(() =>
                new Memory(array),
                Throws.InstanceOf<ArgumentNullException>());
        }

        [Test, Category(Categories.Memory)]
        public void CreateWithNullStreamThrows()
        {
            Stream stream = null;
            Assert.That(() =>
                new Memory(stream),
                Throws.InstanceOf<ArgumentNullException>());
        }

        [Test, Category(Categories.Memory)]
        public void SizeEqualsLengthOfInitialByteArray()
        {
            var bytes = ArrayEx.Create(1024, i => (byte)(i % 255));
            var memory = new Memory(bytes);

            Assert.That(memory.Size, Is.EqualTo(bytes.Length));
        }

        [Test, Category(Categories.Memory)]
        public void SizeEqualsLengthOfInitialStream()
        {
            var bytes = ArrayEx.Create(1024, i => (byte)(i % 255));
            using (var stream = new MemoryStream(bytes))
            {
                var memory = new Memory(stream);

                Assert.That(memory.Size, Is.EqualTo(stream.Length));
            }
        }

        [Test, Category(Categories.Memory)]
        public void ReadByteReturnsExpectedByteAtIndex()
        {
            var bytes = ArrayEx.Create(1024, i => (byte)(i % 255));
            var memory = new Memory(bytes);

            for (int i = 0; i < memory.Size; i++)
            {
                var b = memory.ReadByte(i);
                Assert.That(b, Is.EqualTo(bytes[i]));
            }
        }

        [Test, Category(Categories.Memory)]
        public void ReadBytesReturnsExpectedByteArrayAtIndex()
        {
            byte[] bytes = { 0xff, 0x00, 0x0f, 0xf0, 0xff, 0x00 };
            var memory = new Memory(bytes);

            byte[][] expected =
            { 
                new byte[] { 0xff, 0x00 }, 
                new byte[] { 0x00, 0x0f }, 
                new byte[] { 0x0f, 0xf0 }, 
                new byte[] { 0xf0, 0xff }, 
                new byte[] { 0xff, 0x00 }
            };

            for (int i = 0; i < expected.Length; i++)
            {
                var b = memory.ReadBytes(i, 2);
                Assert.That(b, Is.EqualTo(expected[i]));
            }
        }

        [Test, Category(Categories.Memory)]
        public void ReadWordReturnsExpectedByteAtIndex()
        {
            byte[] bytes = { 0xff, 0x00, 0x0f, 0xf0, 0xff, 0x00 };
            var memory = new Memory(bytes);

            ushort[] expected = { 0xff00, 0x000f, 0x0ff0, 0xf0ff, 0xff00 };

            for (int i = 0; i < memory.Size - 1; i++)
            {
                var w = memory.ReadWord(i);
                Assert.That(w, Is.EqualTo(expected[i]));
            }
        }

        [Test, Category(Categories.Memory)]
        public void ReadWordsReturnsExpectedWordArrayAtIndex()
        {
            byte[] bytes = { 0xff, 0x00, 0x0f, 0xf0, 0xff, 0x00 };
            var memory = new Memory(bytes);

            ushort[][] expected =
            { 
                new ushort[] { 0xff00, 0x0ff0 }, 
                new ushort[] { 0x000f, 0xf0ff }, 
                new ushort[] { 0x0ff0, 0xff00 }
            };

            for (int i = 0; i < expected.Length; i++)
            {
                var w = memory.ReadWords(i, 2);
                Assert.That(w, Is.EqualTo(expected[i]));
            }
        }

        [Test, Category(Categories.Memory)]
        public void WriteByteWritesExpectedByteAtIndex()
        {
            var bytes = new byte[1024];
            var memory = new Memory(bytes);

            for (int i = 0; i < memory.Size; i++)
            {
                var b = memory.ReadByte(i);
                Assert.That(b, Is.EqualTo(0));

                memory.WriteByte(i, (byte)(i % 255));
            }

            var expected = ArrayEx.Create(1024, i => (byte)(i % 255));

            for (int i = 0; i < memory.Size; i++)
            {
                var b = memory.ReadByte(i);
                Assert.That(b, Is.EqualTo(bytes[i]));
            }
        }

        [Test, Category(Categories.Memory)]
        public void WriteBytesWritesExpectedByteArrayAtIndex()
        {
            byte[] bytes = new byte[6];
            var memory = new Memory(bytes);

            byte[][] expected =
            {
                new byte[] { 0xff, 0x00 }, 
                new byte[] { 0x00, 0x0f }, 
                new byte[] { 0x0f, 0xf0 }, 
                new byte[] { 0xf0, 0xff }, 
                new byte[] { 0xff, 0x00 }
            };

            for (int i = 0; i < expected.Length; i++)
            {
                memory.WriteBytes(i, expected[i]);
                var b = memory.ReadBytes(i, 2);
                Assert.That(b, Is.EqualTo(expected[i]));
            }
        }

        [Test, Category(Categories.Memory)]
        public void WriteWordWritesExpectedWordAtIndex()
        {
            byte[] bytes = new byte[6];
            var memory = new Memory(bytes);

            ushort[] expected = { 0xff00, 0x000f, 0x0ff0, 0xf0ff, 0xff00 };

            for (int i = 0; i < expected.Length; i++)
            {
                memory.WriteWord(i, expected[i]);
                var w = memory.ReadWord(i);
                Assert.That(w, Is.EqualTo(expected[i]));
            }
        }

        [Test, Category(Categories.Memory)]
        public void WriteWordsWritesExpectedWordArrayAtIndex()
        {
            byte[] bytes = { 0xff, 0x00, 0x0f, 0xf0, 0xff, 0x00 };
            var memory = new Memory(bytes);

            ushort[][] expected =
            { 
                new ushort[] { 0xff00, 0x0ff0 }, 
                new ushort[] { 0x000f, 0xf0ff }, 
                new ushort[] { 0x0ff0, 0xff00 }
            };

            for (int i = 0; i < expected.Length; i++)
            {
                memory.WriteWords(i, expected[i]);
                var w = memory.ReadWords(i, 2);
                Assert.That(w, Is.EqualTo(expected[i]));
            }
        }

        [Test, Category(Categories.Memory)]
        public void CreateMemoryReaderWithInvalidIndexThrows()
        {
            var bytes = ArrayEx.Create(1024, i => (byte)(i % 255));
            var memory = new Memory(bytes);

            Assert.That(() =>
                memory.CreateReader(-1),
                Throws.InstanceOf<ArgumentOutOfRangeException>());

            Assert.That(new TestDelegate(() =>
                memory.CreateReader(0)),
                Throws.Nothing);

            Assert.That(new TestDelegate(() =>
                memory.CreateReader(1023)),
                Throws.Nothing);

            Assert.That(() =>
                memory.CreateReader(1024),
                Throws.InstanceOf<ArgumentOutOfRangeException>());
        }
    }
}
