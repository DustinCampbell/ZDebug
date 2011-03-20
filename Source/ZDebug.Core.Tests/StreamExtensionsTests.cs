using System;
using System.IO;
using NUnit.Framework;
using ZDebug.Core.Extensions;
using ZDebug.Core.Tests.Utilities;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Tests
{
    [TestFixture]
    public class StreamExtensionsTests
    {
        [Test, Category(Categories.Utilties)]
        public void ReadFullyWithNulThrows()
        {
            Stream stream = null;
            Assert.That(() =>
                stream.ReadFully(),
                Throws.InstanceOf<ArgumentNullException>());
        }

        [Test, Category(Categories.Utilties)]
        public void ReadFullyProducesExpectedByteArray()
        {
            var bytes = ArrayEx.Create(8192, i => (byte)(i % 255));
            using (var stream = new MemoryStream(bytes))
            {
                var readBytes = stream.ReadFully();

                Assert.That(readBytes, Is.EqualTo(bytes));
            }
        }
    }
}
