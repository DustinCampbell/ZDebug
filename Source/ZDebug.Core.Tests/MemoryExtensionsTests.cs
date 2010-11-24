using NUnit.Framework;
using ZDebug.Core.Basics;
using ZDebug.Core.Tests.Utilities;

namespace ZDebug.Core.Tests
{
    [TestFixture]
    public partial class MemoryExtensionsTests
    {
        private Memory LoadCurses()
        {
            using (var stream = ZCode.LoadCurses())
            {
                return new Memory(stream);
            }
        }

        private Memory LoadCZech()
        {
            using (var stream = ZCode.LoadCZech())
            {
                return new Memory(stream);
            }
        }

        private Memory LoadZork1()
        {
            using (var stream = ZCode.LoadZork1())
            {
                return new Memory(stream);
            }
        }
    }
}
