using ZDebug.Core.Routines;

namespace ZDebug.Compiler
{
    internal sealed class ZRoutineCall
    {
        public readonly CompiledZMachine Machine;
        public readonly ZRoutine Routine;

        private ZCompilerResult compilationResult;

        public ZRoutineCall(ZRoutine routine, CompiledZMachine machine)
        {
            this.Machine = machine;
            this.Routine = routine;
        }

        public void Compile()
        {
            if (compilationResult == null)
            {
                compilationResult = Machine.Compile(Routine);
            }
        }

        public ushort Invoke0()
        {
            Compile();

            var locals = Machine.GetLocalArray(Routine);
            try
            {
                return compilationResult.Code(compilationResult.Calls, locals, 0);
            }
            finally
            {
                Machine.ReleaseLocalArray(locals);
            }
        }

        public ushort Invoke1(ushort arg0)
        {
            Compile();

            var locals = Machine.GetLocalArray(Routine);
            try
            {
                locals[0] = arg0;

                return compilationResult.Code(compilationResult.Calls, locals, 1);
            }
            finally
            {
                Machine.ReleaseLocalArray(locals);
            }
        }

        public ushort Invoke2(ushort arg0, ushort arg1)
        {
            Compile();

            var locals = Machine.GetLocalArray(Routine);
            try
            {
                locals[0] = arg0;
                locals[1] = arg1;

                return compilationResult.Code(compilationResult.Calls, locals, 2);
            }
            finally
            {
                Machine.ReleaseLocalArray(locals);
            }
        }

        public ushort Invoke3(ushort arg0, ushort arg1, ushort arg2)
        {
            Compile();

            var locals = Machine.GetLocalArray(Routine);
            try
            {
                locals[0] = arg0;
                locals[1] = arg1;
                locals[2] = arg2;

                return compilationResult.Code(compilationResult.Calls, locals, 3);
            }
            finally
            {
                Machine.ReleaseLocalArray(locals);
            }
        }

        public ushort Invoke4(ushort arg0, ushort arg1, ushort arg2, ushort arg3)
        {
            Compile();

            var locals = Machine.GetLocalArray(Routine);
            try
            {
                locals[0] = arg0;
                locals[1] = arg1;
                locals[2] = arg2;
                locals[3] = arg3;

                return compilationResult.Code(compilationResult.Calls, locals, 4);
            }
            finally
            {
                Machine.ReleaseLocalArray(locals);
            }
        }

        public ushort Invoke5(ushort arg0, ushort arg1, ushort arg2, ushort arg3, ushort arg4)
        {
            Compile();

            var locals = Machine.GetLocalArray(Routine);
            try
            {
                locals[0] = arg0;
                locals[1] = arg1;
                locals[2] = arg2;
                locals[3] = arg3;
                locals[4] = arg4;

                return compilationResult.Code(compilationResult.Calls, locals, 5);
            }
            finally
            {
                Machine.ReleaseLocalArray(locals);
            }
        }

        public ushort Invoke6(ushort arg0, ushort arg1, ushort arg2, ushort arg3, ushort arg4, ushort arg5)
        {
            Compile();

            var locals = Machine.GetLocalArray(Routine);
            try
            {
                locals[0] = arg0;
                locals[1] = arg1;
                locals[2] = arg2;
                locals[3] = arg3;
                locals[4] = arg4;
                locals[5] = arg5;

                return compilationResult.Code(compilationResult.Calls, locals, 6);
            }
            finally
            {
                Machine.ReleaseLocalArray(locals);
            }
        }

        public ushort Invoke7(ushort arg0, ushort arg1, ushort arg2, ushort arg3, ushort arg4, ushort arg5, ushort arg6)
        {
            Compile();

            var locals = Machine.GetLocalArray(Routine);
            try
            {
                locals[0] = arg0;
                locals[1] = arg1;
                locals[2] = arg2;
                locals[3] = arg3;
                locals[4] = arg4;
                locals[5] = arg5;
                locals[6] = arg6;

                return compilationResult.Code(compilationResult.Calls, locals, 7);
            }
            finally
            {
                Machine.ReleaseLocalArray(locals);
            }
        }
    }
}
