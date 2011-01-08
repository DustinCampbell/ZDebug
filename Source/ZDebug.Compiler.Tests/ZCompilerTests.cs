using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Reflection.Emit;
using ZDebug.Compiler.Tests.Utilities;

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

        [Test]
        public void TestPopStack5()
        {
            var del = (Func<ushort[]>)DynamicMethodHelpers.Create(typeof(Func<ushort[]>), il =>
            {
                var stack = il.DeclareLocal(new ushort[] { 1, 2, 3 });
                var sp = il.DeclareLocal(3);
                var result = il.DeclareLocal<ushort>();
                var results = il.DeclareArrayLocal<ushort>(3);

                for (int i = 0; i < 3; i++)
                {
                    il.Emit(OpCodes.Ldloc, results);
                    il.Emit(OpCodes.Ldc_I4, i);

                    il.PopStack(stack, sp, result);
                    il.Emit(OpCodes.Ldloc, result);

                    il.Emit(OpCodes.Stelem_I2);
                }

                il.Emit(OpCodes.Ldloc, results);
            });

            var res = del();

            Assert.That(res, Is.EqualTo(new ushort[] { 3, 2, 1 }));
        }

        [Test]
        public void TestPeekStack1()
        {
            var del = (Func<ushort>)DynamicMethodHelpers.Create(typeof(Func<ushort>), il =>
            {
                var stack = il.DeclareLocal(new ushort[] { 1, 2, 3 });
                var sp = il.DeclareLocal(3);
                var result = il.DeclareLocal<ushort>();

                il.PeekStack(stack, sp, result);

                il.Emit(OpCodes.Ldloc, result);
            });

            var res = del();

            Assert.That(res, Is.EqualTo(3));
        }

        [Test]
        public void TestPeekStack2()
        {
            var del = (Func<ushort>)DynamicMethodHelpers.Create(typeof(Func<ushort>), il =>
            {
                var stack = il.DeclareLocal(new ushort[] { 1, 2, 3 });
                var sp = il.DeclareLocal(2);
                var result = il.DeclareLocal<ushort>();

                il.PeekStack(stack, sp, result);

                il.Emit(OpCodes.Ldloc, result);
            });

            var res = del();

            Assert.That(res, Is.EqualTo(2));
        }

        [Test]
        public void TestPeekStack3()
        {
            var del = (Func<ushort>)DynamicMethodHelpers.Create(typeof(Func<ushort>), il =>
            {
                var stack = il.DeclareLocal(new ushort[] { 1, 2, 3 });
                var sp = il.DeclareLocal(1);
                var result = il.DeclareLocal<ushort>();

                il.PeekStack(stack, sp, result);

                il.Emit(OpCodes.Ldloc, result);
            });

            var res = del();

            Assert.That(res, Is.EqualTo(1));
        }

        [Test]
        public void TestPeekStack4()
        {
            var del = (Func<ushort>)DynamicMethodHelpers.Create(typeof(Func<ushort>), il =>
            {
                var stack = il.DeclareLocal(new ushort[] { 1, 2, 3 });
                var sp = il.DeclareLocal(0);
                var result = il.DeclareLocal<ushort>();

                il.PeekStack(stack, sp, result);

                il.Emit(OpCodes.Ldloc, result);
            });

            Assert.That(() => del(), Throws.TypeOf<ZMachineException>().With.Property("Message").EqualTo("Stack is empty."));
        }

        [Test]
        public void TestPeekStack5()
        {
            var del = (Func<ushort[]>)DynamicMethodHelpers.Create(typeof(Func<ushort[]>), il =>
            {
                var stack = il.DeclareLocal(new ushort[] { 1, 2, 3 });
                var sp = il.DeclareLocal(3);
                var result = il.DeclareLocal<ushort>();
                var results = il.DeclareArrayLocal<ushort>(3);

                for (int i = 0; i < 3; i++)
                {
                    il.Emit(OpCodes.Ldloc, results);
                    il.Emit(OpCodes.Ldc_I4, i);

                    il.PeekStack(stack, sp, result);
                    il.Emit(OpCodes.Ldloc, result);

                    il.Emit(OpCodes.Stelem_I2);
                }

                il.Emit(OpCodes.Ldloc, results);
            });

            var res = del();

            Assert.That(res, Is.EqualTo(new ushort[] { 3, 3, 3 }));
        }

        [Test]
        public void TestPushStack1()
        {
            var del = (Action)DynamicMethodHelpers.Create(typeof(Action), il =>
            {
                var stack = il.DeclareArrayLocal<ushort>(ZCompiler.STACK_SIZE);
                var sp = il.DeclareLocal(ZCompiler.STACK_SIZE);
                var value = il.DeclareLocal(1);

                il.PushStack(stack, sp, value);
            });

            Assert.That(() => del(), Throws.TypeOf<ZMachineException>().With.Property("Message").EqualTo("Stack is full."));
        }

        [Test]
        public void TestPushStack2()
        {
            var del = (Func<ushort[]>)DynamicMethodHelpers.Create(typeof(Func<ushort[]>), il =>
            {
                var stack = il.DeclareLocal(new ushort[] { 1, 2, 0 });
                var sp = il.DeclareLocal(2);
                var value = il.DeclareLocal(3);

                il.PushStack(stack, sp, value);

                il.Emit(OpCodes.Ldloc, stack);
            });

            var res = del();

            Assert.That(res, Is.EqualTo(new ushort[] { 1, 2, 3 }));
        }

        [Test]
        public void TestPushStack3()
        {
            var del = (Func<ushort[]>)DynamicMethodHelpers.Create(typeof(Func<ushort[]>), il =>
            {
                var stack = il.DeclareLocal(new ushort[] { 1, 0, 3 });
                var sp = il.DeclareLocal(1);
                var value = il.DeclareLocal(2);

                il.PushStack(stack, sp, value);

                il.Emit(OpCodes.Ldloc, stack);
            });

            var res = del();

            Assert.That(res, Is.EqualTo(new ushort[] { 1, 2, 3 }));
        }

        [Test]
        public void TestPushStack4()
        {
            var del = (Func<ushort[]>)DynamicMethodHelpers.Create(typeof(Func<ushort[]>), il =>
            {
                var stack = il.DeclareArrayLocal<ushort>(3);
                var sp = il.DeclareLocal(0);
                var value = il.DeclareLocal<ushort>();

                for (int i = 1; i <= 3; i++)
                {
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Stloc, value);

                    il.PushStack(stack, sp, value);
                }

                il.Emit(OpCodes.Ldloc, stack);
            });

            var res = del();

            Assert.That(res, Is.EqualTo(new ushort[] { 1, 2, 3 }));
        }

        [Test]
        public void TestSetStackTop1()
        {
            var del = (Action)DynamicMethodHelpers.Create(typeof(Action), il =>
            {
                var stack = il.DeclareArrayLocal<ushort>(ZCompiler.STACK_SIZE);
                var sp = il.DeclareLocal(0);
                var value = il.DeclareLocal(1);

                il.SetStackTop(stack, sp, value);
            });

            Assert.That(() => del(), Throws.TypeOf<ZMachineException>().With.Property("Message").EqualTo("Stack is empty."));
        }

        [Test]
        public void TestSetStackTop2()
        {
            var del = (Func<ushort[]>)DynamicMethodHelpers.Create(typeof(Func<ushort[]>), il =>
            {
                var stack = il.DeclareLocal(new ushort[] { 1, 2, 2 });
                var sp = il.DeclareLocal(3);
                var value = il.DeclareLocal(3);

                il.SetStackTop(stack, sp, value);

                il.Emit(OpCodes.Ldloc, stack);
            });

            var res = del();

            Assert.That(res, Is.EqualTo(new ushort[] { 1, 2, 3 }));
        }

        [Test]
        public void TestSetStackTop3()
        {
            var del = (Func<ushort[]>)DynamicMethodHelpers.Create(typeof(Func<ushort[]>), il =>
            {
                var stack = il.DeclareLocal(new ushort[] { 1, 1, 3 });
                var sp = il.DeclareLocal(2);
                var value = il.DeclareLocal(2);

                il.SetStackTop(stack, sp, value);

                il.Emit(OpCodes.Ldloc, stack);
            });

            var res = del();

            Assert.That(res, Is.EqualTo(new ushort[] { 1, 2, 3 }));
        }
    }
}
