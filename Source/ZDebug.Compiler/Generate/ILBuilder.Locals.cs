using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ZDebug.Compiler.Generate
{
    public sealed partial class ILBuilder
    {
        private class LocalWrapper : ILocal
        {
            protected readonly ILBuilder builder;
            protected readonly LocalBuilder local;
            private bool released;

            public LocalWrapper(ILBuilder builder, LocalBuilder local)
            {
                this.builder = builder;
                this.local = local;
            }

            public void Renew()
            {
                released = false;
            }

            public void Release()
            {
                if (released)
                {
                    throw new ZCompilerException("Attempted to release local that has already been released.");
                }

                builder.ReleaseLocal(this);
                released = true;
            }

            void IDisposable.Dispose()
            {
                Release();
            }

            public void Load()
            {
                if (released)
                {
                    throw new ZCompilerException("Attempted to load local that has already been released.");
                }

                builder.Emit(OpCodes.Ldloc, local);
            }

            public void LoadAddress()
            {
                if (released)
                {
                    throw new ZCompilerException("Attempted to load local that has already been released.");
                }

                builder.Emit(OpCodes.Ldloca_S, local);
            }

            public void LoadAndBox()
            {
                Load();

                if (local.LocalType.IsValueType)
                {
                    builder.Emit(OpCodes.Box, local.LocalType);
                }
            }

            public void Store()
            {
                if (released)
                {
                    throw new ZCompilerException("Attempted to store local that has already been released.");
                }

                builder.Emit(OpCodes.Stloc, local);
            }

            public int Index
            {
                get { return local.LocalIndex; }
            }

            public Type Type
            {
                get { return local.LocalType; }
            }
        }

        private class ArrayLocalWrapper : LocalWrapper, IArrayLocal
        {
            private readonly Type elementType;
            private readonly OpCode loadOpCode;
            private readonly OpCode storeOpCode;

            public ArrayLocalWrapper(ILBuilder builder, LocalBuilder local)
                : base(builder, local)
            {
                this.elementType = local.LocalType.GetElementType();
                this.loadOpCode = GetLoadOpCode(elementType);
                this.storeOpCode = GetStoreOpCode(elementType);
            }

            private static OpCode GetLoadOpCode(Type type)
            {
                if (type == typeof(int))
                {
                    return OpCodes.Ldelem_I4;
                }
                else if (type == typeof(ushort))
                {
                    return OpCodes.Ldelem_U2;
                }
                else if (type == typeof(byte))
                {
                    return OpCodes.Ldelem_U1;
                }
                else if (type.IsClass)
                {
                    return OpCodes.Ldelem_Ref;
                }
                else
                {
                    throw new ZCompilerException("Unsupported array type: " + type.FullName);
                }
            }

            private static OpCode GetStoreOpCode(Type type)
            {
                if (type == typeof(int))
                {
                    return OpCodes.Stelem_I4;
                }
                else if (type == typeof(ushort))
                {
                    return OpCodes.Stelem_I2;
                }
                else if (type == typeof(byte))
                {
                    return OpCodes.Stelem_I1;
                }
                else if (type.IsClass)
                {
                    return OpCodes.Stelem_Ref;
                }
                else
                {
                    throw new ZCompilerException("Unsupported array type: " + type.FullName);
                }
            }

            public void Create(int length)
            {
                builder.Load(length);
                builder.Emit(OpCodes.Newarr, elementType);
                this.Store();
            }

            public void Create(ILocal length)
            {
                length.Load();
                builder.Emit(OpCodes.Newarr, elementType);
                this.Store();
            }

            public void LoadLength()
            {
                this.Load();
                builder.Emit(OpCodes.Ldlen);
            }

            public void LoadElement(CodeBuilder loadIndex)
            {
                this.Load();
                loadIndex();
                builder.Emit(loadOpCode);
            }

            public void StoreElement(CodeBuilder loadIndex, CodeBuilder loadValue)
            {
                this.Load();
                loadIndex();
                loadValue();
                builder.Emit(storeOpCode);
            }
        }

        private static Func<ILBuilder, LocalBuilder, LocalWrapper> CreateLocal = (l, b) => new LocalWrapper(l, b);
        private static Func<ILBuilder, LocalBuilder, ArrayLocalWrapper> CreateArrayLocal = (l, b) => new ArrayLocalWrapper(l, b);

        private TWrapper AllocateLocal<T, TWrapper>(Func<ILBuilder, LocalBuilder, TWrapper> createWrapper) where TWrapper : LocalWrapper
        {
            var type = typeof(T);
            Stack<ILocal> stack;
            if (!locals.TryGetValue(type, out stack))
            {
                return createWrapper(this, DeclareLocal(type));
            }

            if (stack.Count == 0)
            {
                return createWrapper(this, DeclareLocal(type));
            }

            var result = (TWrapper)stack.Pop();
            result.Renew();
            return result;
        }

        private void ReleaseLocal(LocalWrapper local)
        {
            Stack<ILocal> stack;
            if (!locals.TryGetValue(local.Type, out stack))
            {
                stack = new Stack<ILocal>();
                locals.Add(local.Type, stack);
            }

            stack.Push(local);
        }

        public ILocal NewLocal<T>()
        {
            return AllocateLocal<T, LocalWrapper>(CreateLocal);
        }

        public ILocal NewLocal<T>(CodeBuilder loadValue)
        {
            var local = AllocateLocal<T, LocalWrapper>(CreateLocal);
            loadValue();
            local.Store();
            return local;
        }

        public ILocal NewLocal(int value)
        {
            var local = AllocateLocal<int, LocalWrapper>(CreateLocal);
            Load(value);
            local.Store();
            return local;
        }

        public IArrayLocal NewArrayLocal<T>()
        {
            return AllocateLocal<T[], ArrayLocalWrapper>(CreateArrayLocal);
        }

        public IArrayLocal NewArrayLocal<T>(int length)
        {
            var local = AllocateLocal<T[], ArrayLocalWrapper>(CreateArrayLocal);
            local.Create(length);

            return local;
        }

        public IArrayLocal NewArrayLocal<T>(CodeBuilder loadValue)
        {
            var local = AllocateLocal<T[], ArrayLocalWrapper>(CreateArrayLocal);
            loadValue();
            local.Store();
            return local;
        }
    }
}
