using NUnit.Framework;
using ZDebug.Core.Tests.Utilities;

namespace ZDebug.Core.Tests
{
    [TestFixture]
    public partial class StoryTests
    {
        private Story LoadCurses()
        {
            using (var stream = ZCode.LoadCurses())
            {
                return Story.FromStream(stream);
            }
        }

        private Story LoadCZech()
        {
            using (var stream = ZCode.LoadCZech())
            {
                return Story.FromStream(stream);
            }
        }

        private Story LoadZork1()
        {
            using (var stream = ZCode.LoadZork1())
            {
                return Story.FromStream(stream);
            }
        }

        [Test, Category(Categories.Story)]
        public void Curses_CheckVersion()
        {
            var story = LoadCurses();
            Assert.That(story.Version, Is.EqualTo(5));
        }

        [Test, Category(Categories.Story)]
        public void CZech_CheckVersion()
        {
            var story = LoadCZech();
            Assert.That(story.Version, Is.EqualTo(5));
        }

        [Test, Category(Categories.Story)]
        public void Zork1_CheckVersion()
        {
            var story = LoadZork1();
            Assert.That(story.Version, Is.EqualTo(3));
        }
    }
}
