using System.IO;
using NUnit.Framework;
using ZDebug.Core.Tests.Utilities;

namespace ZDebug.Core.Tests
{
    [TestFixture]
    public class SanityZCodeTests
    {
        [Test, Category(Categories.Sanity)]
        public void LoadCZechWorks()
        {
            Stream stream = null;
            try
            {
                Assert.DoesNotThrow(() =>
                    stream = ZCode.LoadCZech());

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
