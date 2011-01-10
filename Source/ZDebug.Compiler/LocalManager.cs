using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;

namespace ZDebug.Compiler
{
    public partial class LocalManager
    {
        private readonly ILGenerator il;
        private readonly Dictionary<Type, Stack<LocalBuilder>> temps = new Dictionary<Type,Stack<LocalBuilder>>();

        public LocalManager(ILGenerator il)
        {
            this.il = il;
        }

        public LocalBuilder Allocate<T>()
        {
            var type = typeof(T);
            Stack<LocalBuilder> stack;
            if (!temps.TryGetValue(type, out stack))
            {
                return il.DeclareLocal<T>();
            }

            if (stack.Count == 0)
            {
                return il.DeclareLocal<T>();
            }

            return stack.Pop();
        }

        public void Release(LocalBuilder local)
        {
            Stack<LocalBuilder> stack;
            if (!temps.TryGetValue(local.LocalType, out stack))
            {
                stack = new Stack<LocalBuilder>();
                temps.Add(local.LocalType, stack);
            }

            stack.Push(local);
        }

        public Temp<T> AllocateTemp<T>()
        {
            return new Temp<T>(this);
        }
    }
}
