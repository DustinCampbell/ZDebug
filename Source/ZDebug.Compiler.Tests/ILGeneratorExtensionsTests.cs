using System;
using System.Reflection.Emit;
using NUnit.Framework;

namespace ZDebug.Compiler.Tests
{
    [TestFixture]
    public class ILGeneratorExtensionsTests
    {
        [Test]
        public void TestFormatString1()
        {
            var dm = new DynamicMethod("Foo", typeof(string), new Type[0]);
            var il = dm.GetILGenerator();

            var loc1 = il.DeclareLocal("world");

            il.FormatString("Hello {0}!", loc1);

            il.Emit(OpCodes.Ret);

            var del = (Func<string>)dm.CreateDelegate(typeof(Func<string>));
            var res = del();

            Assert.That(res, Is.EqualTo("Hello world!"));
        }

        [Test]
        public void TestFormatString2()
        {
            var dm = new DynamicMethod("Foo", typeof(string), new Type[0]);
            var il = dm.GetILGenerator();

            var loc1 = il.DeclareLocal(42);
            il.FormatString("Hello {0}!", loc1);

            il.Emit(OpCodes.Ret);

            var del = (Func<string>)dm.CreateDelegate(typeof(Func<string>));
            var res = del();

            Assert.That(res, Is.EqualTo("Hello 42!"));
        }

        [Test]
        public void TestFormatString3()
        {
            var dm = new DynamicMethod("Foo", typeof(string), new Type[0]);
            var il = dm.GetILGenerator();

            var loc1 = il.DeclareLocal("world");
            var loc2 = il.DeclareLocal(42);

            il.FormatString("Hello {0} {1}!", loc1, loc2);

            il.Emit(OpCodes.Ret);

            var del = (Func<string>)dm.CreateDelegate(typeof(Func<string>));
            var res = del();

            Assert.That(res, Is.EqualTo("Hello world 42!"));
        }

        [Test]
        public void TestFormatString4()
        {
            var dm = new DynamicMethod("Foo", typeof(string), new Type[0]);
            var il = dm.GetILGenerator();

            var loc1 = il.DeclareLocal("world");
            var loc2 = il.DeclareLocal(42);
            var loc3 = il.DeclareLocal(true);

            il.FormatString("Hello {0} {1} {2}!", loc1, loc2, loc3);

            il.Emit(OpCodes.Ret);

            var del = (Func<string>)dm.CreateDelegate(typeof(Func<string>));
            var res = del();

            Assert.That(res, Is.EqualTo("Hello world 42 True!"));
        }

        [Test]
        public void TestFormatString5()
        {
            var dm = new DynamicMethod("Foo", typeof(string), new Type[0]);
            var il = dm.GetILGenerator();

            var loc1 = il.DeclareLocal("world");
            var loc2 = il.DeclareLocal(42);
            var loc3 = il.DeclareLocal(true);
            var loc4 = il.DeclareLocal("Foo");

            il.FormatString("Hello {0} {1} {2} {3}!", loc1, loc2, loc3, loc4);

            il.Emit(OpCodes.Ret);

            var del = (Func<string>)dm.CreateDelegate(typeof(Func<string>));
            var res = del();

            Assert.That(res, Is.EqualTo("Hello world 42 True Foo!"));
        }
    }
}
