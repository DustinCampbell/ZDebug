using NUnit.Framework;
using ZDebug.Core.Basics;
using ZDebug.Core.Instructions;
using ZDebug.Core.Tests.Utilities;

namespace ZDebug.Core.Tests
{
    [TestFixture]
    public class InstructionReaderTests_Curses
    {
        private Story LoadStory()
        {
            using (var stream = ZCode.LoadCurses())
            {
                return Story.FromStream(stream);
            }
        }

        [Test]
        public void FirstInstruction()
        {
            var story = LoadStory();
            var initialPC = Header.ReadInitialPC(story.Memory.Bytes);

            var ireader = new InstructionReader(initialPC, story.Memory.Bytes, null);

            var i = ireader.NextInstruction();

            Assert.That(i, Is.Not.Null, "null check");
            Assert.That(i.Address, Is.EqualTo(0xa31d), "address check");
            Assert.That(i.Length, Is.EqualTo(3), "length check");
            Assert.That(i.Opcode.Kind, Is.EqualTo(OpcodeKind.OneOp), "opcode kind check");
            Assert.That(i.Opcode.Number, Is.EqualTo(0x0f), "opcode number check");
            Assert.That(i.OperandCount, Is.EqualTo(1), "operand count check");
            Assert.That(i.Operands[0].Kind, Is.EqualTo(OperandKind.LargeConstant), "operand kind check");
            Assert.That(i.Operands[0].Value, Is.EqualTo(0x34ce), "operand value check");
            Assert.That(i.HasStoreVariable, Is.False, "store variable check");
            Assert.That(i.HasBranch, Is.False, "branch check");
            Assert.That(i.HasZText, Is.False, "ztext check");
        }
    }
}
