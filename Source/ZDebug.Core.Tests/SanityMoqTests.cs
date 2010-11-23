using Moq;
using NUnit.Framework;

namespace ZDebug.Core.Tests
{
    [TestFixture]
    public class SanityMoqTests
    {
        public interface ICalculator
        {
            int Add(int x, int y);
        }

        [Test, Category(Categories.Sanity)]
        public void SimpleMock()
        {
            var calc = new Mock<ICalculator>();
            calc.Setup(c => c.Add(2, 2)).Returns(4);

            Assert.That(calc.Object.Add(2, 2), Is.EqualTo(4));
        }
    }
}
