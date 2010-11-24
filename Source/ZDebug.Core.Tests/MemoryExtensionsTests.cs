using NUnit.Framework;
using ZDebug.Core.Basics;
using ZDebug.Core.Tests.Utilities;

namespace ZDebug.Core.Tests
{
    [TestFixture]
    public partial class MemoryExtensionsTests
    {
        private Memory LoadCZech()
        {
            using (var stream = ZCode.LoadCZech())
            {
                return new Memory(stream);
            }
        }
    }
}
