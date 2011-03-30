using System;
using System.Diagnostics;
using ZDebug.Compiler.Profiling;
using ZDebug.Core;
using ZDebug.Core.Basics;
using ZDebug.Core.Collections;
using ZDebug.Core.Execution;
using ZDebug.Core.Extensions;
using ZDebug.Core.Routines;
using ZDebug.Core.Text;

namespace ZDebug.Compiler
{
    public sealed partial class CompiledZMachine : ZMachine
    {
        internal const int STACK_SIZE = 32768;

        private readonly IZMachineProfiler profiler;
        private readonly bool precompile;
        private readonly bool debugging;

        // routine state
        private readonly ushort[] stack;
        private int sp;

        private int stackFrame;
        private readonly int[] stackFrames;
        private int sfp;

        private readonly ushort[] locals;
        private ushort localCount;

        private readonly ushort[] arguments;
        private ushort argumentCount;

        private int cacheMiss;

        private readonly ushort objectTableAddress;
        private readonly byte propertyDefaultsTableSize;
        private readonly ushort objectEntriesAddress;
        private readonly byte objectEntrySize;
        private readonly byte objectParentOffset;
        private readonly byte objectSiblingOffset;
        private readonly byte objectChildOffset;
        private readonly byte objectPropertyTableAddressOffset;
        private readonly byte objectAttributeByteCount;
        private readonly byte objectAttributeCount;

        private readonly ushort dictionaryAddress;

        private readonly int packResolution;
        private readonly int routinesOffset;
        private readonly int stringsOffset;

        private readonly ZRoutineTable routineTable;
        private readonly IntegerMap<ZRoutineCall> addressToRoutineCallMap;
        private readonly IntegerMap<ZCompilerResult> compilationResults;

        private int currentAddress = -1;
        private volatile bool inputReceived;
        private volatile bool stopping;

        public CompiledZMachine(Story story, bool precompile = false, bool debugging = false, IZMachineProfiler profiler = null)
            : base(story)
        {
            this.profiler = profiler;
            this.precompile = precompile;
            this.debugging = debugging;

            this.stack = new ushort[STACK_SIZE];
            this.sp = -1;

            this.stackFrame = -1;
            this.stackFrames = new int[STACK_SIZE];
            this.sfp = -1;

            this.locals = new ushort[15];
            this.localCount = 0;

            this.arguments = new ushort[7];
            this.argumentCount = 0;

            this.objectTableAddress = this.Memory.ReadWord(0x0a);
            this.propertyDefaultsTableSize = (byte)(this.Version < 4 ? 31 : 63);
            this.objectEntriesAddress = (ushort)(this.objectTableAddress + (this.propertyDefaultsTableSize * 2));
            this.objectEntrySize = (byte)(this.Version < 4 ? 9 : 14);
            this.objectParentOffset = (byte)(this.Version < 4 ? 4 : 6);
            this.objectSiblingOffset = (byte)(this.Version < 4 ? 5 : 8);
            this.objectChildOffset = (byte)(this.Version < 4 ? 6 : 10);
            this.objectPropertyTableAddressOffset = (byte)(this.Version < 4 ? 7 : 12);
            this.objectAttributeByteCount = (byte)(this.Version < 4 ? 4 : 6);
            this.objectAttributeCount = (byte)(this.Version < 4 ? 32 : 48);

            this.dictionaryAddress = this.Memory.ReadWord(0x08);

            this.packResolution = this.Version < 4 ? 2 : this.Version < 8 ? 4 : 8;
            this.routinesOffset = (this.Version >= 6 && this.Version <= 7) ? Memory.ReadWord(0x28) : 0;
            this.stringsOffset = (this.Version >= 6 && this.Version <= 7) ? Memory.ReadWord(0x2a) : 0;

            this.routineTable = new ZRoutineTable(story);
            this.addressToRoutineCallMap = new IntegerMap<ZRoutineCall>(8192);
            this.compilationResults = new IntegerMap<ZCompilerResult>(8192);

            if (this.precompile)
            {
                foreach (var routine in routineTable)
                {
                    GetRoutineCall(routine.Address);
                }

                this.precompile = false;
            }
        }

        private int GetMainRoutineAddress()
        {
            var mainAddress = this.Memory.ReadWord(0x06);
            if (this.Version != 6)
            {
                mainAddress--;
            }

            return mainAddress;
        }

        internal bool Verify()
        {
            return this.Story.ActualChecksum == Header.ReadChecksum(this.Memory);
        }

        internal void PushFrame()
        {
            // argument count
            // local variable values (reversed)
            // local variable count

            stackFrames[++sfp] = stackFrame;

            var stack = this.stack;
            var sp = this.sp;

            stack[++sp] = this.argumentCount;

            var localCount = this.localCount;
            var locals = this.locals;
            for (int i = localCount - 1; i >= 0; i--)
            {
                stack[++sp] = locals[i];
            }

            stack[++sp] = (ushort)localCount;

            this.stackFrame = sp;
            this.sp = sp;
        }

        internal unsafe void PushFrame0()
        {
            // argument count
            // local variable values (reversed)
            // local variable count

            stackFrames[++sfp] = stackFrame;

            var temp = this.sp;

            fixed (ushort* pStack = this.stack)
            {
                ushort* sp = pStack + temp;

                *++sp = this.argumentCount;
                *++sp = 0;
            }

            temp += 2;

            this.stackFrame = temp;
            this.sp = temp;
        }

        internal unsafe void PushFrame1()
        {
            // argument count
            // local variable values (reversed)
            // local variable count

            stackFrames[++sfp] = stackFrame;

            var temp = this.sp;

            fixed (ushort* pStack = this.stack)
            fixed (ushort* pLocals = this.locals)
            {
                ushort* sp = pStack + temp;
                ushort* lp = pLocals + 1;

                *++sp = this.argumentCount;
                *++sp = *--lp;
                *++sp = 1;
            }

            temp += 3;

            this.stackFrame = temp;
            this.sp = temp;
        }

        internal unsafe void PushFrame2()
        {
            // argument count
            // local variable values (reversed)
            // local variable count

            stackFrames[++sfp] = stackFrame;

            var temp = this.sp;

            fixed (ushort* pStack = this.stack)
            fixed (ushort* pLocals = this.locals)
            {
                ushort* sp = pStack + temp;
                ushort* lp = pLocals + 2;

                *++sp = this.argumentCount;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = 2;
            }

            temp += 4;

            this.stackFrame = temp;
            this.sp = temp;
        }

        internal unsafe void PushFrame3()
        {
            // argument count
            // local variable values (reversed)
            // local variable count

            stackFrames[++sfp] = stackFrame;

            var temp = this.sp;

            fixed (ushort* pStack = this.stack)
            fixed (ushort* pLocals = this.locals)
            {
                ushort* sp = pStack + temp;
                ushort* lp = pLocals + 3;

                *++sp = this.argumentCount;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = 3;
            }

            temp += 5;

            this.stackFrame = temp;
            this.sp = temp;
        }

        internal unsafe void PushFrame4()
        {
            // argument count
            // local variable values (reversed)
            // local variable count

            stackFrames[++sfp] = stackFrame;

            var temp = this.sp;

            fixed (ushort* pStack = this.stack)
            fixed (ushort* pLocals = this.locals)
            {
                ushort* sp = pStack + temp;
                ushort* lp = pLocals + 4;

                *++sp = this.argumentCount;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = 4;
            }

            temp += 6;

            this.stackFrame = temp;
            this.sp = temp;
        }

        internal unsafe void PushFrame5()
        {
            // argument count
            // local variable values (reversed)
            // local variable count

            stackFrames[++sfp] = stackFrame;

            var temp = this.sp;

            fixed (ushort* pStack = this.stack)
            fixed (ushort* pLocals = this.locals)
            {
                ushort* sp = pStack + temp;
                ushort* lp = pLocals + 5;

                *++sp = this.argumentCount;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = 5;
            }

            temp += 7;

            this.stackFrame = temp;
            this.sp = temp;
        }

        internal unsafe void PushFrame6()
        {
            // argument count
            // local variable values (reversed)
            // local variable count

            stackFrames[++sfp] = stackFrame;

            var temp = this.sp;

            fixed (ushort* pStack = this.stack)
            fixed (ushort* pLocals = this.locals)
            {
                ushort* sp = pStack + temp;
                ushort* lp = pLocals + 6;

                *++sp = this.argumentCount;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = 6;
            }

            temp += 8;

            this.stackFrame = temp;
            this.sp = temp;
        }

        internal unsafe void PushFrame7()
        {
            // argument count
            // local variable values (reversed)
            // local variable count

            stackFrames[++sfp] = stackFrame;

            var temp = this.sp;

            fixed (ushort* pStack = this.stack)
            fixed (ushort* pLocals = this.locals)
            {
                ushort* sp = pStack + temp;
                ushort* lp = pLocals + 7;

                *++sp = this.argumentCount;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = 7;
            }

            temp += 9;

            this.stackFrame = temp;
            this.sp = temp;
        }

        internal unsafe void PushFrame8()
        {
            // argument count
            // local variable values (reversed)
            // local variable count

            stackFrames[++sfp] = stackFrame;

            var temp = this.sp;

            fixed (ushort* pStack = this.stack)
            fixed (ushort* pLocals = this.locals)
            {
                ushort* sp = pStack + temp;
                ushort* lp = pLocals + 8;

                *++sp = this.argumentCount;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = 8;
            }

            temp += 10;

            this.stackFrame = temp;
            this.sp = temp;
        }

        internal unsafe void PushFrame9()
        {
            // argument count
            // local variable values (reversed)
            // local variable count

            stackFrames[++sfp] = stackFrame;

            var temp = this.sp;

            fixed (ushort* pStack = this.stack)
            fixed (ushort* pLocals = this.locals)
            {
                ushort* sp = pStack + temp;
                ushort* lp = pLocals + 9;

                *++sp = this.argumentCount;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = 9;
            }

            temp += 11;

            this.stackFrame = temp;
            this.sp = temp;
        }

        internal unsafe void PushFrame10()
        {
            // argument count
            // local variable values (reversed)
            // local variable count

            stackFrames[++sfp] = stackFrame;

            var temp = this.sp;

            fixed (ushort* pStack = this.stack)
            fixed (ushort* pLocals = this.locals)
            {
                ushort* sp = pStack + temp;
                ushort* lp = pLocals + 10;

                *++sp = this.argumentCount;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = 10;
            }

            temp += 12;

            this.stackFrame = temp;
            this.sp = temp;
        }

        internal unsafe void PushFrame11()
        {
            // argument count
            // local variable values (reversed)
            // local variable count

            stackFrames[++sfp] = stackFrame;

            var temp = this.sp;

            fixed (ushort* pStack = this.stack)
            fixed (ushort* pLocals = this.locals)
            {
                ushort* sp = pStack + temp;
                ushort* lp = pLocals + 11;

                *++sp = this.argumentCount;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = 11;
            }

            temp += 13;

            this.stackFrame = temp;
            this.sp = temp;
        }

        internal unsafe void PushFrame12()
        {
            // argument count
            // local variable values (reversed)
            // local variable count

            stackFrames[++sfp] = stackFrame;

            var temp = this.sp;

            fixed (ushort* pStack = this.stack)
            fixed (ushort* pLocals = this.locals)
            {
                ushort* sp = pStack + temp;
                ushort* lp = pLocals + 12;

                *++sp = this.argumentCount;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = 12;
            }

            temp += 14;

            this.stackFrame = temp;
            this.sp = temp;
        }

        internal unsafe void PushFrame13()
        {
            // argument count
            // local variable values (reversed)
            // local variable count

            stackFrames[++sfp] = stackFrame;

            var temp = this.sp;

            fixed (ushort* pStack = this.stack)
            fixed (ushort* pLocals = this.locals)
            {
                ushort* sp = pStack + temp;
                ushort* lp = pLocals + 13;

                *++sp = this.argumentCount;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = 13;
            }

            temp += 15;

            this.stackFrame = temp;
            this.sp = temp;
        }

        internal unsafe void PushFrame14()
        {
            // argument count
            // local variable values (reversed)
            // local variable count

            stackFrames[++sfp] = stackFrame;

            var temp = this.sp;

            fixed (ushort* pStack = this.stack)
            fixed (ushort* pLocals = this.locals)
            {
                ushort* sp = pStack + temp;
                ushort* lp = pLocals + 14;

                *++sp = this.argumentCount;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = 14;
            }

            temp += 16;

            this.stackFrame = temp;
            this.sp = temp;
        }

        internal unsafe void PushFrame15()
        {
            // argument count
            // local variable values (reversed)
            // local variable count

            stackFrames[++sfp] = stackFrame;

            var temp = this.sp;

            fixed (ushort* pStack = this.stack)
            fixed (ushort* pLocals = this.locals)
            {
                ushort* sp = pStack + temp;
                ushort* lp = pLocals + 15;

                *++sp = this.argumentCount;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = *--lp;
                *++sp = 15;
            }

            temp += 17;

            this.stackFrame = temp;
            this.sp = temp;
        }

        internal unsafe void PopFrame()
        {
            // local variable count
            // local variable values
            // argument count

            var temp = this.stackFrame;

            fixed (ushort* pStack = this.stack)
            fixed (ushort* pLocals = this.locals)
            {
                ushort* sp = pStack + temp;
                ushort* lp = pLocals;

                var localCount = *(sp--);
                for (int i = 0; i < localCount; i++)
                {
                    *(lp++) = *(sp--);
                }

                this.argumentCount = *(sp--);
                this.localCount = localCount;

                this.stackFrame = stackFrames[sfp--];
                this.sp = temp - localCount - 2;
            }
        }

        private ZRoutine GetRoutineByAddress(int address)
        {
            ZRoutine routine;
            if (!routineTable.TryGetByAddress(address, out routine))
            {
                routineTable.Add(address);
                routine = routineTable.GetByAddress(address);
            }

            return routine;
        }

        internal ZCompilerResult Compile(ZRoutine routine)
        {
            ZCompilerResult result;
            if (!compilationResults.TryGetValue(routine.Address, out result))
            {
                result = ZCompiler.Compile(routine, machine: this);

                compilationResults.Add(routine.Address, result);

                if (profiler != null)
                {
                    profiler.RoutineCompiled(result.Statistics);
                }
            }

            return result;
        }

        private bool compiling;

        internal ZRoutineCall GetRoutineCall(int address)
        {
            ZRoutineCall routineCall;
            if (!addressToRoutineCallMap.TryGetValue(address, out routineCall))
            {
                cacheMiss++;
                var routine = GetRoutineByAddress(address);
                routineCall = new ZRoutineCall(routine, machine: this);
                addressToRoutineCallMap.Add(address, routineCall);
            }

            if (this.precompile && !compiling)
            {
                compiling = true;
                routineCall.Compile();
                compiling = false;
            }

            return routineCall;
        }

        private unsafe void SetupCall(ZRoutineCall routineCall, ushort argCount)
        {
            // PushFrame();

            this.argumentCount = argCount;

            var locals = routineCall.Routine.Locals;
            var localCount = (ushort)locals.Length;

            fixed (ushort* pLocals = this.locals)
            {
                for (int i = argCount; i < localCount; i++)
                {
                    *(pLocals + i) = locals[i];
                }
            }

            this.localCount = localCount;
        }

        internal unsafe ushort DirectCall0(ZRoutineCall routineCall)
        {
            Debug.Assert(routineCall.Routine.Address != 0);

            this.argumentCount = 0;

            var locals = routineCall.Routine.Locals;
            var localCount = (ushort)locals.Length;

            fixed (ushort* pLocals = this.locals)
            {
                for (int i = 0; i < localCount; i++)
                {
                    *(pLocals + i) = (this.Version < 5 ? locals[i] : (ushort)0);
                }
            }

            this.localCount = localCount;

            return routineCall.Invoke();
        }

        internal ushort CalculatedCall0(int address)
        {
            Debug.Assert(address != 0);

            return DirectCall0(GetRoutineCall(address));
        }

        internal unsafe ushort DirectCall1(ZRoutineCall routineCall, ushort arg0)
        {
            Debug.Assert(routineCall.Routine.Address != 0);

            this.argumentCount = 1;

            var locals = routineCall.Routine.Locals;
            var localCount = (ushort)locals.Length;

            fixed (ushort* pLocals = this.locals)
            {
                *pLocals = arg0;

                for (int i = 1; i < localCount; i++)
                {
                    *(pLocals + i) = (this.Version < 5 ? locals[i] : (ushort)0);
                }
            }

            this.localCount = localCount;

            return routineCall.Invoke();
        }

        internal ushort CalculatedCall1(int address, ushort arg0)
        {
            Debug.Assert(address != 0);

            return DirectCall1(GetRoutineCall(address), arg0);
        }

        internal unsafe ushort DirectCall2(ZRoutineCall routineCall, ushort arg0, ushort arg1)
        {
            Debug.Assert(routineCall.Routine.Address != 0);

            this.argumentCount = 2;

            var locals = routineCall.Routine.Locals;
            var localCount = (ushort)locals.Length;

            fixed (ushort* pLocals = this.locals)
            {
                *pLocals = arg0;
                *(pLocals + 1) = arg1;

                for (int i = 2; i < localCount; i++)
                {
                    *(pLocals + i) = (this.Version < 5 ? locals[i] : (ushort)0);
                }
            }

            this.localCount = localCount;

            return routineCall.Invoke();
        }

        internal ushort CalculatedCall2(int address, ushort arg0, ushort arg1)
        {
            Debug.Assert(address != 0);

            return DirectCall2(GetRoutineCall(address), arg0, arg1);
        }

        internal unsafe ushort DirectCall3(ZRoutineCall routineCall, ushort arg0, ushort arg1, ushort arg2)
        {
            Debug.Assert(routineCall.Routine.Address != 0);

            this.argumentCount = 3;

            var locals = routineCall.Routine.Locals;
            var localCount = (ushort)locals.Length;

            fixed (ushort* pLocals = this.locals)
            {
                *pLocals = arg0;
                *(pLocals + 1) = arg1;
                *(pLocals + 2) = arg2;

                for (int i = 3; i < localCount; i++)
                {
                    *(pLocals + i) = (this.Version < 5 ? locals[i] : (ushort)0);
                }
            }

            this.localCount = localCount;

            return routineCall.Invoke();
        }

        internal ushort CalculatedCall3(int address, ushort arg0, ushort arg1, ushort arg2)
        {
            Debug.Assert(address != 0);

            return DirectCall3(GetRoutineCall(address), arg0, arg1, arg2);
        }

        internal unsafe ushort DirectCall4(ZRoutineCall routineCall, ushort arg0, ushort arg1, ushort arg2, ushort arg3)
        {
            Debug.Assert(routineCall.Routine.Address != 0);

            this.argumentCount = 4;

            var locals = routineCall.Routine.Locals;
            var localCount = (ushort)locals.Length;

            fixed (ushort* pLocals = this.locals)
            {
                *pLocals = arg0;
                *(pLocals + 1) = arg1;
                *(pLocals + 2) = arg2;
                *(pLocals + 3) = arg3;

                for (int i = 4; i < localCount; i++)
                {
                    *(pLocals + i) = (this.Version < 5 ? locals[i] : (ushort)0);
                }
            }

            this.localCount = localCount;

            return routineCall.Invoke();
        }

        internal ushort CalculatedCall4(int address, ushort arg0, ushort arg1, ushort arg2, ushort arg3)
        {
            Debug.Assert(address != 0);

            return DirectCall4(GetRoutineCall(address), arg0, arg1, arg2, arg3);
        }

        internal unsafe ushort DirectCall5(ZRoutineCall routineCall, ushort arg0, ushort arg1, ushort arg2, ushort arg3, ushort arg4)
        {
            Debug.Assert(routineCall.Routine.Address != 0);

            this.argumentCount = 5;

            var locals = routineCall.Routine.Locals;
            var localCount = (ushort)locals.Length;

            fixed (ushort* pLocals = this.locals)
            {
                *pLocals = arg0;
                *(pLocals + 1) = arg1;
                *(pLocals + 2) = arg2;
                *(pLocals + 3) = arg3;
                *(pLocals + 4) = arg4;

                for (int i = 5; i < localCount; i++)
                {
                    *(pLocals + i) = (this.Version < 5 ? locals[i] : (ushort)0);
                }
            }

            this.localCount = localCount;

            return routineCall.Invoke();
        }

        internal ushort CalculatedCall5(int address, ushort arg0, ushort arg1, ushort arg2, ushort arg3, ushort arg4)
        {
            Debug.Assert(address != 0);

            return DirectCall5(GetRoutineCall(address), arg0, arg1, arg2, arg3, arg4);
        }

        internal unsafe ushort DirectCall6(ZRoutineCall routineCall, ushort arg0, ushort arg1, ushort arg2, ushort arg3, ushort arg4, ushort arg5)
        {
            Debug.Assert(routineCall.Routine.Address != 0);

            this.argumentCount = 6;

            var locals = routineCall.Routine.Locals;
            var localCount = (ushort)locals.Length;

            fixed (ushort* pLocals = this.locals)
            {
                *pLocals = arg0;
                *(pLocals + 1) = arg1;
                *(pLocals + 2) = arg2;
                *(pLocals + 3) = arg3;
                *(pLocals + 4) = arg4;
                *(pLocals + 5) = arg5;

                for (int i = 6; i < localCount; i++)
                {
                    *(pLocals + i) = (this.Version < 5 ? locals[i] : (ushort)0);
                }
            }

            this.localCount = localCount;

            return routineCall.Invoke();
        }

        internal ushort CalculatedCall6(int address, ushort arg0, ushort arg1, ushort arg2, ushort arg3, ushort arg4, ushort arg5)
        {
            Debug.Assert(address != 0);

            return DirectCall6(GetRoutineCall(address), arg0, arg1, arg2, arg3, arg4, arg5);
        }

        internal unsafe ushort DirectCall7(ZRoutineCall routineCall, ushort arg0, ushort arg1, ushort arg2, ushort arg3, ushort arg4, ushort arg5, ushort arg6)
        {
            Debug.Assert(routineCall.Routine.Address != 0);

            this.argumentCount = 7;

            var locals = routineCall.Routine.Locals;
            var localCount = (ushort)locals.Length;

            fixed (ushort* pLocals = this.locals)
            {
                *pLocals = arg0;
                *(pLocals + 1) = arg1;
                *(pLocals + 2) = arg2;
                *(pLocals + 3) = arg3;
                *(pLocals + 4) = arg4;
                *(pLocals + 5) = arg5;
                *(pLocals + 6) = arg6;

                for (int i = 7; i < localCount; i++)
                {
                    *(pLocals + i) = (this.Version < 5 ? locals[i] : (ushort)0);
                }
            }

            this.localCount = localCount;

            return routineCall.Invoke();
        }

        internal ushort CalculatedCall7(int address, ushort arg0, ushort arg1, ushort arg2, ushort arg3, ushort arg4, ushort arg5, ushort arg6)
        {
            Debug.Assert(address != 0);

            return DirectCall7(GetRoutineCall(address), arg0, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        internal ZRoutineCode GetRoutineCode(int address)
        {
            var routine = GetRoutineByAddress(address);
            return Compile(routine).Code;
        }

        internal void Profiler_Call(int address, bool calculated)
        {
            if (profiler != null)
            {
                profiler.Call(address, calculated);
            }
        }

        internal void EnterRoutine(int address)
        {
            if (profiler != null)
            {
                profiler.EnterRoutine(address);
            }
        }

        internal void ExitRoutine(int address)
        {
            if (profiler != null)
            {
                profiler.ExitRoutine(address);
            }
        }

        internal void ExecutingInstruction(int address)
        {
            if (profiler != null)
            {
                if (currentAddress >= 0)
                {
                    ExecutedInstruction();
                }

                currentAddress = address;
                profiler.ExecutingInstruction(address);
            }
        }

        internal void ExecutedInstruction()
        {
            if (profiler != null)
            {
                profiler.ExecutedInstruction(currentAddress);
            }
        }

        internal void Quit()
        {
            if (profiler != null)
            {
                profiler.Quit();
            }
        }

        internal void Interrupt()
        {
            if (profiler != null)
            {
                profiler.Interrupt();
            }
        }

        internal string ReadZText(int address)
        {
            var zwords = this.ZText.ReadZWords(address);
            return ConvertZText(zwords);
        }

        internal int NextRandom(short range)
        {
            // range should be inclusive, so we need to subtract 1 since System.Random.Next makes it exclusive
            const ushort minValue = 1;
            ushort maxValue = Math.Max(minValue, (ushort)(range - 1));
            var result = GenerateRandomNumber(minValue, maxValue);

            return result;
        }

        internal void SeedRandom(short range)
        {
            if (range == 0)
            {
                SetRandomSeed((int)DateTime.Now.Ticks);
            }
            else
            {
                SetRandomSeed(+range);
            }
        }

        internal string ConvertZText(ushort[] zwords)
        {
            return this.ZText.ZWordsAsString(zwords, ZTextFlags.All);
        }

        internal void Read_Z3(ushort textBuffer, ushort parseBuffer)
        {
            inputReceived = false;

            this.Screen.ShowStatus();

            byte maxChars = this.Memory.ReadByte(textBuffer);

            this.Screen.ReadCommand(maxChars, s =>
            {
                string text = s.ToLower();

                for (int i = 0; i < text.Length; i++)
                {
                    this.Memory.WriteByte(textBuffer + 1 + i, (byte)text[i]);
                }

                this.Memory.WriteByte(textBuffer + 1 + text.Length, 0);

                // TODO: Use ztext.TokenizeLine.

                ZCommandToken[] tokens = this.ZText.TokenizeCommand(text, dictionaryAddress);

                byte maxWords = this.Memory.ReadByte(parseBuffer);
                byte parsedWords = Math.Min(maxWords, (byte)tokens.Length);

                this.Memory.WriteByte(parseBuffer + 1, parsedWords);

                for (int i = 0; i < parsedWords; i++)
                {
                    ZCommandToken token = tokens[i];

                    ushort address = this.ZText.LookupWord(token.Text, dictionaryAddress);
                    if (address > 0)
                    {
                        this.Memory.WriteWord(parseBuffer + 2 + (i * 4), address);
                    }
                    else
                    {
                        this.Memory.WriteWord(parseBuffer + 2 + (i * 4), 0);
                    }

                    this.Memory.WriteByte(parseBuffer + 2 + (i * 4) + 2, (byte)token.Length);
                    this.Memory.WriteByte(parseBuffer + 2 + (i * 4) + 3, (byte)(token.Start + 1));
                }

                inputReceived = true;
            });

            while (!inputReceived && !stopping)
            {
            }

            if (stopping)
            {
                throw new ZMachineInterruptedException();
            }
        }

        internal void Read_Z4(ushort textBuffer, ushort parseBuffer)
        {
            // TODO: Support timed input

            inputReceived = false;

            byte maxChars = this.Memory.ReadByte(textBuffer);

            this.Screen.ReadCommand(maxChars, s =>
            {
                string text = s.ToLower();

                for (int i = 0; i < text.Length; i++)
                {
                    this.Memory.WriteByte(textBuffer + 1 + i, (byte)text[i]);
                }

                this.Memory.WriteByte(textBuffer + 1 + text.Length, 0);

                // TODO: Use ztext.TokenizeLine.

                ZCommandToken[] tokens = this.ZText.TokenizeCommand(text, dictionaryAddress);

                byte maxWords = this.Memory.ReadByte(parseBuffer);
                byte parsedWords = Math.Min(maxWords, (byte)tokens.Length);

                this.Memory.WriteByte(parseBuffer + 1, parsedWords);

                for (int i = 0; i < parsedWords; i++)
                {
                    ZCommandToken token = tokens[i];

                    ushort address = this.ZText.LookupWord(token.Text, dictionaryAddress);
                    if (address > 0)
                    {
                        this.Memory.WriteWord(parseBuffer + 2 + (i * 4), address);
                    }
                    else
                    {
                        this.Memory.WriteWord(parseBuffer + 2 + (i * 4), 0);
                    }

                    this.Memory.WriteByte(parseBuffer + 2 + (i * 4) + 2, (byte)token.Length);
                    this.Memory.WriteByte(parseBuffer + 2 + (i * 4) + 3, (byte)(token.Start + 1));
                }

                inputReceived = true;
            });

            while (!inputReceived && !stopping)
            {
            }

            if (stopping)
            {
                throw new ZMachineInterruptedException();
            }
        }

        internal unsafe ushort Read_Z5(ushort textBuffer, ushort parseBuffer)
        {
            // TODO: Support timed input

            inputReceived = false;
            ushort result = 0;

            fixed (byte* pMemory = Memory)
            {
                byte* pTextBuffer = pMemory + textBuffer;

                byte maxChars = *pTextBuffer++;

                this.Screen.ReadCommand(maxChars, s =>
                {
                    string text = s.ToLower();

                    byte existingTextCount = *pTextBuffer;
                    *pTextBuffer++ = (byte)text.Length;
                    pTextBuffer += existingTextCount;

                    for (int i = 0; i < text.Length; i++)
                    {
                        *pTextBuffer++ = (byte)text[i];
                    }

                    if (parseBuffer > 0)
                    {
                        this.ZText.TokenizeLine(textBuffer, parseBuffer, dictionaryAddress, flag: false);
                    }

                    // TODO: Update this when timed input is supported
                    result = 10;

                    inputReceived = true;
                });
            }

            while (!inputReceived && !stopping)
            {
            }

            if (stopping)
            {
                throw new ZMachineInterruptedException();
            }

            return result;
        }

        internal ushort ReadChar()
        {
            inputReceived = false;
            ushort result = 0;

            this.Screen.ReadChar(ch =>
            {
                result = (ushort)ch;
                inputReceived = true;
            });

            while (!inputReceived)
            {
            }

            return result;
        }

        internal void Tokenize(ushort textBuffer, ushort parseBuffer, ushort dictionary, bool flag)
        {
            this.ZText.TokenizeLine(textBuffer, parseBuffer, dictionary, flag);
        }

        internal void op_copy_table(ushort first, ushort second, ushort size)
        {
            if (second == 0) // zero out first table
            {
                for (int j = 0; j < size; j++)
                {
                    this.Memory.WriteByte(first + j, 0);
                }
            }
            else if ((short)size < 0 || first > second) // copy forwards
            {
                var copySize = size;
                if ((short)copySize < 0)
                {
                    copySize = (ushort)(-((short)size));
                }

                for (int j = 0; j < copySize; j++)
                {
                    var value = this.Memory.ReadByte(first + j);
                    this.Memory.WriteByte(second + j, value);
                }
            }
            else // copy backwards
            {
                for (int j = size - 1; j >= 0; j--)
                {
                    var value = this.Memory.ReadByte(first + j);
                    this.Memory.WriteByte(second + j, value);
                }
            }
        }

        internal ushort op_scan_table(ushort x, ushort table, ushort len, ushort form)
        {
            ushort address = table;

            for (int j = 0; j < len; j++)
            {
                if ((form & 0x80) != 0)
                {
                    var value = this.Memory.ReadWord(address);
                    if (value == x)
                    {
                        return address;
                    }
                }
                else
                {
                    var value = this.Memory[address];
                    if (value == x)
                    {
                        return address;
                    }
                }

                address += (ushort)(form & 0x7f);
            }

            return 0;
        }

        public int UnpackRoutineAddress(ushort byteAddress)
        {
            return (byteAddress * packResolution) + (routinesOffset * 8);
        }

        public int UnpackStringAddress(ushort byteAddress)
        {
            return (byteAddress * packResolution) + (stringsOffset * 8);
        }

        public void Run()
        {
            stopping = false;
            var routineCall = GetRoutineCall(GetMainRoutineAddress());
            routineCall.Invoke();
        }

        internal void Tick()
        {
            if (stopping)
            {
                stopping = false;
                throw new ZMachineInterruptedException();
            }
        }

        public void Stop()
        {
            stopping = true;
        }

        public bool Profiling
        {
            get { return profiler != null; }
        }

        public bool Precompile
        {
            get
            {
                return precompile;
            }
        }

        public bool Debugging
        {
            get
            {
                return debugging;
            }
        }

        public ushort ObjectTableAddress
        {
            get { return objectTableAddress; }
        }

        public byte PropertyDefaultsTableSize
        {
            get { return propertyDefaultsTableSize; }
        }

        public ushort ObjectEntriesAddress
        {
            get { return objectEntriesAddress; }
        }

        public byte ObjectEntrySize
        {
            get { return objectEntrySize; }
        }

        public byte ObjectParentOffset
        {
            get { return objectParentOffset; }
        }

        public byte ObjectSiblingOffset
        {
            get { return objectSiblingOffset; }
        }

        public byte ObjectChildOffset
        {
            get { return objectChildOffset; }
        }

        public byte ObjectPropertyTableAddressOffset
        {
            get { return objectPropertyTableAddressOffset; }
        }

        public byte ObjectAttributesByteCount
        {
            get { return objectAttributeByteCount; }
        }

        public byte ObjectAttributeCount
        {
            get { return objectAttributeCount; }
        }

        public int RoutinesOffset
        {
            get { return routinesOffset; }
        }

        public int StringsOffset
        {
            get { return stringsOffset; }
        }
    }
}
