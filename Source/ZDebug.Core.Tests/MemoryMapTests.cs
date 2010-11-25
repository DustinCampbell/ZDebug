using NUnit.Framework;
using ZDebug.Core.Basics;
using ZDebug.Core.Tests.Utilities;

namespace ZDebug.Core.Tests
{
    [TestFixture]
    public class MemoryMapTests
    {
        private Story LoadDreamhold()
        {
            using (var stream = ZCode.LoadDreamhold())
            {
                return Story.FromStream(stream);
            }
        }

        private void AssertRegion(MemoryMapRegion region, string name, int @base, int end, int size)
        {
            Assert.That(region.Name, Is.EqualTo(name));
            Assert.That(region.Base, Is.EqualTo(@base));
            Assert.That(region.End, Is.EqualTo(end));
            Assert.That(region.Size, Is.EqualTo(size));
        }

        [Test, Category(Categories.MemoryMap)]
        public void Dreamhold_MemoryMap()
        {
            var story = LoadDreamhold();
            AssertRegion(story.MemoryMap[0], "Header", 0x00, 0x3f, 0x40);
        }
    }
}