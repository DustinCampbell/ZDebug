using System;
using System.Linq;
using System.Reflection.Emit;
using NUnit.Framework;

namespace ZDebug.Compiler.Tests
{
    [TestFixture]
    public class ILGeneratorExtensionsTests
    {
        private Delegate CreateDynamicMethod(Type delegateType, Action<ILGenerator> codeGenerator)
        {
            if (delegateType.BaseType != typeof(MulticastDelegate))
            {
                throw new ArgumentException("'delegateType' does not represent a valid delegate", "delegateType");
            }

            var invokeMethod = delegateType.GetMethod("Invoke");
            if (invokeMethod == null)
            {
                throw new ArgumentException("'delegateType' does not represent a valid delegate", "delegateType");
            }

            var returnType = invokeMethod.ReturnType;
            var parameterTypes = Array.ConvertAll(invokeMethod.GetParameters(), pi => pi.ParameterType);

            var dm = new DynamicMethod("TestMethod", returnType, parameterTypes);
            var il = dm.GetILGenerator();

            codeGenerator(il);

            il.Emit(OpCodes.Ret);

            return dm.CreateDelegate(delegateType);
        }

        [Test]
        public void TestFormatString1()
        {
            var del = (Func<string>)CreateDynamicMethod(typeof(Func<string>), il =>
            {
                var loc1 = il.DeclareLocal("world");

                il.FormatString("Hello {0}!", loc1);
            });

            var res = del();

            Assert.That(res, Is.EqualTo("Hello world!"));
        }

        [Test]
        public void TestFormatString2()
        {
            var del = (Func<string>)CreateDynamicMethod(typeof(Func<string>), il =>
            {
                var loc1 = il.DeclareLocal(42);

                il.FormatString("Hello {0}!", loc1);
            });

            var res = del();

            Assert.That(res, Is.EqualTo("Hello 42!"));
        }

        [Test]
        public void TestFormatString3()
        {
            var del = (Func<string>)CreateDynamicMethod(typeof(Func<string>), il =>
            {
                var loc1 = il.DeclareLocal("world");
                var loc2 = il.DeclareLocal(42);

                il.FormatString("Hello {0} {1}!", loc1, loc2);
            });

            var res = del();

            Assert.That(res, Is.EqualTo("Hello world 42!"));
        }

        [Test]
        public void TestFormatString4()
        {
            var del = (Func<string>)CreateDynamicMethod(typeof(Func<string>), il =>
            {
                var loc1 = il.DeclareLocal("world");
                var loc2 = il.DeclareLocal(42);
                var loc3 = il.DeclareLocal(true);

                il.FormatString("Hello {0} {1} {2}!", loc1, loc2, loc3);
            });

            var res = del();

            Assert.That(res, Is.EqualTo("Hello world 42 True!"));
        }

        [Test]
        public void TestFormatString5()
        {
            var del = (Func<string>)CreateDynamicMethod(typeof(Func<string>), il =>
            {
                var loc1 = il.DeclareLocal("world");
                var loc2 = il.DeclareLocal(42);
                var loc3 = il.DeclareLocal(true);
                var loc4 = il.DeclareLocal("Foo");

                il.FormatString("Hello {0} {1} {2} {3}!", loc1, loc2, loc3, loc4);
            });

            var res = del();

            Assert.That(res, Is.EqualTo("Hello world 42 True Foo!"));
        }

        [Test]
        public void TestThrowException1()
        {
            var del = (Action)CreateDynamicMethod(typeof(Action), il =>
            {
                il.ThrowException("Oh dear!");
            });

            Assert.That(() => del(), Throws.TypeOf<ZMachineException>().With.Property("Message").EqualTo("Oh dear!"));
        }

        [Test]
        public void TestThrowException2()
        {
            var del = (Action)CreateDynamicMethod(typeof(Action), il =>
            {
                var loc1 = il.DeclareLocal("Oh dear");
                il.ThrowException("{0}!", loc1);
            });

            Assert.That(() => del(), Throws.TypeOf<ZMachineException>().With.Property("Message").EqualTo("Oh dear!"));
        }

        [Test]
        public void TestThrowException3()
        {
            var del = (Action)CreateDynamicMethod(typeof(Action), il =>
            {
                var loc1 = il.DeclareLocal("Oh");
                var loc2 = il.DeclareLocal("dear");
                il.ThrowException("{0} {1}!", loc1, loc2);
            });

            Assert.That(() => del(), Throws.TypeOf<ZMachineException>().With.Property("Message").EqualTo("Oh dear!"));
        }

        [Test]
        public void TestThrowException4()
        {
            var del = (Action)CreateDynamicMethod(typeof(Action), il =>
            {
                var loc1 = il.DeclareLocal("Oh");
                var loc2 = il.DeclareLocal(" ");
                var loc3 = il.DeclareLocal("dear");
                il.ThrowException("{0}{1}{2}!", loc1, loc2, loc3);
            });

            Assert.That(() => del(), Throws.TypeOf<ZMachineException>().With.Property("Message").EqualTo("Oh dear!"));
        }

        [Test]
        public void TestThrowException5()
        {
            var del = (Action)CreateDynamicMethod(typeof(Action), il =>
            {
                var loc1 = il.DeclareLocal("Oh");
                var loc2 = il.DeclareLocal(" ");
                var loc3 = il.DeclareLocal("dear");
                var loc4 = il.DeclareLocal("!");
                il.ThrowException("{0}{1}{2}{3}", loc1, loc2, loc3, loc4);
            });

            Assert.That(() => del(), Throws.TypeOf<ZMachineException>().With.Property("Message").EqualTo("Oh dear!"));
        }
    }
}
