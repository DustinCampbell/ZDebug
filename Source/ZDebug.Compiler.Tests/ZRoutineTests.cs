using NUnit.Framework;
using ZDebug.Compiler.Tests.Utilities;
using ZDebug.Core.Routines;

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

        //[Test]
        //public void CreateZork1Routine50d0AndCompile()
        //{
        //    var memory = ZCode.ReadZork1();
        //    var routine = ZRoutine.Create(0x50d0, memory);

        //    Assert.That(routine.Address, Is.EqualTo(0x50d0));
        //    Assert.That(routine.Locals.Length, Is.EqualTo(2));
        //    Assert.That(routine.Instructions.Length, Is.EqualTo(1));

        //    var zmachine = new ZMachine(memory);

        //    Assert.That(zmachine.GlobalVariableTableAddress, Is.EqualTo(0x2271));

        //    var zcompilerResult = ZCompiler.Compile(routine, zmachine);
        //    var res = zcompilerResult.Code(new ushort[0]);

        //    Assert.That(res, Is.EqualTo(0));
        //}

        //[Test]
        //public void CreateZork1Routine5472AndCompile1()
        //{
        //    var memory = ZCode.ReadZork1();
        //    var routine = ZRoutine.Create(0x5472, memory);

        //    Assert.That(routine.Address, Is.EqualTo(0x5472));
        //    Assert.That(routine.Locals.Length, Is.EqualTo(3));
        //    Assert.That(routine.Instructions.Length, Is.EqualTo(3));

        //    var zmachine = new ZMachine(memory);

        //    Assert.That(zmachine.GlobalVariableTableAddress, Is.EqualTo(0x2271));

        //    var zcompilerResult = ZCompiler.Compile(routine, zmachine);
        //    var res = zcompilerResult.Code(new ushort[] { 0x8010, 0xffff });

        //    Assert.That(res, Is.EqualTo(0x2497));
        //}

        //[Test]
        //public void CreateZork1Routine5472AndCompile2()
        //{
        //    var memory = ZCode.ReadZork1();
        //    var routine = ZRoutine.Create(0x5472, memory);

        //    Assert.That(routine.Address, Is.EqualTo(0x5472));
        //    Assert.That(routine.Locals.Length, Is.EqualTo(3));
        //    Assert.That(routine.Instructions.Length, Is.EqualTo(3));

        //    var zmachine = new ZMachine(memory);

        //    Assert.That(zmachine.GlobalVariableTableAddress, Is.EqualTo(0x2271));

        //    var zcompilerResult = ZCompiler.Compile(routine, zmachine);
        //    zcompilerResult.Code(new ushort[] { 0x8010, 0xffff });
        //    var res = zcompilerResult.Code(new ushort[] { 0x807c, 0xffff });

        //    Assert.That(res, Is.EqualTo(0x2491));
        //}

        //[Test]
        //public void CreateZork1Routine5472AndCompile3()
        //{
        //    var memory = ZCode.ReadZork1();
        //    var routine = ZRoutine.Create(0x5472, memory);

        //    Assert.That(routine.Address, Is.EqualTo(0x5472));
        //    Assert.That(routine.Locals.Length, Is.EqualTo(3));
        //    Assert.That(routine.Instructions.Length, Is.EqualTo(3));

        //    var zmachine = new ZMachine(memory);

        //    Assert.That(zmachine.GlobalVariableTableAddress, Is.EqualTo(0x2271));

        //    var zcompilerResult = ZCompiler.Compile(routine, zmachine);
        //    zcompilerResult.Code(new ushort[] { 0x8010, 0xffff });
        //    zcompilerResult.Code(new ushort[] { 0x807c, 0xffff });
        //    var res = zcompilerResult.Code(new ushort[] { 0x80f0, 0xffff });

        //    Assert.That(res, Is.EqualTo(0x248b));
        //}

        //[Test]
        //public void CreateZork1Routine5486AndCompile()
        //{
        //    var memory = ZCode.ReadZork1();
        //    var routine = ZRoutine.Create(0x5486, memory);

        //    Assert.That(routine.Address, Is.EqualTo(0x5486));
        //    Assert.That(routine.Locals.Length, Is.EqualTo(5));
        //    Assert.That(routine.Instructions.Length, Is.EqualTo(14));

        //    var zmachine = new ZMachine(memory);

        //    Assert.That(zmachine.GlobalVariableTableAddress, Is.EqualTo(0x2271));

        //    var zcompilerResult = ZCompiler.Compile(routine, zmachine);
        //    var res = zcompilerResult.Code(new ushort[] { 0x8010 });

        //    Assert.That(res, Is.EqualTo(0x2497));
        //}
    }
}
