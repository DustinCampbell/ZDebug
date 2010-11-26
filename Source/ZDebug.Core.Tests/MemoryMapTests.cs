﻿using NUnit.Framework;
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
            AssertRegion(story.MemoryMap[1], "Abbreviation data", 0x40, 0x41, 0x02);
            AssertRegion(story.MemoryMap[2], "Abbreviation pointer table", 0x42, 0x101, 0xc0);
            AssertRegion(story.MemoryMap[3], "Header extension table", 0x102, 0x109, 0x08);
            AssertRegion(story.MemoryMap[4], "Dictionary", 0xab8c, 0xda93, 0x2f08);
        }
    }
}