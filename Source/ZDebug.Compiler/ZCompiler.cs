using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using ZDebug.Core.Instructions;
using System.Reflection;
using ZDebug.Core.Execution;

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

        private readonly static MethodInfo callHelper = typeof(ZMachine).GetMethod(
            name: "Call",
            bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            types: new Type[] { typeof(int), typeof(ushort[]) },
            modifiers: null);

        internal const int STACK_SIZE = 1024;

        private readonly ZRoutine routine;
        private readonly ZMachine machine;

        private ILGenerator il;
        private LocalManager localManager;
        private Dictionary<int, Label> addressToLabelMap;
        private Instruction currentInstruction;

        private LocalBuilder memory;
        private LocalBuilder screen;

        private LocalBuilder args;
        private LocalBuilder argCount;

        private LocalBuilder stack;
        private LocalBuilder sp;

        private LocalBuilder locals;

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

            this.il = dm.GetILGenerator();
            this.localManager = new LocalManager(il);

            // First pass: gather branches and labels
            this.addressToLabelMap = new Dictionary<int, Label>();
            foreach (var i in routine.Instructions)
            {
                if (i.HasBranch && i.Branch.Kind == BranchKind.Address)
                {
                    var address = i.Address + i.Length + i.Branch.Offset - 2;
                    this.addressToLabelMap.Add(address, il.DefineLabel());
                }
                else if (i.Opcode.IsJump)
                {
                    var address = i.Address + i.Length + (short)i.Operands[0].Value - 2;
                    this.addressToLabelMap.Add(address, il.DefineLabel());
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

            this.memory = il.DeclareLocal<byte[]>();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, memoryField);
            il.Emit(OpCodes.Stloc, this.memory);

            // Second pass: determine whether screen is used
            foreach (var i in routine.Instructions)
            {
                if (i.UsesScreen())
                {
                    this.screen = il.DeclareLocal<IScreen>();
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, screenField);
                    il.Emit(OpCodes.Stloc, this.screen);
                    break;
                }
            }

            // Create stack, sp and locals
            this.stack = usesStack ? il.DeclareArrayLocal<ushort>(STACK_SIZE) : null;
            this.sp = usesStack ? il.DeclareLocal(0) : null;

            var localValues = routine.Locals;
            int localCount = localValues.Length;
            this.locals = il.DeclareArrayLocal<ushort>(localCount);
            for (int i = 0; i < localCount; i++)
            {
                if (localValues[i] != 0)
                {
                    il.Emit(OpCodes.Ldloc, this.locals);
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Ldc_I4, localValues[i]);
                    il.Emit(OpCodes.Stelem_I2);
                }
            }

            // Copy arguments locally
            this.argCount = il.DeclareLocal<int>();
            this.args = il.DeclareLocal<ushort[]>();
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stloc, args);
            il.Emit(OpCodes.Ldloc, args);
            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Conv_I4);
            il.Emit(OpCodes.Stloc, argCount);

            // TODO: Don't copy args if there aren't any

            // Initialize locals with args
            il.CopyArray(this.args, this.locals);

            // Fourth pass: emit IL for instructions
            foreach (var i in routine.Instructions)
            {
                Label label;
                if (this.addressToLabelMap.TryGetValue(i.Address, out label))
                {
                    il.MarkLabel(label);
                }

                il.Emit(OpCodes.Nop);
                il.DebugWrite(string.Format("{0:x4}: {1}", i.Address, i.Opcode.Name));

                currentInstruction = i;
                Assemble();
            }

            return (ZRoutineCode)dm.CreateDelegate(typeof(ZRoutineCode), machine);
        }

        private void NotImplemented()
        {
            il.ThrowException("'" + currentInstruction.Opcode.Name + "' not implemented.");
        }

        private void Branch()
        {
            // It is expected that the value on the top of the evaluation stack
            // is the boolean value to compare branch.Condition with.

            var noJump = il.DefineLabel();

            il.LoadBool(currentInstruction.Branch.Condition);
            il.Emit(OpCodes.Bne_Un_S, noJump);

            switch (currentInstruction.Branch.Kind)
            {
                case BranchKind.RFalse:
                    il.DebugWrite("branching rfalse...");
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Ret);
                    break;

                case BranchKind.RTrue:
                    il.DebugWrite("branching rtrue...");
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Ret);
                    break;

                default: // BranchKind.Address
                    var address = currentInstruction.Address + currentInstruction.Length + currentInstruction.Branch.Offset - 2;
                    var jump = addressToLabelMap[address];
                    il.DebugWrite(string.Format("branching to {0:x4}...", address));
                    il.Emit(OpCodes.Br, jump);
                    break;
            }

            il.MarkLabel(noJump);
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
                        case 0x04:
                            op_dec_chk();
                            return;
                        case 0x05:
                            op_inc_chk();
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
                        case 0x1a:
                            if (machine.Version >= 5)
                            {
                                op_call_n();
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
                        case 0x0b:
                            op_ret();
                            return;
                        case 0x0c:
                            op_jump();
                            return;
                        case 0x0d:
                            op_print_paddr();
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
                        case 0x0a:
                            op_quit();
                            return;
                        case 0x0b:
                            op_new_line();
                            return;
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
                        case 0x03:
                            op_put_prop();
                            return;
                        case 0x05:
                            op_print_char();
                            return;
                        case 0x06:
                            op_print_num();
                            return;
                    }

                    break;
                case OpcodeKind.Ext:
                    break;
            }

            throw new ZCompilerException(
                string.Format("Unsupported opcode: {0} ({1} {2:x2})", currentInstruction.Opcode.Name, currentInstruction.Opcode.Kind, currentInstruction.Opcode.Number));
        }

        public static ZRoutineCode Compile(ZRoutine routine, ZMachine machine)
        {
            return new ZCompiler(routine, machine).Compile();
        }
    }
}
