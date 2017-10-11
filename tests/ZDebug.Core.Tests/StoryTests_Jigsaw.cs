using NUnit.Framework;
using ZDebug.Core.Tests.Utilities;

namespace ZDebug.Core.Tests
{
    [TestFixture]
    public class StoryTests_Jigsaw
    {
        private Story LoadStory()
        {
            using (var stream = ZCode.LoadJigsaw())
            {
                return Story.FromStream(stream);
            }
        }

        [Test, Category(Categories.Story)]
        public void CheckVersion()
        {
            var story = LoadStory();
            Assert.That(story.Version, Is.EqualTo(8));
        }

        [Test, Category(Categories.Story)]
        public void CheckIsInformStory()
        {
            var story = LoadStory();
            Assert.That(story.IsInformStory, Is.True);
        }

        [Test, Category(Categories.Story)]
        public void CheckInformVersion()
        {
            var story = LoadStory();
            Assert.That(story.InformData.Version, Is.EqualTo(0));
        }
    }
}
