using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using ZDebug.Compiler.Generate;
using ZDebug.Core.Execution;
using ZDebug.Core.Extensions;
using ZDebug.Core.Instructions;
using ZDebug.Core.Routines;
using ZDebug.Core.Utilities;

namespace ZDebug.Compiler
{
    public partial class ZCompiler
    {
        private static string GetName(ZRoutine routine, int localCount)
        {
            return "ZRoutine_" + localCount + "_" + routine.Address.ToString("x4");
        }

        private static IDictionary<int, ILabel> CollectLabels(ZRoutine routine, ILBuilder il)
        {
            var results = new Dictionary<int, ILabel>();

            foreach (var i in routine.Instructions)
            {
                if (i.HasBranch && i.Branch.Kind == BranchKind.Address)
                {
                    var address = i.Address + i.Length + i.Branch.Offset - 2;
                    if (!results.ContainsKey(address))
                    {
                        results.Add(address, il.NewLabel());
                    }
                }
                else if (i.Opcode.IsJump)
                {
                    var address = i.Address + i.Length + (short)i.Operands[0].Value - 2;
                    if (!results.ContainsKey(address))
                    {
                        results.Add(address, il.NewLabel());
                    }
                }
            }

            return results;
        }

        private bool CreateStackAndSPVariablesIfNeeded(ZRoutine routine, ILBuilder il)
        {
            foreach (var i in routine.Instructions)
            {
                if (i.UsesStack())
                {
                    // stack...
                    var stackField = Reflection<CompiledZMachine>.GetField("stack", @public: false);
                    this.stack = il.NewArrayLocal<ushort>(il.GenerateLoadInstanceField(stackField));

                    // sp...
                    var spField = Reflection<CompiledZMachine>.GetField("sp", @public: false);
                    this.spRef = il.NewRefLocal<int>(il.GenerateLoadInstanceFieldAddress(spField));

                    return true;
                }
            }

            return false;
        }

        private bool CreateMemoryVariableIfNeeded(ZRoutine routine, ILBuilder il)
        {
            foreach (var i in routine.Instructions)
            {
                if (i.UsesMemory())
                {
                    var memoryField = Reflection<ZMachine>.GetField("Memory");
                    this.memory = il.NewArrayLocal<byte>(il.GenerateLoadInstanceField(memoryField));

                    return true;
                }
            }

            return false;
        }

        private bool CreateScreenVariableIfNeeded(ZRoutine routine, ILBuilder il)
        {
            foreach (var i in routine.Instructions)
            {
                if (i.UsesScreen())
                {
                    var screenField = Reflection<ZMachine>.GetField("Screen", @public: false);
                    this.screen = il.NewLocal<IScreen>(il.GenerateLoadInstanceField(screenField));

                    return true;
                }
            }

            return false;
        }

        private bool CreateOutputStreamsVariableIfNeeded(ZRoutine routine, ILBuilder il)
        {
            foreach (var i in routine.Instructions)
            {
                if (i.UsesOutputStreams())
                {
                    var outputStreamsField = Reflection<ZMachine>.GetField("OutputStreams", @public: false);
                    this.outputStreams = il.NewLocal<IOutputStream>(il.GenerateLoadInstanceField(outputStreamsField));

                    return true;
                }
            }

            return false;
        }

        private void CompileRoutine(DynamicMethod dm)
        {
            var ilGenerator = dm.GetILGenerator();
            this.il = new ILBuilder(ilGenerator);

            Profiler_EnterRoutine();

            this.addressToLabelMap = CollectLabels(this.routine, il);
            this.usesStack = CreateStackAndSPVariablesIfNeeded(this.routine, il);
            this.usesMemory = CreateMemoryVariableIfNeeded(this.routine, il);
            this.usesScreen = CreateScreenVariableIfNeeded(this.routine, il);
            this.usesOutputStreams = CreateOutputStreamsVariableIfNeeded(this.routine, il);

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
        }

        private ZCodeDelegate0 Compile0()
        {
            var dm = new DynamicMethod(
                name: GetName(routine, localCount: 0),
                returnType: typeof(ushort),
                parameterTypes: ZCodeParameterTypes.ForDelegate<ZCodeDelegate0>(),
                owner: typeof(CompiledZMachine),
                skipVisibility: true);

            CompileRoutine(dm);

            return (ZCodeDelegate0)dm.CreateDelegate(typeof(ZCodeDelegate0), machine);
        }

        private ZCodeDelegate1 Compile1()
        {
            var dm = new DynamicMethod(
                name: GetName(routine, localCount: 1),
                returnType: typeof(ushort),
                parameterTypes: ZCodeParameterTypes.ForDelegate<ZCodeDelegate1>(),
                owner: typeof(CompiledZMachine),
                skipVisibility: true);

            CompileRoutine(dm);

            return (ZCodeDelegate1)dm.CreateDelegate(typeof(ZCodeDelegate1), machine);
        }

        private ZCodeDelegate2 Compile2()
        {
            var dm = new DynamicMethod(
                name: GetName(routine, localCount: 2),
                returnType: typeof(ushort),
                parameterTypes: ZCodeParameterTypes.ForDelegate<ZCodeDelegate2>(),
                owner: typeof(CompiledZMachine),
                skipVisibility: true);

            CompileRoutine(dm);

            return (ZCodeDelegate2)dm.CreateDelegate(typeof(ZCodeDelegate2), machine);
        }

        private ZCodeDelegate3 Compile3()
        {
            var dm = new DynamicMethod(
                name: GetName(routine, localCount: 3),
                returnType: typeof(ushort),
                parameterTypes: ZCodeParameterTypes.ForDelegate<ZCodeDelegate3>(),
                owner: typeof(CompiledZMachine),
                skipVisibility: true);

            CompileRoutine(dm);

            return (ZCodeDelegate3)dm.CreateDelegate(typeof(ZCodeDelegate3), machine);
        }

        private ZCodeDelegate4 Compile4()
        {
            var dm = new DynamicMethod(
                name: GetName(routine, localCount: 4),
                returnType: typeof(ushort),
                parameterTypes: ZCodeParameterTypes.ForDelegate<ZCodeDelegate4>(),
                owner: typeof(CompiledZMachine),
                skipVisibility: true);

            CompileRoutine(dm);

            return (ZCodeDelegate4)dm.CreateDelegate(typeof(ZCodeDelegate4), machine);
        }

        private ZCodeDelegate5 Compile5()
        {
            var dm = new DynamicMethod(
                name: GetName(routine, localCount: 5),
                returnType: typeof(ushort),
                parameterTypes: ZCodeParameterTypes.ForDelegate<ZCodeDelegate5>(),
                owner: typeof(CompiledZMachine),
                skipVisibility: true);

            CompileRoutine(dm);

            return (ZCodeDelegate5)dm.CreateDelegate(typeof(ZCodeDelegate5), machine);
        }

        private ZCodeDelegate6 Compile6()
        {
            var dm = new DynamicMethod(
                name: GetName(routine, localCount: 6),
                returnType: typeof(ushort),
                parameterTypes: ZCodeParameterTypes.ForDelegate<ZCodeDelegate6>(),
                owner: typeof(CompiledZMachine),
                skipVisibility: true);

            CompileRoutine(dm);

            return (ZCodeDelegate6)dm.CreateDelegate(typeof(ZCodeDelegate6), machine);
        }

        private ZCodeDelegate7 Compile7()
        {
            var dm = new DynamicMethod(
                name: GetName(routine, localCount: 7),
                returnType: typeof(ushort),
                parameterTypes: ZCodeParameterTypes.ForDelegate<ZCodeDelegate7>(),
                owner: typeof(CompiledZMachine),
                skipVisibility: true);

            CompileRoutine(dm);

            return (ZCodeDelegate7)dm.CreateDelegate(typeof(ZCodeDelegate7), machine);
        }

        private ZCodeDelegate8 Compile8()
        {
            var dm = new DynamicMethod(
                name: GetName(routine, localCount: 8),
                returnType: typeof(ushort),
                parameterTypes: ZCodeParameterTypes.ForDelegate<ZCodeDelegate8>(),
                owner: typeof(CompiledZMachine),
                skipVisibility: true);

            CompileRoutine(dm);

            return (ZCodeDelegate8)dm.CreateDelegate(typeof(ZCodeDelegate8), machine);
        }

        private ZCodeDelegate9 Compile9()
        {
            var dm = new DynamicMethod(
                name: GetName(routine, localCount: 9),
                returnType: typeof(ushort),
                parameterTypes: ZCodeParameterTypes.ForDelegate<ZCodeDelegate9>(),
                owner: typeof(CompiledZMachine),
                skipVisibility: true);

            CompileRoutine(dm);

            return (ZCodeDelegate9)dm.CreateDelegate(typeof(ZCodeDelegate9), machine);
        }

        private ZCodeDelegate10 Compile10()
        {
            var dm = new DynamicMethod(
                name: GetName(routine, localCount: 10),
                returnType: typeof(ushort),
                parameterTypes: ZCodeParameterTypes.ForDelegate<ZCodeDelegate10>(),
                owner: typeof(CompiledZMachine),
                skipVisibility: true);

            CompileRoutine(dm);

            return (ZCodeDelegate10)dm.CreateDelegate(typeof(ZCodeDelegate10), machine);
        }

        private ZCodeDelegate11 Compile11()
        {
            var dm = new DynamicMethod(
                name: GetName(routine, localCount: 11),
                returnType: typeof(ushort),
                parameterTypes: ZCodeParameterTypes.ForDelegate<ZCodeDelegate11>(),
                owner: typeof(CompiledZMachine),
                skipVisibility: true);

            CompileRoutine(dm);

            return (ZCodeDelegate11)dm.CreateDelegate(typeof(ZCodeDelegate11), machine);
        }

        private ZCodeDelegate12 Compile12()
        {
            var dm = new DynamicMethod(
                name: GetName(routine, localCount: 12),
                returnType: typeof(ushort),
                parameterTypes: ZCodeParameterTypes.ForDelegate<ZCodeDelegate12>(),
                owner: typeof(CompiledZMachine),
                skipVisibility: true);

            CompileRoutine(dm);

            return (ZCodeDelegate12)dm.CreateDelegate(typeof(ZCodeDelegate12), machine);
        }

        private ZCodeDelegate13 Compile13()
        {
            var dm = new DynamicMethod(
                name: GetName(routine, localCount: 13),
                returnType: typeof(ushort),
                parameterTypes: ZCodeParameterTypes.ForDelegate<ZCodeDelegate13>(),
                owner: typeof(CompiledZMachine),
                skipVisibility: true);

            CompileRoutine(dm);

            return (ZCodeDelegate13)dm.CreateDelegate(typeof(ZCodeDelegate13), machine);
        }

        private ZCodeDelegate14 Compile14()
        {
            var dm = new DynamicMethod(
                name: GetName(routine, localCount: 14),
                returnType: typeof(ushort),
                parameterTypes: ZCodeParameterTypes.ForDelegate<ZCodeDelegate14>(),
                owner: typeof(CompiledZMachine),
                skipVisibility: true);

            CompileRoutine(dm);

            return (ZCodeDelegate14)dm.CreateDelegate(typeof(ZCodeDelegate14), machine);
        }

        private ZCodeDelegate15 Compile15()
        {
            var dm = new DynamicMethod(
                name: GetName(routine, localCount: 15),
                returnType: typeof(ushort),
                parameterTypes: ZCodeParameterTypes.ForDelegate<ZCodeDelegate15>(),
                owner: typeof(CompiledZMachine),
                skipVisibility: true);

            CompileRoutine(dm);

            return (ZCodeDelegate15)dm.CreateDelegate(typeof(ZCodeDelegate15), machine);
        }

        private void DiscardStackOperands(int from, int to)
        {
            int stackOperands = 0;
            for (int i = from; i <= to; i++)
            {
                var op = GetOperand(i);
                if (op.Kind == OperandKind.Variable && op.Value == 0)
                {
                    stackOperands++;
                }
            }

            if (stackOperands > 0)
            {
                spRef.Load();
                spRef.Load();
                spRef.LoadIndirectValue();
                il.Math.Subtract(stackOperands);
                spRef.StoreIndirectValue();
            }
        }

        private void LoadCallArguments(int address, int argCount, int localCount)
        {
            var numberArgsToLoad = Math.Min(argCount, localCount);
            for (int i = 1; i <= numberArgsToLoad; i++)
            {
                LoadOperand(i);
            }

            // Do we have more args than locals? If so, discard any unloaded args that manipulate the stack.
            if (argCount > localCount)
            {
                DiscardStackOperands(localCount, argCount);
            }

            // Do we have more locals than args? If so, load the remaining initial local values.
            if (localCount > argCount)
            {
                if (machine.Version < 5)
                {
                    for (int i = argCount; i < localCount; i++)
                    {
                        il.Load(machine.Memory.ReadWord(address + (i * 2) + 1));
                    }
                }
                else
                {
                    for (int i = argCount; i < localCount; i++)
                    {
                        il.Load(0);
                    }
                }
            }
        }

        private void DirectCall0(int address, int argCount)
        {
            var getCode = Reflection<CompiledZMachine>.GetMethod("GetCode0", Types.Array<int>());
            il.LoadThis();
            il.Load(address);
            il.Call(getCode);

            il.Load(argCount);
            LoadCallArguments(address, argCount, 0);

            var invoke = Reflection<ZCodeDelegate0>.GetMethod("Invoke", ZCodeParameterTypes.ForDelegate<ZCodeDelegate0>());
            il.Call(invoke);
        }

        private void DirectCall1(int address, int argCount)
        {
            var getCode = Reflection<CompiledZMachine>.GetMethod("GetCode1", Types.Array<int>());
            il.LoadThis();
            il.Load(address);
            il.Call(getCode);

            il.Load(argCount);
            LoadCallArguments(address, argCount, 1);

            var invoke = Reflection<ZCodeDelegate1>.GetMethod("Invoke", ZCodeParameterTypes.ForDelegate<ZCodeDelegate1>());
            il.Call(invoke);
        }

        private void DirectCall2(int address, int argCount)
        {
            var getCode = Reflection<CompiledZMachine>.GetMethod("GetCode2", Types.Array<int>());
            il.LoadThis();
            il.Load(address);
            il.Call(getCode);

            il.Load(argCount);
            LoadCallArguments(address, argCount, 2);

            var invoke = Reflection<ZCodeDelegate2>.GetMethod("Invoke", ZCodeParameterTypes.ForDelegate<ZCodeDelegate2>());
            il.Call(invoke);
        }

        private void DirectCall3(int address, int argCount)
        {
            var getCode = Reflection<CompiledZMachine>.GetMethod("GetCode3", Types.Array<int>());
            il.LoadThis();
            il.Load(address);
            il.Call(getCode);

            il.Load(argCount);
            LoadCallArguments(address, argCount, 3);

            var invoke = Reflection<ZCodeDelegate3>.GetMethod("Invoke", ZCodeParameterTypes.ForDelegate<ZCodeDelegate3>());
            il.Call(invoke);
        }

        private void DirectCall4(int address, int argCount)
        {
            var getCode = Reflection<CompiledZMachine>.GetMethod("GetCode4", Types.Array<int>());
            il.LoadThis();
            il.Load(address);
            il.Call(getCode);

            il.Load(argCount);
            LoadCallArguments(address, argCount, 4);

            var invoke = Reflection<ZCodeDelegate4>.GetMethod("Invoke", ZCodeParameterTypes.ForDelegate<ZCodeDelegate4>());
            il.Call(invoke);
        }

        private void DirectCall5(int address, int argCount)
        {
            var getCode = Reflection<CompiledZMachine>.GetMethod("GetCode5", Types.Array<int>());
            il.LoadThis();
            il.Load(address);
            il.Call(getCode);

            il.Load(argCount);
            LoadCallArguments(address, argCount, 5);

            var invoke = Reflection<ZCodeDelegate5>.GetMethod("Invoke", ZCodeParameterTypes.ForDelegate<ZCodeDelegate5>());
            il.Call(invoke);
        }

        private void DirectCall6(int address, int argCount)
        {
            var getCode = Reflection<CompiledZMachine>.GetMethod("GetCode6", Types.Array<int>());
            il.LoadThis();
            il.Load(address);
            il.Call(getCode);

            il.Load(argCount);
            LoadCallArguments(address, argCount, 6);

            var invoke = Reflection<ZCodeDelegate6>.GetMethod("Invoke", ZCodeParameterTypes.ForDelegate<ZCodeDelegate6>());
            il.Call(invoke);
        }

        private void DirectCall7(int address, int argCount)
        {
            var getCode = Reflection<CompiledZMachine>.GetMethod("GetCode7", Types.Array<int>());
            il.LoadThis();
            il.Load(address);
            il.Call(getCode);

            il.Load(argCount);
            LoadCallArguments(address, argCount, 7);

            var invoke = Reflection<ZCodeDelegate7>.GetMethod("Invoke", ZCodeParameterTypes.ForDelegate<ZCodeDelegate7>());
            il.Call(invoke);
        }

        private void DirectCall8(int address, int argCount)
        {
            var getCode = Reflection<CompiledZMachine>.GetMethod("GetCode8", Types.Array<int>());
            il.LoadThis();
            il.Load(address);
            il.Call(getCode);

            il.Load(argCount);
            LoadCallArguments(address, argCount, 8);

            var invoke = Reflection<ZCodeDelegate8>.GetMethod("Invoke", ZCodeParameterTypes.ForDelegate<ZCodeDelegate8>());
            il.Call(invoke);
        }

        private void DirectCall9(int address, int argCount)
        {
            var getCode = Reflection<CompiledZMachine>.GetMethod("GetCode9", Types.Array<int>());
            il.LoadThis();
            il.Load(address);
            il.Call(getCode);

            il.Load(argCount);
            LoadCallArguments(address, argCount, 9);

            var invoke = Reflection<ZCodeDelegate9>.GetMethod("Invoke", ZCodeParameterTypes.ForDelegate<ZCodeDelegate9>());
            il.Call(invoke);
        }

        private void DirectCall10(int address, int argCount)
        {
            var getCode = Reflection<CompiledZMachine>.GetMethod("GetCode10", Types.Array<int>());
            il.LoadThis();
            il.Load(address);
            il.Call(getCode);

            il.Load(argCount);
            LoadCallArguments(address, argCount, 10);

            var invoke = Reflection<ZCodeDelegate10>.GetMethod("Invoke", ZCodeParameterTypes.ForDelegate<ZCodeDelegate10>());
            il.Call(invoke);
        }

        private void DirectCall11(int address, int argCount)
        {
            var getCode = Reflection<CompiledZMachine>.GetMethod("GetCode11", Types.Array<int>());
            il.LoadThis();
            il.Load(address);
            il.Call(getCode);

            il.Load(argCount);
            LoadCallArguments(address, argCount, 11);

            var invoke = Reflection<ZCodeDelegate11>.GetMethod("Invoke", ZCodeParameterTypes.ForDelegate<ZCodeDelegate11>());
            il.Call(invoke);
        }

        private void DirectCall12(int address, int argCount)
        {
            var getCode = Reflection<CompiledZMachine>.GetMethod("GetCode12", Types.Array<int>());
            il.LoadThis();
            il.Load(address);
            il.Call(getCode);

            il.Load(argCount);
            LoadCallArguments(address, argCount, 12);

            var invoke = Reflection<ZCodeDelegate12>.GetMethod("Invoke", ZCodeParameterTypes.ForDelegate<ZCodeDelegate12>());
            il.Call(invoke);
        }

        private void DirectCall13(int address, int argCount)
        {
            var getCode = Reflection<CompiledZMachine>.GetMethod("GetCode13", Types.Array<int>());
            il.LoadThis();
            il.Load(address);
            il.Call(getCode);

            il.Load(argCount);
            LoadCallArguments(address, argCount, 13);

            var invoke = Reflection<ZCodeDelegate13>.GetMethod("Invoke", ZCodeParameterTypes.ForDelegate<ZCodeDelegate13>());
            il.Call(invoke);
        }

        private void DirectCall14(int address, int argCount)
        {
            var getCode = Reflection<CompiledZMachine>.GetMethod("GetCode14", Types.Array<int>());
            il.LoadThis();
            il.Load(address);
            il.Call(getCode);

            il.Load(argCount);
            LoadCallArguments(address, argCount, 14);

            var invoke = Reflection<ZCodeDelegate14>.GetMethod("Invoke", ZCodeParameterTypes.ForDelegate<ZCodeDelegate14>());
            il.Call(invoke);
        }

        private void DirectCall15(int address, int argCount)
        {
            var getCode = Reflection<CompiledZMachine>.GetMethod("GetCode15", Types.Array<int>());
            il.LoadThis();
            il.Load(address);
            il.Call(getCode);

            il.Load(argCount);
            LoadCallArguments(address, argCount, 15);

            var invoke = Reflection<ZCodeDelegate15>.GetMethod("Invoke", ZCodeParameterTypes.ForDelegate<ZCodeDelegate15>());
            il.Call(invoke);
        }

        private void DirectCall_Optimized(Operand addressOp)
        {
            var argCount = currentInstruction.OperandCount - 1;

            if (machine.Profiling)
            {
                // Call Profiler_Call(address, calculated)
                il.LoadThis();
                il.Load(addressOp.Value != 0 ? machine.UnpackRoutineAddress(addressOp.Value) : 0);
                il.Load(false);
                il.Call(Reflection<CompiledZMachine>.GetMethod("Profiler_Call", Types.Array<int, bool>(), @public: false));
            }

            if (addressOp.Value == 0)
            {
                DiscardStackOperands(1, argCount);
                Return(0);
            }
            else
            {
                var address = machine.UnpackRoutineAddress(addressOp.Value);
                var localCount = machine.Memory[address];

                switch (localCount)
                {
                    case 0:
                        DirectCall0(address, argCount);
                        break;
                    case 1:
                        DirectCall1(address, argCount);
                        break;
                    case 2:
                        DirectCall2(address, argCount);
                        break;
                    case 3:
                        DirectCall3(address, argCount);
                        break;
                    case 4:
                        DirectCall4(address, argCount);
                        break;
                    case 5:
                        DirectCall5(address, argCount);
                        break;
                    case 6:
                        DirectCall6(address, argCount);
                        break;
                    case 7:
                        DirectCall7(address, argCount);
                        break;
                    case 8:
                        DirectCall8(address, argCount);
                        break;
                    case 9:
                        DirectCall9(address, argCount);
                        break;
                    case 10:
                        DirectCall10(address, argCount);
                        break;
                    case 11:
                        DirectCall11(address, argCount);
                        break;
                    case 12:
                        DirectCall12(address, argCount);
                        break;
                    case 13:
                        DirectCall13(address, argCount);
                        break;
                    case 14:
                        DirectCall14(address, argCount);
                        break;
                    case 15:
                        DirectCall15(address, argCount);
                        break;
                    default:
                        throw new ZCompilerException(
                            string.Format("Unexpected local count {0} at address {1:x4}", localCount, address));
                }
            }
        }

        internal static ZCodeDelegate0 Compile0(ZRoutine routine, CompiledZMachine machine)
        {
            return new ZCompiler(routine, machine).Compile0();
        }

        internal static ZCodeDelegate1 Compile1(ZRoutine routine, CompiledZMachine machine)
        {
            return new ZCompiler(routine, machine).Compile1();
        }

        internal static ZCodeDelegate2 Compile2(ZRoutine routine, CompiledZMachine machine)
        {
            return new ZCompiler(routine, machine).Compile2();
        }

        internal static ZCodeDelegate3 Compile3(ZRoutine routine, CompiledZMachine machine)
        {
            return new ZCompiler(routine, machine).Compile3();
        }

        internal static ZCodeDelegate4 Compile4(ZRoutine routine, CompiledZMachine machine)
        {
            return new ZCompiler(routine, machine).Compile4();
        }

        internal static ZCodeDelegate5 Compile5(ZRoutine routine, CompiledZMachine machine)
        {
            return new ZCompiler(routine, machine).Compile5();
        }

        internal static ZCodeDelegate6 Compile6(ZRoutine routine, CompiledZMachine machine)
        {
            return new ZCompiler(routine, machine).Compile6();
        }

        internal static ZCodeDelegate7 Compile7(ZRoutine routine, CompiledZMachine machine)
        {
            return new ZCompiler(routine, machine).Compile7();
        }

        internal static ZCodeDelegate8 Compile8(ZRoutine routine, CompiledZMachine machine)
        {
            return new ZCompiler(routine, machine).Compile8();
        }

        internal static ZCodeDelegate9 Compile9(ZRoutine routine, CompiledZMachine machine)
        {
            return new ZCompiler(routine, machine).Compile9();
        }

        internal static ZCodeDelegate10 Compile10(ZRoutine routine, CompiledZMachine machine)
        {
            return new ZCompiler(routine, machine).Compile10();
        }

        internal static ZCodeDelegate11 Compile11(ZRoutine routine, CompiledZMachine machine)
        {
            return new ZCompiler(routine, machine).Compile11();
        }

        internal static ZCodeDelegate12 Compile12(ZRoutine routine, CompiledZMachine machine)
        {
            return new ZCompiler(routine, machine).Compile12();
        }

        internal static ZCodeDelegate13 Compile13(ZRoutine routine, CompiledZMachine machine)
        {
            return new ZCompiler(routine, machine).Compile13();
        }

        internal static ZCodeDelegate14 Compile14(ZRoutine routine, CompiledZMachine machine)
        {
            return new ZCompiler(routine, machine).Compile14();
        }

        internal static ZCodeDelegate15 Compile15(ZRoutine routine, CompiledZMachine machine)
        {
            return new ZCompiler(routine, machine).Compile15();
        }
    }
}
