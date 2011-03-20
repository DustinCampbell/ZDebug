using System;
using NUnit.Framework;
using ZDebug.Core.Extensions;
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

        [Test, Category(Categories.Utilties)]
        public void ResizeWithNullThrows()
        {
            int[] array = null;

            Assert.That(() =>
                array.Resize(0),
                Throws.InstanceOf<ArgumentNullException>());
        }

        [Test, Category(Categories.Utilties)]
        public void ResizeWithLengthLessThanZeroThrows()
        {
            var array = ArrayEx.Empty<int>();

            Assert.That(() =>
                array.Resize(-1),
                Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test, Category(Categories.Utilties)]
        public void ResizeWithSameLengthProducesIdenticalArray()
        {
            var array = ArrayEx.Create(10, i => i + 1);
            var resizedArray = array.Resize(array.Length);

            Assert.That(resizedArray.Length, Is.EqualTo(array.Length));

            Assert.That(resizedArray, Is.EqualTo(array));
        }

        [Test, Category(Categories.Utilties)]
        public void ResizeWithSmallerLengthTruncates()
        {
            var array = ArrayEx.Create(10, i => i + 1);
            var resizedArray = array.Resize(array.Length / 2);

            Assert.That(resizedArray.Length, Is.LessThan(array.Length));

            for (int i = 0; i < resizedArray.Length; i++)
            {
                Assert.That(resizedArray[i], Is.EqualTo(array[i]));
            }
        }

        [Test, Category(Categories.Utilties)]
        public void ResizeWithLargerLengthGrows()
        {
            var array = ArrayEx.Create(10, i => i + 1);
            var resizedArray = array.Resize(array.Length * 2);

            Assert.That(resizedArray.Length, Is.GreaterThan(array.Length));

            for (int i = 0; i < array.Length; i++)
            {
                Assert.That(resizedArray[i], Is.EqualTo(array[i]));
            }

            for (int i = array.Length; i < resizedArray.Length; i++)
            {
                Assert.That(resizedArray[i], Is.EqualTo(0));
            }
        }

        [Test, Category(Categories.Utilties)]
        public void SelectWithNullThrows()
        {
            int[] array = null;

            Assert.That(() =>
                array.Select(i => (byte)i),
                Throws.InstanceOf<ArgumentNullException>());
        }

        [Test, Category(Categories.Utilties)]
        public void SelectWithEmptyArrayAndNullLambdaDoesNotThrow()
        {
            var array = ArrayEx.Empty<int>();

            Assert.That(new TestDelegate(() =>
                array.Select<int, byte>(null)),
                Throws.Nothing);
        }

        [Test, Category(Categories.Utilties)]
        public void SelectWithNullLambdaThrows()
        {
            var array = ArrayEx.Create(10, i => i + 1);

            Assert.That(() =>
                array.Select<int, byte>(null),
                Throws.InstanceOf<ArgumentNullException>());
        }

        [Test, Category(Categories.Utilties)]
        public void SelectIntArrayToByteArray()
        {
            var array = ArrayEx.Create(10, i => i + 1);
            var newArray = array.Select(i => (byte)i);

            byte[] expected = ArrayEx.Create(10, i => (byte)(i + 1));

            Assert.That(newArray, Is.EqualTo(expected));
        }
    }
}
