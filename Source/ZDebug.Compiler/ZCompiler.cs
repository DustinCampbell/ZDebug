using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using ZDebug.Compiler.Generate;
using ZDebug.Compiler.Profiling;
using ZDebug.Core.Instructions;
using ZDebug.Core.Routines;
using ZDebug.Core.Utilities;

// Potential optimizations:
//
// * Speed up calculated calls
// * Create ref locals for globals directly accessed

namespace ZDebug.Compiler
{
    public partial class ZCompiler
    {
        private readonly ZRoutine routine;
        private readonly CompiledZMachine machine;
        private bool debugging;

        private ILBuilder il;
        private IDictionary<int, ILabel> addressToLabelMap;
        private Instruction currentInstruction;
        private List<ZRoutineCall> calls;

        private IArrayLocal memory;
        private ILocal screen;
        private ILocal outputStreams;

        private bool usesStack;
        private bool usesMemory;
        private bool usesScreen;
        private bool usesOutputStreams;

        private IArrayLocal stack;
        private IRefLocal spRef;

        private IArrayLocal locals;
        private IRefLocal[] localRefs;

        private int calculatedLoadVariableCount;
        private int calculatedStoreVariableCount;

        private static readonly Type[] directCall0Types = Types.Array<ZRoutineCall>();
        private static readonly Type[] directCall1Types = Types.Array<ZRoutineCall, ushort>();
        private static readonly Type[] directCall2Types = Types.Array<ZRoutineCall, ushort, ushort>();
        private static readonly Type[] directCall3Types = Types.Array<ZRoutineCall, ushort, ushort, ushort>();
        private static readonly Type[] directCall4Types = Types.Array<ZRoutineCall, ushort, ushort, ushort, ushort>();
        private static readonly Type[] directCall5Types = Types.Array<ZRoutineCall, ushort, ushort, ushort, ushort, ushort>();
        private static readonly Type[] directCall6Types = Types.Array<ZRoutineCall, ushort, ushort, ushort, ushort, ushort, ushort>();
        private static readonly Type[] directCall7Types = Types.Array<ZRoutineCall, ushort, ushort, ushort, ushort, ushort, ushort, ushort>();

        private static readonly Type[] calculatedCall0Types = Types.Array<int>();
        private static readonly Type[] calculatedCall1Types = Types.Array<int, ushort>();
        private static readonly Type[] calculatedCall2Types = Types.Array<int, ushort, ushort>();
        private static readonly Type[] calculatedCall3Types = Types.Array<int, ushort, ushort, ushort>();
        private static readonly Type[] calculatedCall4Types = Types.Array<int, ushort, ushort, ushort, ushort>();
        private static readonly Type[] calculatedCall5Types = Types.Array<int, ushort, ushort, ushort, ushort, ushort>();
        private static readonly Type[] calculatedCall6Types = Types.Array<int, ushort, ushort, ushort, ushort, ushort, ushort>();
        private static readonly Type[] calculatedCall7Types = Types.Array<int, ushort, ushort, ushort, ushort, ushort, ushort, ushort>();

        private ZCompiler(ZRoutine routine, CompiledZMachine machine, bool debugging = false)
        {
            this.routine = routine;
            this.machine = machine;
            this.debugging = debugging;
        }

        private static string GetName(ZRoutine routine)
        {
            return "ZRoutine_" + routine.Address.ToString("x4");
        }

        private ZCompilerResult Compile()
        {
            var sw = Stopwatch.StartNew();

            var dm = new DynamicMethod(
                name: GetName(routine),
                returnType: typeof(ushort),
                parameterTypes: Types.Array<CompiledZMachine, ZRoutineCall[]>(),
                owner: typeof(CompiledZMachine),
                skipVisibility: true);

            var ilGen = dm.GetILGenerator();
            this.il = new ILBuilder(ilGen);

            this.calls = new List<ZRoutineCall>();

            Profiler_EnterRoutine();

            this.addressToLabelMap = CollectLabels(this.routine, this.il);
            this.usesStack = CreateStackAndSPVariablesIfNeeded(this.routine, this.il);
            this.usesMemory = CreateMemoryVariableIfNeeded(this.routine, this.il);
            this.usesScreen = CreateScreenVariableIfNeeded(this.routine, this.il);
            this.usesOutputStreams = CreateOutputStreamsVariableIfNeeded(this.routine, this.il);

            var localCount = routine.Locals.Length;
            if (localCount > 0)
            {
                // correct local byref variables to vector elements of the ZMachine's locals array.
                var localsField = Reflection<CompiledZMachine>.GetField("locals", @public: false);

                // Determine whether we need the 'locals' variable or not.
                // We should only need this if there is an instruction with
                // a by-ref first operand that's a variable.
                foreach (var i in routine.Instructions)
                {
                    if (i.Opcode.IsFirstOpByRef && i.Operands[0].Kind == OperandKind.Variable)
                    {
                        this.locals = il.NewArrayLocal<ushort>(il.GenerateLoadInstanceField(localsField));
                        break;
                    }
                }

                this.localRefs = new IRefLocal[localCount];

                // Try the code below to see if it's more efficent than loading the field once for each local.
                il.LoadArg(0);
                il.Load(localsField);

                for (int i = 0; i < localCount - 1; i++)
                {
                    il.Duplicate();
                }

                for (int i = 0; i < localCount; i++)
                {
                    this.localRefs[i] = il.NewRefLocal<ushort>();
                    il.Load(i);
                    il.Emit(OpCodes.Ldelema, typeof(ushort));
                    this.localRefs[i].Store();
                }
            }

            // Sixth pass: emit IL for instructions
            foreach (var i in routine.Instructions)
            {
                ILabel label;
                if (this.addressToLabelMap.TryGetValue(i.Address, out label))
                {
                    label.Mark();
                }

                currentInstruction = i;

                Debugging_Tick();
                Profiler_ExecutingInstruction();
                il.DebugWrite(i.PrettyPrint(machine));

                Assemble();
            }

            var code = (ZRoutineCode)dm.CreateDelegate(typeof(ZRoutineCode), machine);

            sw.Stop();

            var statistics = new RoutineCompilationStatistics(
                this.routine,
                il.OpcodeCount,
                il.LocalCount,
                il.Size,
                sw.Elapsed,
                calculatedLoadVariableCount,
                calculatedStoreVariableCount);

            return new ZCompilerResult(this.routine, calls.ToArray(), code, statistics);
        }

        private void Debugging_Tick()
        {
            if (machine.Debugging)
            {
                il.LoadThis();
                il.Call(Reflection<CompiledZMachine>.GetMethod("Tick", @public: false));
            }
        }

        private void Profiler_EnterRoutine()
        {
            if (machine.Profiling)
            {
                il.LoadArg(0); // ZMachine
                il.Load(routine.Address);

                var enterRoutine = Reflection<CompiledZMachine>.GetMethod("EnterRoutine", Types.Array<int>(), @public: false);
                il.Call(enterRoutine);
            }
        }

        private void Profiler_ExitRoutine()
        {
            if (machine.Profiling)
            {
                il.LoadArg(0); // ZMachine
                il.Load(routine.Address);

                var exitRoutine = Reflection<CompiledZMachine>.GetMethod("ExitRoutine", Types.Array<int>(), @public: false);
                il.Call(exitRoutine);
            }
        }

        private void Profiler_ExecutingInstruction()
        {
            if (machine.Profiling)
            {
                il.LoadArg(0); // ZMachine
                il.Load(currentInstruction.Address);

                var executingInstruction = Reflection<CompiledZMachine>.GetMethod("ExecutingInstruction", Types.Array<int>(), @public: false);
                il.Call(executingInstruction);
            }
        }

        private void Profiler_ExecutedInstruction()
        {
            if (machine.Profiling)
            {
                il.LoadArg(0);

                var executedInstruction = Reflection<CompiledZMachine>.GetMethod("ExecutedInstruction", Types.None, @public: false);
                il.Call(executedInstruction);
            }
        }

        private void Profiler_Quit()
        {
            if (machine.Profiling)
            {
                il.LoadArg(0);

                var quit = Reflection<CompiledZMachine>.GetMethod("Quit", Types.None, @public: false);
                il.Call(quit);

                Profiler_ExecutedInstruction();
            }
        }

        private void Profiler_Interrupt()
        {
            if (machine.Profiling)
            {
                il.LoadArg(0);

                var interrupt = Reflection<CompiledZMachine>.GetMethod("Interrupt", Types.None, @public: false);
                il.Call(interrupt);
            }
        }

        private void NotImplemented()
        {
            il.RuntimeError(string.Format("{0:x4}: Opcode '{1}' not implemented.", currentInstruction.Address, currentInstruction.Opcode.Name));
        }

        private static Type[] GetDirectCallTypes(int argumentCount)
        {
            switch (argumentCount)
            {
                case 0: return directCall0Types;
                case 1: return directCall1Types;
                case 2: return directCall2Types;
                case 3: return directCall3Types;
                case 4: return directCall4Types;
                case 5: return directCall5Types;
                case 6: return directCall6Types;
                case 7: return directCall7Types;
                default: throw new ZCompilerException("Only calls of 0 to 7 arguments are supported.");
            }
        }

        private static Type[] GetCalculatedCallTypes(int argumentCount)
        {
            switch (argumentCount)
            {
                case 0: return calculatedCall0Types;
                case 1: return calculatedCall1Types;
                case 2: return calculatedCall2Types;
                case 3: return calculatedCall3Types;
                case 4: return calculatedCall4Types;
                case 5: return calculatedCall5Types;
                case 6: return calculatedCall6Types;
                case 7: return calculatedCall7Types;
                default: throw new ZCompilerException("Only calls of 0 to 7 arguments are supported.");
            }
        }

        private void PushFrame(int localCount)
        {
            switch (localCount)
            {
                case 0:
                    il.LoadThis();
                    il.Call(Reflection<CompiledZMachine>.GetMethod("PushFrame0", @public: false));
                    break;

                case 1:
                    il.LoadThis();
                    il.Call(Reflection<CompiledZMachine>.GetMethod("PushFrame1", @public: false));
                    break;

                case 2:
                    il.LoadThis();
                    il.Call(Reflection<CompiledZMachine>.GetMethod("PushFrame2", @public: false));
                    break;

                case 3:
                    il.LoadThis();
                    il.Call(Reflection<CompiledZMachine>.GetMethod("PushFrame3", @public: false));
                    break;

                case 4:
                    il.LoadThis();
                    il.Call(Reflection<CompiledZMachine>.GetMethod("PushFrame4", @public: false));
                    break;

                case 5:
                    il.LoadThis();
                    il.Call(Reflection<CompiledZMachine>.GetMethod("PushFrame5", @public: false));
                    break;

                case 6:
                    il.LoadThis();
                    il.Call(Reflection<CompiledZMachine>.GetMethod("PushFrame6", @public: false));
                    break;

                case 7:
                    il.LoadThis();
                    il.Call(Reflection<CompiledZMachine>.GetMethod("PushFrame7", @public: false));
                    break;

                case 8:
                    il.LoadThis();
                    il.Call(Reflection<CompiledZMachine>.GetMethod("PushFrame8", @public: false));
                    break;

                case 9:
                    il.LoadThis();
                    il.Call(Reflection<CompiledZMachine>.GetMethod("PushFrame9", @public: false));
                    break;

                case 10:
                    il.LoadThis();
                    il.Call(Reflection<CompiledZMachine>.GetMethod("PushFrame10", @public: false));
                    break;

                case 11:
                    il.LoadThis();
                    il.Call(Reflection<CompiledZMachine>.GetMethod("PushFrame11", @public: false));
                    break;

                case 12:
                    il.LoadThis();
                    il.Call(Reflection<CompiledZMachine>.GetMethod("PushFrame12", @public: false));
                    break;

                case 13:
                    il.LoadThis();
                    il.Call(Reflection<CompiledZMachine>.GetMethod("PushFrame13", @public: false));
                    break;

                case 14:
                    il.LoadThis();
                    il.Call(Reflection<CompiledZMachine>.GetMethod("PushFrame14", @public: false));
                    break;

                case 15:
                    il.LoadThis();
                    il.Call(Reflection<CompiledZMachine>.GetMethod("PushFrame15", @public: false));
                    break;

                default:
                    throw new ZCompilerException("Unexpected: only 15 locals allowed.");
            }
        }

        private void DirectCall(Operand addressOp)
        {
            if (machine.Profiling)
            {
                il.LoadArg(0); // ZMachine

                if (addressOp.Value == 0)
                {
                    il.Load(0);
                }
                else
                {
                    il.Load(machine.UnpackRoutineAddress(addressOp.Value));
                }

                il.Load(false);

                var profilerCall = Reflection<CompiledZMachine>.GetMethod("Profiler_Call", Types.Array<int, bool>(), @public: false);
                il.Call(profilerCall);
            }

            if (addressOp.Value == 0)
            {
                Return(0);
            }
            else
            {
                var address = machine.UnpackRoutineAddress(addressOp.Value);
                var routineCall = machine.GetRoutineCall(address);
                var index = calls.Count;
                calls.Add(routineCall);

                il.LoadArg(0);

                il.LoadArg(1);
                il.Load(index);
                il.Emit(OpCodes.Ldelem_Ref);

                var argCount = currentInstruction.OperandCount - 1;
                for (int i = 0; i < argCount; i++)
                {
                    LoadOperand(i + 1);
                }

                // Push frame after loading operands in case any operands manipulate the stack.
                PushFrame(localRefs != null ? localRefs.Length : 0);

                var callName = "DirectCall" + argCount.ToString();
                var callTypes = GetDirectCallTypes(argCount);

                var call = Reflection<CompiledZMachine>.GetMethod(callName, callTypes, @public: false);

                il.Call(call);
            }
        }

        private void CalculatedCall(Operand addressOp)
        {
            using (var address = il.NewLocal<int>())
            {
                LoadVariable((byte)addressOp.Value);
                address.Store();

                // is this address 0?
                var nonZeroCall = il.NewLabel();
                var done = il.NewLabel();
                address.Load();
                nonZeroCall.BranchIf(Condition.True);

                if (machine.Profiling)
                {
                    il.LoadArg(0); // ZMachine
                    il.Load(0);
                    il.Load(true);

                    var profilerCall = Reflection<CompiledZMachine>.GetMethod("Profiler_Call", Types.Array<int, bool>(), @public: false);
                    il.Call(profilerCall);
                }

                var argCount = currentInstruction.OperandCount - 1;

                // discard any SP operands...
                int spOperands = 0;
                for (int i = 1; i <= argCount; i++)
                {
                    var op = GetOperand(i);
                    if (op.Kind == OperandKind.Variable && op.Value == 0)
                    {
                        spOperands++;
                    }
                }

                if (spOperands > 0)
                {
                    spRef.Load();
                    spRef.Load();
                    spRef.LoadIndirectValue();
                    il.Math.Subtract(spOperands);
                    spRef.StoreIndirectValue();
                }

                il.Load(0);

                done.Branch();

                nonZeroCall.Mark();

                if (machine.Profiling)
                {
                    il.LoadArg(0); // ZMachine
                    address.Load();
                    UnpackRoutineAddress();
                    il.Load(true);

                    var profilerCall = Reflection<CompiledZMachine>.GetMethod("Profiler_Call", Types.Array<int, bool>(), @public: false);
                    il.Call(profilerCall);
                }

                il.LoadArg(0);
                address.Load();
                UnpackRoutineAddress();

                for (int i = 1; i <= argCount; i++)
                {
                    LoadOperand(i);
                }

                // Push frame after loading operands in case any operands manipulate the stack.
                il.LoadThis();
                il.Call(Reflection<CompiledZMachine>.GetMethod("PushFrame", @public: false));

                var callName = "CalculatedCall" + argCount.ToString();
                var callTypes = GetCalculatedCallTypes(argCount);

                var call = Reflection<CompiledZMachine>.GetMethod(callName, callTypes, @public: false);

                il.Call(call);

                done.Mark();
            }
        }

        private void Call()
        {
            var addressOp = GetOperand(0);
            if (addressOp.Kind != OperandKind.Variable)
            {
                DirectCall(addressOp);
            }
            else
            {
                CalculatedCall(addressOp);
            }
        }

        private void Return(int? value = null)
        {
            Profiler_ExitRoutine();

            il.LoadArg(0);

            var popFrame = Reflection<CompiledZMachine>.GetMethod("PopFrame", @public: false);
            il.Call(popFrame);

            if (value != null)
            {
                il.Return(value.Value);
            }
            else
            {
                il.Return();
            }
        }

        private void Branch()
        {
            if (!currentInstruction.HasBranch)
            {
                throw new ZCompilerException("Expected instruction to have a branch.");
            }

            // It is expected that the value on the top of the evaluation stack
            // is the boolean value to compare branch.Condition with.

            var noJump = il.NewLabel();

            il.Load(currentInstruction.Branch.Condition);
            noJump.BranchIf(Condition.NotEqual, @short: true);

            switch (currentInstruction.Branch.Kind)
            {
                case BranchKind.RFalse:
                    il.DebugWrite("  > branching rfalse...");
                    Return(0);
                    break;

                case BranchKind.RTrue:
                    il.DebugWrite("  > branching rtrue...");
                    Return(1);
                    break;

                default: // BranchKind.Address
                    var address = currentInstruction.Address + currentInstruction.Length + currentInstruction.Branch.Offset - 2;
                    var jump = addressToLabelMap[address];
                    il.DebugWrite(string.Format("  > branching to {0:x4}...", address));
                    jump.Branch();
                    break;
            }

            noJump.Mark();
        }

        private void Store(CodeBuilder valueLoader, bool indirect = false)
        {
            if (!currentInstruction.HasStoreVariable)
            {
                throw new ZCompilerException("Expected instruction to have a store variable.");
            }

            StoreVariable(currentInstruction.StoreVariable, valueLoader, indirect);
        }

        //private void PushFrame()
        //{
        //    il.DebugWrite("PushFrame(sp = {0}, sfp = {1}, stackFrame = {2})", spRef, sfpRef, stackFrameRef);
        //    il.DebugIndent();

        //    il.DebugWrite("stackFrames[++{0}] = {1}", sfpRef, stackFrameRef);

        //    // stackFrames[++sfp] = stackFrame;
        //    sfpRef.Load();
        //    sfpRef.Load();
        //    sfpRef.LoadIndirectValue();
        //    il.Math.Add(1);
        //    sfpRef.StoreIndirectValue();

        //    stackFrames.StoreElement(
        //        indexLoader: () =>
        //        {
        //            sfpRef.Load();
        //            sfpRef.LoadIndirectValue();
        //        },
        //        valueLoader: () =>
        //        {
        //            stackFrameRef.Load();
        //            stackFrameRef.LoadIndirectValue();
        //        });

        //    il.DebugWrite("stack[++{0}] = this.argumentCount ({1})", spRef, argumentCountRef);

        //    // stack[++sp] = this.argumentCount;
        //    PushStack(() =>
        //    {
        //        argumentCountRef.Load();
        //        argumentCountRef.LoadIndirectValue();
        //    });

        //    if (localRefs != null)
        //    {
        //        // for (int i = localCount - 1; i >= 0; i--)
        //        //   stack[++sp] = locals[i];
        //        for (int i = localRefs.Length - 1; i >= 0; i--)
        //        {
        //            il.DebugWrite("stack[++{0}] = this.locals[" + i + "] ({1:x4})", spRef, localRefs[i]);

        //            PushStack(() =>
        //            {
        //                LoadLocalVariable(i);
        //            });

        //            il.DebugWrite("sp = {0}", spRef);
        //        }

        //        il.DebugWrite("stack[++{0}] = localCount (" + localRefs.Length + ")", spRef);

        //        // stack[++sp] = localCount;
        //        PushStack(() =>
        //        {
        //            il.Load((ushort)localRefs.Length);
        //        });
        //    }
        //    else
        //    {
        //        il.DebugWrite("stack[++{0}] = localCount (0)", spRef);

        //        // stack[++sp] = 0;
        //        PushStack(() =>
        //        {
        //            il.Load(0);
        //        });
        //    }

        //    // stackFrame = sp;
        //    stackFrameRef.Load();
        //    spRef.Load();
        //    spRef.LoadIndirectValue();
        //    stackFrameRef.StoreIndirectValue();

        //    il.DebugWrite("stackFrame = {0}", stackFrameRef);
        //    il.DebugWrite("sp = {0}", spRef);

        //    il.DebugUnindent();
        //}

        private void Assemble()
        {
            switch (currentInstruction.Opcode.Kind)
            {
                case OpcodeKind.TwoOp:
                    switch (currentInstruction.Opcode.Number)
                    {
                        case 0x01:
                            op_je();
                            return;
                        case 0x02:
                            op_jl();
                            return;
                        case 0x03:
                            op_jg();
                            return;
                        case 0x04:
                            op_dec_chk();
                            return;
                        case 0x05:
                            op_inc_chk();
                            return;
                        case 0x06:
                            op_jin();
                            return;
                        case 0x07:
                            op_test();
                            return;
                        case 0x08:
                            op_or();
                            return;
                        case 0x09:
                            op_and();
                            return;
                        case 0x0a:
                            op_test_attr();
                            return;
                        case 0x0b:
                            op_set_attr();
                            return;
                        case 0x0c:
                            op_clear_attr();
                            return;
                        case 0x0d:
                            op_store();
                            return;
                        case 0x0e:
                            op_insert_obj();
                            return;
                        case 0x0f:
                            op_loadw();
                            return;
                        case 0x10:
                            op_loadb();
                            return;
                        case 0x11:
                            op_get_prop();
                            return;
                        case 0x12:
                            op_get_prop_addr();
                            return;
                        case 0x13:
                            op_get_next_prop();
                            return;
                        case 0x14:
                            op_add();
                            return;
                        case 0x15:
                            op_sub();
                            return;
                        case 0x16:
                            op_mul();
                            return;
                        case 0x17:
                            op_div();
                            return;
                        case 0x18:
                            op_mod();
                            return;
                        case 0x19:
                            if (machine.Version >= 4)
                            {
                                op_call_s();
                                return;
                            }

                            break;
                        case 0x1a:
                            if (machine.Version >= 5)
                            {
                                op_call_n();
                                return;
                            }

                            break;
                        case 0x1b:
                            if (machine.Version == 6)
                            {
                                op_set_color6();
                                return;
                            }
                            if (machine.Version >= 5)
                            {
                                op_set_color();
                                return;
                            }

                            break;
                    }

                    break;
                case OpcodeKind.OneOp:
                    switch (currentInstruction.Opcode.Number)
                    {
                        case 0x00:
                            op_jz();
                            return;
                        case 0x01:
                            op_get_sibling();
                            return;
                        case 0x02:
                            op_get_child();
                            return;
                        case 0x03:
                            op_get_parent();
                            return;
                        case 0x04:
                            op_get_prop_len();
                            return;
                        case 0x05:
                            op_inc();
                            return;
                        case 0x06:
                            op_dec();
                            return;
                        case 0x07:
                            op_print_addr();
                            return;
                        case 0x08:
                            if (machine.Version >= 4)
                            {
                                op_call_s();
                                return;
                            }

                            break;
                        case 0x09:
                            op_remove_obj();
                            return;
                        case 0x0a:
                            op_print_obj();
                            return;
                        case 0x0b:
                            op_ret();
                            return;
                        case 0x0c:
                            op_jump();
                            return;
                        case 0x0d:
                            op_print_paddr();
                            return;
                        case 0x0e:
                            op_load();
                            return;
                        case 0x0f:
                            if (machine.Version >= 5)
                            {
                                op_call_n();
                                return;
                            }

                            break;
                    }

                    break;
                case OpcodeKind.ZeroOp:
                    switch (currentInstruction.Opcode.Number)
                    {
                        case 0x00:
                            op_rtrue();
                            return;
                        case 0x01:
                            op_rfalse();
                            return;
                        case 0x02:
                            op_print();
                            return;
                        case 0x03:
                            op_print_ret();
                            return;
                        case 0x05:
                            if (machine.Version <= 4)
                            {
                                op_save();
                                return;
                            }

                            break;
                        case 0x06:
                            if (machine.Version <= 4)
                            {
                                op_restore();
                                return;
                            }

                            break;
                        case 0x07:
                            op_restart();
                            return;
                        case 0x08:
                            op_ret_popped();
                            return;
                        case 0x0a:
                            op_quit();
                            return;
                        case 0x0b:
                            op_new_line();
                            return;
                        case 0x0c:
                            if (machine.Version == 3)
                            {
                                op_show_status();
                                return;
                            }

                            break;
                        case 0x0d:
                            if (machine.Version >= 3)
                            {
                                op_verify();
                                return;
                            }

                            break;
                        case 0x0f:
                            if (machine.Version >= 5)
                            {
                                op_piracy();
                                return;
                            }

                            break;
                    }

                    break;
                case OpcodeKind.VarOp:
                    switch (currentInstruction.Opcode.Number)
                    {
                        case 0x00:
                            op_call_s();
                            return;
                        case 0x01:
                            op_storew();
                            return;
                        case 0x02:
                            op_storeb();
                            return;
                        case 0x03:
                            op_put_prop();
                            return;
                        case 0x04:
                            if (machine.Version < 4)
                            {
                                op_sread1();
                                return;
                            }
                            if (machine.Version == 4)
                            {
                                op_sread4();
                                return;
                            }
                            if (machine.Version > 4)
                            {
                                op_aread();
                                return;
                            }

                            break;
                        case 0x05:
                            op_print_char();
                            return;
                        case 0x06:
                            op_print_num();
                            return;
                        case 0x07:
                            op_random();
                            return;
                        case 0x08:
                            op_push();
                            return;
                        case 0x09:
                            if (machine.Version != 6)
                            {
                                op_pull();
                                return;
                            }

                            break;
                        case 0x0a:
                            if (machine.Version >= 3)
                            {
                                op_split_window();
                                return;
                            }

                            break;
                        case 0x0b:
                            if (machine.Version >= 3)
                            {
                                op_set_window();
                                return;
                            }

                            break;
                        case 0x0d:
                            if (machine.Version >= 4)
                            {
                                op_erase_window();
                                return;
                            }

                            break;
                        case 0x11:
                            if (machine.Version >= 4)
                            {
                                op_text_style();
                                return;
                            }

                            break;
                        case 0x12:
                            if (machine.Version >= 4)
                            {
                                op_buffer_mode();
                                return;
                            }

                            break;
                        case 0x13:
                            if (machine.Version >= 3 && machine.Version < 5)
                            {
                                op_output_stream();
                                return;
                            }
                            if (machine.Version == 6)
                            {
                                op_output_stream();
                                return;
                            }
                            if (machine.Version >= 5)
                            {
                                op_output_stream();
                                return;
                            }

                            break;
                        case 0x0c:
                            if (machine.Version >= 4)
                            {
                                op_call_s();
                                return;
                            }

                            break;
                        case 0x0f:
                            if (machine.Version == 6)
                            {
                                op_set_cursor6();
                                return;
                            }
                            if (machine.Version >= 4)
                            {
                                op_set_cursor();
                                return;
                            }

                            break;
                        case 0x16:
                            if (machine.Version >= 4)
                            {
                                op_read_char();
                                return;
                            }

                            break;

                        case 0x17:
                            if (machine.Version >= 4)
                            {
                                op_scan_table();
                                return;
                            }

                            break;

                        case 0x18:
                            if (machine.Version >= 5)
                            {
                                op_not();
                                return;
                            }

                            break;
                        case 0x19:
                            if (machine.Version >= 5)
                            {
                                op_call_n();
                                return;
                            }

                            break;
                        case 0x1a:
                            if (machine.Version >= 5)
                            {
                                op_call_n();
                                return;
                            }

                            break;
                        case 0x1b:
                            if (machine.Version >= 5)
                            {
                                op_tokenize();
                                return;
                            }

                            break;

                        case 0x1d:
                            if (machine.Version >= 5)
                            {
                                op_copy_table();
                                return;
                            }

                            break;

                        case 0x1f:
                            if (machine.Version >= 5)
                            {
                                op_check_arg_count();
                                return;
                            }

                            break;
                    }

                    break;
                case OpcodeKind.Ext:
                    switch (currentInstruction.Opcode.Number)
                    {
                        case 0x00:
                            if (machine.Version >= 5)
                            {
                                op_save();
                                return;
                            }

                            break;

                        case 0x01:
                            if (machine.Version >= 5)
                            {
                                op_restore();
                                return;
                            }

                            break;

                        case 0x02:
                            if (machine.Version >= 5)
                            {
                                op_log_shift();
                                return;
                            }

                            break;
                        case 0x03:
                            if (machine.Version >= 5)
                            {
                                op_art_shift();
                                return;
                            }

                            break;
                        case 0x09:
                            if (machine.Version >= 5)
                            {
                                op_save_undo();
                                return;
                            }

                            break;
                        case 0x0a:
                            if (machine.Version >= 5)
                            {
                                op_restore_undo();
                                return;
                            }

                            break;
                    }
                    break;
            }

            throw new ZCompilerException(
                string.Format(
                    "Unsupported opcode: {0} ({1} {2:x2})",
                    currentInstruction.Opcode.Name,
                    currentInstruction.Opcode.Kind,
                    currentInstruction.Opcode.Number));
        }

        public static ZCompilerResult Compile(ZRoutine routine, CompiledZMachine machine)
        {
            return new ZCompiler(routine, machine).Compile();
        }
    }
}
