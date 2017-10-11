using NUnit.Framework;
using ZDebug.Core.Tests.Utilities;

namespace ZDebug.Core.Tests
{
    [TestFixture]
    public class InformDataTests
    {
        private Story LoadCZech()
        {
            using (var stream = ZCode.LoadCZech())
            {
                return Story.FromStream(stream);
            }
        }

        [Test, Category(Categories.InformData)]
        public void GetPropertyName4()
        {
            var story = LoadCZech();
            Assert.That(story.InformData.GetPropertyName(4), Is.EqualTo("propa"));
        }

        [Test, Category(Categories.InformData)]
        public void GetPropertyName7()
        {
            var story = LoadCZech();
            Assert.That(story.InformData.GetPropertyName(7), Is.EqualTo("propd"));
        }
    }
}
