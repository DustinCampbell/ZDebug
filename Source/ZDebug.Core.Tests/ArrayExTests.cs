using System;
using NUnit.Framework;
using ZDebug.Core.Tests.Utilities;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Tests
{
    [TestFixture]
    public class ArrayExTests
    {
        [Test, Category(Categories.Utilties)]
        public void CreateWithLengthLessThanZeroThrows()
        {
            Assert.That(() =>
                ArrayEx.Create<int>(-1),
                Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test, Category(Categories.Utilties)]
        public void CreateEmptyArrayWithNullLambda()
        {
            var array = ArrayEx.Create<int>(5);

            Assert.That(array.Length, Is.EqualTo(5));

            for (int i = 0; i < array.Length; i++)
            {
                Assert.That(array[i], Is.EqualTo(0));
            }
        }

        [Test, Category(Categories.Utilties)]
        public void CreateSimpleArray()
        {
            var array = ArrayEx.Create<int>(5, i => i + 1);

            Assert.That(array.Length, Is.EqualTo(5));

            int[] expected = { 1, 2, 3, 4, 5 };

            for (int i = 0; i < array.Length; i++)
            {
                Assert.That(array[i], Is.EqualTo(expected[i]));
            }
        }

        [Test, Category(Categories.Utilties)]
        public void Empty()
        {
            var array = ArrayEx.Empty<int>();

            Assert.That(array.Length, Is.EqualTo(0));
        }
    }
}
