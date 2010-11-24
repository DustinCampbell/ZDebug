using NUnit.Framework;
using ZDebug.Core.Tests.Utilities;

namespace ZDebug.Core.Tests
{
    [TestFixture]
    public class StoryTests
    {
        private Story LoadCZech()
        {
            using (var stream = ZCode.LoadCZech())
            {
                return Story.FromStream(stream);
            }
        }

        [Test, Category(Categories.Memory)]
        public void CheckVersion()
        {
            var story = LoadCZech();
            Assert.That(story.Version, Is.EqualTo(5));
        }
    }
}
