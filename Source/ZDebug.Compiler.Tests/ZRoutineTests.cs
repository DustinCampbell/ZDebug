using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ZDebug.Compiler.Tests.Utilities;

namespace ZDebug.Compiler.Tests
{
    [TestFixture]
    public class ZRoutineTests
    {
        [Test]
        public void CreateZork1MainRoutine()
        {
            var memory = ZCode.ReadZork1();
            var pc = memory.ReadWord(0x06);
            var mainAddress = pc - 1;

            var mainRoutine = ZRoutine.Create(mainAddress, memory);

            Assert.That(mainRoutine.Address, Is.EqualTo(0x4f04));
            Assert.That(mainRoutine.Locals.Length, Is.EqualTo(0));
            Assert.That(mainRoutine.Instructions.Length, Is.EqualTo(32));
        }
    }
}
