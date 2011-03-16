using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using ZDebug.Compiler.Generate;
using ZDebug.Compiler.Profiling;
using ZDebug.Compiler.Utilities;
using ZDebug.Core.Execution;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler
{
    public partial class ZCompiler
    {
        private readonly ZRoutine routine;
        private readonly ZMachine machine;
        private bool debugging;

        private ILBuilder il;
        private Dictionary<int, ILabel> addressToLabelMap;
        private Instruction currentInstruction;
        private List<ZRoutineCall> calls;

        private IArrayLocal memory;
        private ILocal screen;
        private ILocal outputStreams;

        private bool usesStack;
        private bool usesMemory;

        private IArrayLocal stack;
        private LocalBuilder sp;
        private IArrayLocal locals;

        private ZCompiler(ZRoutine routine, ZMachine machine, bool debugging = false)
        {
            this.routine = routine;
            this.machine = machine;
            this.debugging = debugging;
        }

        private static string GetName(ZRoutine routine)
        {
            return "ZRoutine_" + routine.Address.ToString("x4");
        }

        public ZCompilerResult Compile()
        {
            var sw = Stopwatch.StartNew();

            var dm = new DynamicMethod(
                name: GetName(routine),
                returnType: typeof(ushort),
                parameterTypes: Types.Two<ZMachine, ZRoutineCall[]>(),
                owner: typeof(ZMachine),
                skipVisibility: true);

            var ilGen = dm.GetILGenerator();
            this.il = new ILBuilder(ilGen);

            this.calls = new List<ZRoutineCall>();

            Profiler_EnterRoutine();

            // First pass: gather branches and labels
            this.addressToLabelMap = new Dictionary<int, ILabel>();
            foreach (var i in routine.Instructions)
            {
                if (i.HasBranch && i.Branch.Kind == BranchKind.Address)
                {
                    var address = i.Address + i.Length + i.Branch.Offset - 2;
                    if (!this.addressToLabelMap.ContainsKey(address))
                    {
                        this.addressToLabelMap.Add(address, il.NewLabel());
                    }
                }
                else if (i.Opcode.IsJump)
                {
                    var address = i.Address + i.Length + (short)i.Operands[0].Value - 2;
                    if (!this.addressToLabelMap.ContainsKey(address))
                    {
                        this.addressToLabelMap.Add(address, il.NewLabel());
                    }
                }
            }

            // Second pass: determine whether stack is used
            foreach (var i in routine.Instructions)
            {
                if (i.UsesStack())
                {
                    this.usesStack = true;

                    // stack...
                    var stackField = Reflection<ZMachine>.GetField("stack", @public: false);
                    this.stack = il.NewArrayLocal<ushort>(il.GenerateLoadInstanceField(stackField));

                    // sp...
                    var spField = Reflection<ZMachine>.GetField("sp", @public: false);
                    var int32ByRefType = typeof(int).MakeByRefType();
                    this.sp = ilGen.DeclareLocal(int32ByRefType);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldflda, spField);
                    il.Emit(OpCodes.Stloc, this.sp);

                    break;
                }
            }

            // Third pass: determine whether memory is used
            foreach (var i in routine.Instructions)
            {
                if (i.UsesMemory())
                {
                    this.usesMemory = true;

                    var memoryField = Reflection<ZMachine>.GetField("memory", @public: false);
                    this.memory = il.NewArrayLocal<byte>(il.GenerateLoadInstanceField(memoryField));

                    break;
                }
            }

            if (routine.Locals.Length > 0)
            {
                var localsField = Reflection<ZMachine>.GetField("locals", @public: false);
                this.locals = il.NewArrayLocal<ushort>(il.GenerateLoadInstanceField(localsField));
            }

            // Fourth pass: determine whether screen is used
            foreach (var i in routine.Instructions)
            {
                if (i.UsesScreen())
                {
                    var screenField = Reflection<ZMachine>.GetField("screen", @public: false);
                    this.screen = il.NewLocal<IScreen>(il.GenerateLoadInstanceField(screenField));
                    break;
                }
            }

            // Fifth pass: determine whether outputStreams is used
            foreach (var i in routine.Instructions)
            {
                if (i.UsesOutputStreams())
                {
                    var outputStreamsField = Reflection<ZMachine>.GetField("outputStreams", @public: false);
                    this.outputStreams = il.NewLocal<IOutputStream>(il.GenerateLoadInstanceField(outputStreamsField));
                    break;
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

                Profiler_ExecutingInstruction();
                il.DebugWrite(i.PrettyPrint(machine));

                Assemble();
            }

            var code = (ZRoutineCode)dm.CreateDelegate(typeof(ZRoutineCode), machine);

            sw.Stop();

            var statistics = new RoutineCompilationStatistics(this.routine, il.OpcodeCount, il.LocalCount, il.Size, sw.Elapsed);

            return new ZCompilerResult(this.routine, calls.ToArray(), code, statistics);
        }

        private void Profiler_EnterRoutine()
        {
            if (machine.Profiling)
            {
                il.LoadArg(0); // ZMachine
                il.Load(routine.Address);

                var enterRoutine = Reflection<ZMachine>.GetMethod("EnterRoutine", Types.One<int>(), @public: false);
                il.Call(enterRoutine);
            }
        }

        private void Profiler_ExitRoutine()
        {
            if (machine.Profiling)
            {
                il.LoadArg(0); // ZMachine
                il.Load(routine.Address);

                var exitRoutine = Reflection<ZMachine>.GetMethod("ExitRoutine", Types.One<int>(), @public: false);
                il.Call(exitRoutine);
            }
        }

        private void Profiler_ExecutingInstruction()
        {
            if (machine.Profiling)
            {
                il.LoadArg(0); // ZMachine
                il.Load(currentInstruction.Address);

                var executingInstruction = Reflection<ZMachine>.GetMethod("ExecutingInstruction", Types.One<int>(), @public: false);
                il.Call(executingInstruction);
            }
        }

        private void Profiler_ExecutedInstruction()
        {
            if (machine.Profiling)
            {
                il.LoadArg(0);

                var executedInstruction = Reflection<ZMachine>.GetMethod("ExecutedInstruction", Types.None, @public: false);
                il.Call(executedInstruction);
            }
        }

        private void Profiler_Quit()
        {
            if (machine.Profiling)
            {
                il.LoadArg(0);

                var quit = Reflection<ZMachine>.GetMethod("Quit", Types.None, @public: false);
                il.Call(quit);

                Profiler_ExecutedInstruction();
            }
        }

        private void Profiler_Interrupt()
        {
            if (machine.Profiling)
            {
                il.LoadArg(0);

                var interrupt = Reflection<ZMachine>.GetMethod("Interrupt", Types.None, @public: false);
                il.Call(interrupt);
            }
        }

        private void NotImplemented()
        {
            il.RuntimeError(string.Format("{0:x4}: Opcode '{1}' not implemented.", currentInstruction.Address, currentInstruction.Opcode.Name));
        }

        private void DirectCall(Operand addressOp)
        {
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

                var callName = "Call" + argCount.ToString();
                var callTypeList = new List<Type>() { typeof(ZRoutineCall) };
                for (int i = 0; i < argCount; i++)
                {
                    callTypeList.Add(typeof(ushort));
                }
                var callTypes = callTypeList.ToArray();

                var call = Reflection<ZMachine>.GetMethod(callName, callTypes, @public: false);

                il.Call(call);
            }
        }

        private void IndirectCall(Operand addressOp)
        {
            using (var address = il.NewLocal<int>())
            {
                LoadUnpackedRoutineAddress(addressOp);
                address.Store();

                // is this address 0?
                var nonZeroCall = il.NewLabel();
                var done = il.NewLabel();
                address.Load();
                nonZeroCall.BranchIf(Condition.True);

                var argCount = currentInstruction.OperandCount - 1;
                for (int i = 0; i < argCount; i++)
                {
                    LoadOperand(i + 1);
                    il.Pop();
                }

                il.Load(0);

                done.Branch();

                nonZeroCall.Mark();

                il.LoadArg(0);
                address.Load();

                for (int i = 0; i < argCount; i++)
                {
                    LoadOperand(i + 1);
                }

                var callName = "Call" + argCount.ToString();
                var callTypeList = new List<Type>() { typeof(int) };
                for (int i = 0; i < argCount; i++)
                {
                    callTypeList.Add(typeof(ushort));
                }
                var callTypes = callTypeList.ToArray();

                var call = Reflection<ZMachine>.GetMethod(callName, callTypes, @public: false);

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
                IndirectCall(addressOp);
            }
        }

        private void Return(int? value = null)
        {
            Profiler_ExitRoutine();

            il.LoadArg(0);

            var popFrame = Reflection<ZMachine>.GetMethod("PopFrame", @public: false);
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

        public static ZCompilerResult Compile(ZRoutine routine, ZMachine machine)
        {
            return new ZCompiler(routine, machine).Compile();
        }
    }
}
