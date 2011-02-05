﻿using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using ZDebug.Core.Instructions;
using System.Reflection;
using ZDebug.Core.Execution;
using ZDebug.Compiler.Generate;

namespace ZDebug.Compiler
{
    public partial class ZCompiler
    {
        private readonly static FieldInfo memoryField = typeof(ZMachine).GetField(
            name: "memory",
            bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly static FieldInfo screenField = typeof(ZMachine).GetField(
            name: "screen",
            bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly static FieldInfo outputStreamsField = typeof(ZMachine).GetField(
            name: "outputStreams",
            bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly static MethodInfo callHelper = typeof(ZMachine).GetMethod(
            name: "Call",
            bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            types: new Type[] { typeof(int), typeof(ushort[]) },
            modifiers: null);

        private readonly static MethodInfo readZTextHelper = typeof(ZMachine).GetMethod(
            name: "ReadZText",
            bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            types: new Type[] { typeof(int) },
            modifiers: null);

        private readonly static MethodInfo nextRandomHelper = typeof(ZMachine).GetMethod(
            name: "NextRandom",
            bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            types: new Type[] { typeof(short) },
            modifiers: null);

        private readonly static MethodInfo seedRandomHelper = typeof(ZMachine).GetMethod(
            name: "SeedRandom",
            bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            types: new Type[] { typeof(short) },
            modifiers: null);

        private readonly static MethodInfo convertZTextHelper = typeof(ZMachine).GetMethod(
            name: "ConvertZText",
            bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            types: new Type[] { typeof(ushort[]) },
            modifiers: null);

        private readonly static MethodInfo readZ3Helper = typeof(ZMachine).GetMethod(
            name: "Read_Z3",
            bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            types: new Type[] { typeof(ushort), typeof(ushort) },
            modifiers: null);

        private readonly static MethodInfo readZ4Helper = typeof(ZMachine).GetMethod(
            name: "Read_Z4",
            bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            types: new Type[] { typeof(ushort), typeof(ushort) },
            modifiers: null);

        private readonly static MethodInfo readZ5Helper = typeof(ZMachine).GetMethod(
            name: "Read_Z5",
            bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            types: new Type[] { typeof(ushort), typeof(ushort) },
            modifiers: null);

        internal const int STACK_SIZE = 1024;

        private readonly ZRoutine routine;
        private readonly ZMachine machine;

        private ILBuilder il;
        private Dictionary<int, ILabel> addressToLabelMap;
        private Instruction currentInstruction;

        private IArrayLocal memory;
        private ILocal screen;
        private ILocal outputStreams;

        private IArrayLocal args;
        private ILocal argCount;

        private IArrayLocal stack;
        private ILocal sp;

        private IArrayLocal locals;

        private ZCompiler(ZRoutine routine, ZMachine machine)
        {
            this.routine = routine;
            this.machine = machine;
        }

        private static string GetName(ZRoutine routine)
        {
            return "ZRoutine_" + routine.Address.ToString("x4");
        }

        public ZRoutineCode Compile()
        {
            var dm = new DynamicMethod(
                name: GetName(routine),
                returnType: typeof(ushort),
                parameterTypes: new Type[] { typeof(ZMachine), typeof(ushort[]) },
                owner: typeof(ZMachine),
                skipVisibility: true);

            var ilGen = dm.GetILGenerator();
            this.il = new ILBuilder(ilGen);

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

            // Third pass: determine whether local stack is used
            var usesStack = false;
            foreach (var i in routine.Instructions)
            {
                usesStack = i.UsesStack();
                if (usesStack)
                {
                    break;
                }
            }

            // Fourth pass: determine whether memory is used
            // TODO: Implement!

            this.memory = il.NewArrayLocal<byte>(il.GenerateLoadInstanceField(memoryField));

            // Second pass: determine whether screen is used
            foreach (var i in routine.Instructions)
            {
                if (i.UsesScreen())
                {
                    this.screen = il.NewLocal<IScreen>(il.GenerateLoadInstanceField(screenField));
                    break;
                }
            }

            this.outputStreams = il.NewLocal<IOutputStream>(il.GenerateLoadInstanceField(outputStreamsField));

            // Create stack and sp
            this.stack = usesStack ? il.NewArrayLocal<ushort>(STACK_SIZE) : null;
            this.sp = usesStack ? il.NewLocal(0) : null;

            // Copy arguments locally
            this.argCount = il.NewLocal<int>();
            this.args = il.NewArrayLocal<ushort>(il.GenerateLoadArg(1));
            this.args.LoadLength();
            il.Convert.ToInt32();
            this.argCount.Store();

            // Copy locals
            var localValues = routine.Locals;
            int localCount = localValues.Length;
            if (localCount > 0)
            {
                this.locals = il.NewArrayLocal<ushort>(localCount);
                for (int i = 0; i < localCount; i++)
                {
                    if (localValues[i] != 0)
                    {
                        this.locals.StoreElement(
                            loadIndex: il.GenerateLoad(i),
                            loadValue: il.GenerateLoad(localValues[i]));
                    }
                }

                // TODO: Don't copy args if there aren't any

                // Initialize locals with args
                il.CopyArray(this.args, this.locals);
            }

            // Fourth pass: emit IL for instructions
            foreach (var i in routine.Instructions)
            {
                ILabel label;
                if (this.addressToLabelMap.TryGetValue(i.Address, out label))
                {
                    label.Mark();
                }

                il.DebugWrite(i.PrettyPrint(machine));

                currentInstruction = i;
                Assemble();
            }

            return (ZRoutineCode)dm.CreateDelegate(typeof(ZRoutineCode), machine);
        }

        private void NotImplemented()
        {
            il.RuntimeError(string.Format("{0:x4}: Opcode '{1}' not implemented.", currentInstruction.Address, currentInstruction.Opcode.Name));
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
                    il.Return(0);
                    break;

                case BranchKind.RTrue:
                    il.DebugWrite("  > branching rtrue...");
                    il.Return(1);
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

        public static ZRoutineCode Compile(ZRoutine routine, ZMachine machine)
        {
            return new ZCompiler(routine, machine).Compile();
        }
    }
}
