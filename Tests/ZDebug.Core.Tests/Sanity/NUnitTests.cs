using NUnit.Framework;
using ZDebug.Core.Tests.Utilities;

namespace ZDebug.Core.Tests
{
    [TestFixture]
    public class SanityNUnitTests
    {
        [Test, Category(Categories.Sanity)]
        public void AssertTrue()
        {
            Assert.That(true, Is.True);
        }

        [Test, Category(Categories.Sanity)]
        public void AssertFalse()
        {
            Assert.That(false, Is.False);
        }
    }
}
