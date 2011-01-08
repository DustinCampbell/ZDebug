using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Reflection.Emit;

namespace ZDebug.Compiler.Tests
{
    [TestFixture]
    public class ZCompilerTests
    {
        [Test]
        public void TestCheckStackEmpty1()
        {
            var del = (Action)DynamicMethodHelpers.Create(typeof(Action), il =>
            {
                var sp = il.DeclareLocal(1);
                il.CheckStackEmpty(sp);
            });

            Assert.That(() => del(), Throws.Nothing);
        }

        [Test]
        public void TestCheckStackEmpty2()
        {
            var del = (Action)DynamicMethodHelpers.Create(typeof(Action), il =>
            {
                var sp = il.DeclareLocal(0);
                il.CheckStackEmpty(sp);
            });

            Assert.That(() => del(), Throws.TypeOf<ZMachineException>().With.Property("Message").EqualTo("Stack is empty."));
        }

        [Test]
        public void TestCheckStackFull1()
        {
            var del = (Action)DynamicMethodHelpers.Create(typeof(Action), il =>
            {
                var sp = il.DeclareLocal(ZCompiler.STACK_SIZE - 1);
                il.CheckStackFull(sp);
            });

            Assert.That(() => del(), Throws.Nothing);
        }

        [Test]
        public void TestCheckStackFull2()
        {
            var del = (Action)DynamicMethodHelpers.Create(typeof(Action), il =>
            {
                var sp = il.DeclareLocal(ZCompiler.STACK_SIZE);
                il.CheckStackFull(sp);
            });

            Assert.That(() => del(), Throws.TypeOf<ZMachineException>().With.Property("Message").EqualTo("Stack is full."));
        }

        [Test]
        public void TestPopStack1()
        {
            var del = (Func<ushort>)DynamicMethodHelpers.Create(typeof(Func<ushort>), il =>
            {
                var stack = il.DeclareLocal(new ushort[] { 1, 2, 3 });
                var sp = il.DeclareLocal(3);
                var result = il.DeclareLocal<ushort>();

                il.PopStack(stack, sp, result);

                il.Emit(OpCodes.Ldloc, result);
            });

            var res = del();

            Assert.That(res, Is.EqualTo(3));
        }

        [Test]
        public void TestPopStack2()
        {
            var del = (Func<ushort>)DynamicMethodHelpers.Create(typeof(Func<ushort>), il =>
            {
                var stack = il.DeclareLocal(new ushort[] { 1, 2, 3 });
                var sp = il.DeclareLocal(2);
                var result = il.DeclareLocal<ushort>();

                il.PopStack(stack, sp, result);

                il.Emit(OpCodes.Ldloc, result);
            });

            var res = del();

            Assert.That(res, Is.EqualTo(2));
        }

        [Test]
        public void TestPopStack3()
        {
            var del = (Func<ushort>)DynamicMethodHelpers.Create(typeof(Func<ushort>), il =>
            {
                var stack = il.DeclareLocal(new ushort[] { 1, 2, 3 });
                var sp = il.DeclareLocal(1);
                var result = il.DeclareLocal<ushort>();

                il.PopStack(stack, sp, result);

                il.Emit(OpCodes.Ldloc, result);
            });

            var res = del();

            Assert.That(res, Is.EqualTo(1));
        }

        [Test]
        public void TestPopStack4()
        {
            var del = (Func<ushort>)DynamicMethodHelpers.Create(typeof(Func<ushort>), il =>
            {
                var stack = il.DeclareLocal(new ushort[] { 1, 2, 3 });
                var sp = il.DeclareLocal(0);
                var result = il.DeclareLocal<ushort>();

                il.PopStack(stack, sp, result);

                il.Emit(OpCodes.Ldloc, result);
            });

            Assert.That(() => del(), Throws.TypeOf<ZMachineException>().With.Property("Message").EqualTo("Stack is empty."));
        }
    }
}
