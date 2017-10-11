using System.IO;
using NUnit.Framework;
using ZDebug.Core.Tests.Utilities;

namespace ZDebug.Core.Tests
{
    [TestFixture]
    public class SanityZCodeTests
    {
        [Test, Category(Categories.Sanity)]
        public void LoadCursesWorks()
        {
            Stream stream = null;
            try
            {
                Assert.That(new TestDelegate(() =>
                    stream = ZCode.LoadCurses()),
                    Throws.Nothing);

                Assert.That(stream, Is.Not.Null);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }
        }

        [Test, Category(Categories.Sanity)]
        public void LoadCZechWorks()
        {
            Stream stream = null;
            try
            {
                Assert.That(new TestDelegate(() =>
                    stream = ZCode.LoadCZech()),
                    Throws.Nothing);

                Assert.That(stream, Is.Not.Null);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }
        }

        [Test, Category(Categories.Sanity)]
        public void LoadZork1Works()
        {
            Stream stream = null;
            try
            {
                Assert.That(new TestDelegate(() =>
                    stream = ZCode.LoadZork1()),
                    Throws.Nothing);

                Assert.That(stream, Is.Not.Null);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }
        }
    }
}
