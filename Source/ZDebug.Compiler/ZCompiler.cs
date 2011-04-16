using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using ZDebug.Compiler.Analysis.ControlFlow;
using ZDebug.Compiler.Generate;
using ZDebug.Compiler.Profiling;
using ZDebug.Core.Execution;
using ZDebug.Core.Instructions;
using ZDebug.Core.Routines;
using ZDebug.Core.Utilities;

// Potential optimizations:
//
// * Speed up calculated calls
// * Create ref locals for globals directly accessed

namespace ZDebug.Compiler
{
    internal partial class ZCompiler
    {
        private readonly ZRoutine routine;
        private readonly CompiledZMachine machine;
        private bool debugging;

        private ILBuilder il;
        private ControlFlowGraph controlFlowGraph;
        private Dictionary<int, ILabel> addressToLabelMap;
        private List<ZRoutineCall> calls;

        private LinkedListNode<Instruction> current;

        private ILocal screen;
        private ILocal outputStreams;

        private bool usesStack;
        private bool usesMemory;
        private bool usesScreen;
        private bool usesOutputStreams;

        private int calculatedLoadVariableCount;
        private int calculatedStoreVariableCount;

        private ZCompiler(ZRoutine routine, CompiledZMachine machine, bool debugging = false)
        {
            this.routine = routine;
            this.machine = machine;
            this.debugging = debugging;
        }

        private static DynamicMethod CreateDynamicMethod(ZRoutine routine)
        {
            return new DynamicMethod(
                name: string.Format("{0:x4}_{1}_locals", routine.Address, routine.Locals.Length),
                returnType: typeof(ushort),
                parameterTypes: Types.Array<CompiledZMachine, byte[], ushort[], ushort[], int, ZRoutineCall[], int>(),
                owner: typeof(CompiledZMachine),
                skipVisibility: true);
        }

        public ZCompilerResult Compile()
        {
            var sw = Stopwatch.StartNew();

            var dm = CreateDynamicMethod(routine);
            this.il = new ILBuilder(dm.GetILGenerator());

            this.calls = new List<ZRoutineCall>();

            Profiler_EnterRoutine();

            this.controlFlowGraph = ControlFlowGraph.Build(this.routine);
            this.addressToLabelMap = new Dictionary<int, ILabel>(this.controlFlowGraph.CodeBlocks.Count());

            // Determine whether stack, memory, screen and outputStreams are used.
            foreach (var codeBlock in this.controlFlowGraph.CodeBlocks)
            {
                this.addressToLabelMap.Add(codeBlock.Address, il.NewLabel());

                foreach (var i in codeBlock.Instructions)
                {
                    if (!this.usesStack && i.UsesStack())
                    {
                        this.usesStack = true;
                    }

                    if (!this.usesMemory && i.UsesMemory())
                    {
                        this.usesMemory = true;
                    }

                    if (!this.usesScreen && i.UsesScreen())
                    {
                        this.usesScreen = true;

                        // screen...
                        var screenField = Reflection<ZMachine>.GetField("Screen", @public: false);
                        this.screen = il.NewLocal<IScreen>(il.GenerateLoadInstanceField(screenField));
                    }

                    if (!this.usesOutputStreams && i.UsesOutputStreams())
                    {
                        this.usesOutputStreams = true;

                        // outputStreams...
                        var outputStreamsField = Reflection<ZMachine>.GetField("OutputStreams", @public: false);
                        this.outputStreams = il.NewLocal<IOutputStream>(il.GenerateLoadInstanceField(outputStreamsField));
                    }
                }
            }

            // Emit IL
            foreach (var codeBlock in this.controlFlowGraph.CodeBlocks)
            {
                //var generators = codeBlock.Instructions.Select(i => OpcodeGenerator.GetGenerator(i, machine.Version));

                //foreach (var generator in generators)
                //{
                //    ILabel label;
                //    if (this.addressToLabelMap.TryGetValue(generator.Instruction.Address, out label))
                //    {
                //        label.Mark();
                //    }

                //    generator.Generate(il, this);
                //}

                var instructions = new LinkedList<Instruction>(codeBlock.Instructions);
                var current = instructions.First;
                while (current != null)
                {
                    this.current = current;

                    ILabel label;
                    if (this.addressToLabelMap.TryGetValue(current.Value.Address, out label))
                    {
                        label.Mark();
                    }

                    if (machine.Debugging)
                    {
                        il.Arguments.LoadMachine();
                        il.Call(Reflection<CompiledZMachine>.GetMethod("Tick", @public: false));
                    }

                    Profiler_ExecutingInstruction();
                    il.DebugWrite(current.Value.PrettyPrint(machine));

                    Assemble(current.Value.Opcode);

                    current = current.Next;
                }
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

        private void Profiler_EnterRoutine()
        {
            if (machine.Profiling)
            {
                il.Arguments.LoadMachine();
                il.Load(routine.Address);
                il.Call(Reflection<CompiledZMachine>.GetMethod("EnterRoutine", Types.Array<int>(), @public: false));
            }
        }

        private void Profiler_ExitRoutine()
        {
            if (machine.Profiling)
            {
                il.Arguments.LoadMachine();
                il.Load(routine.Address);
                il.Call(Reflection<CompiledZMachine>.GetMethod("ExitRoutine", Types.Array<int>(), @public: false));
            }
        }

        private void Profiler_ExecutingInstruction()
        {
            if (machine.Profiling)
            {
                il.Arguments.LoadMachine();
                il.Load(this.current.Value.Address);
                il.Call(Reflection<CompiledZMachine>.GetMethod("ExecutingInstruction", Types.Array<int>(), @public: false));
            }
        }

        private void Profiler_ExecutedInstruction()
        {
            if (machine.Profiling)
            {
                il.Arguments.LoadMachine();
                il.Call(Reflection<CompiledZMachine>.GetMethod("ExecutedInstruction", Types.None, @public: false));
            }
        }

        private void Profiler_Quit()
        {
            if (machine.Profiling)
            {
                il.Arguments.LoadMachine();
                il.Call(Reflection<CompiledZMachine>.GetMethod("Quit", Types.None, @public: false));

                Profiler_ExecutedInstruction();
            }
        }

        private void Profiler_Interrupt()
        {
            if (machine.Profiling)
            {
                il.Arguments.LoadMachine();
                il.Call(Reflection<CompiledZMachine>.GetMethod("Interrupt", Types.None, @public: false));
            }
        }

        private void NotImplemented()
        {
            il.RuntimeError(string.Format("{0:x4}: Opcode '{1}' not implemented.", this.current.Value.Address, this.current.Value.Opcode.Name));
        }

        private Operand GetOperand(int operandIndex)
        {
            if (operandIndex < 0 || operandIndex >= OperandCount)
            {
                throw new ZCompilerException(
                    string.Format(
                        "Attempted to read operand {0}, but only 0 through {1} are valid.",
                        operandIndex,
                        OperandCount - 1));
            }

            return this.current.Value.Operands[operandIndex];
        }

        /// <summary>
        /// Loads the specified operand onto the evaluation stack.
        /// </summary>
        private void LoadOperand(int operandIndex)
        {
            var op = GetOperand(operandIndex);

            switch (op.Kind)
            {
                case OperandKind.LargeConstant:
                case OperandKind.SmallConstant:
                    il.Load(op.Value);
                    break;

                default: // OperandKind.Variable
                    EmitLoadVariable((byte)op.Value);
                    break;
            }
        }

        private int OperandCount
        {
            get
            {
                return this.current.Value.OperandCount;
            }
        }

        private void Store(CodeBuilder valueLoader)
        {
            if (!this.current.Value.HasStoreVariable)
            {
                throw new ZCompilerException("Expected instruction to have a store variable.");
            }

            using (var value = il.NewLocal<ushort>())
            {
                valueLoader();
                value.Store();

                EmitStoreVariable(this.current.Value.StoreVariable, value);
            }
        }

        private void Assemble(Opcode opcode)
        {
            switch (opcode.Kind)
            {
                case OpcodeKind.TwoOp:
                    switch (opcode.Number)
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
                    switch (opcode.Number)
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
                    switch (opcode.Number)
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
                    switch (opcode.Number)
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
                    switch (opcode.Number)
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
                    opcode.Name,
                    opcode.Kind,
                    opcode.Number));
        }

        public static ZCompilerResult Compile(ZRoutine routine, CompiledZMachine machine)
        {
            return new ZCompiler(routine, machine).Compile();
        }
    }
}
