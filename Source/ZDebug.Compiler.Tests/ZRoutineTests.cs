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

        [Test]
        public void CreateZork1MainRoutineAndCompile()
        {
            var memory = ZCode.ReadZork1();
            var pc = memory.ReadWord(0x06);
            var mainAddress = pc - 1;

            var routine = ZRoutine.Create(mainAddress, memory);

            Assert.That(routine.Address, Is.EqualTo(0x4f04));
            Assert.That(routine.Locals.Length, Is.EqualTo(0));
            Assert.That(routine.Instructions.Length, Is.EqualTo(32));

            var zmachine = new ZMachine(memory);

            Assert.That(zmachine.GlobalVariableTableAddress, Is.EqualTo(0x2271));

            var zcode = ZCompiler.Compile(routine, zmachine);
        }

        [Test]
        public void CreateZork1Routine50d0AndCompile()
        {
            var memory = ZCode.ReadZork1();
            var routine = ZRoutine.Create(0x50d0, memory);

            Assert.That(routine.Address, Is.EqualTo(0x50d0));
            Assert.That(routine.Locals.Length, Is.EqualTo(2));
            Assert.That(routine.Instructions.Length, Is.EqualTo(1));

            var zmachine = new ZMachine(memory);

            Assert.That(zmachine.GlobalVariableTableAddress, Is.EqualTo(0x2271));

            var zcode = ZCompiler.Compile(routine, zmachine);
            var res = zcode(new ushort[0]);

            Assert.That(res, Is.EqualTo(0));
        }

        [Test]
        public void CreateZork1Routine5486AndCompile()
        {
            var memory = ZCode.ReadZork1();
            var routine = ZRoutine.Create(0x5486, memory);

            Assert.That(routine.Address, Is.EqualTo(0x5486));
            Assert.That(routine.Locals.Length, Is.EqualTo(5));
            Assert.That(routine.Instructions.Length, Is.EqualTo(14));

            var zmachine = new ZMachine(memory);

            Assert.That(zmachine.GlobalVariableTableAddress, Is.EqualTo(0x2271));

            var zcode = ZCompiler.Compile(routine, zmachine);
            var res = zcode(new ushort[] { 0x8010 });

            Assert.That(res, Is.EqualTo(0x2497));
        }
    }
}
