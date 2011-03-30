using System.Collections.Generic;
using System.Reflection.Emit;
using ZDebug.Compiler.Generate;
using ZDebug.Core.Execution;
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
                    var memoryField = Reflection<ZMachine>.GetField("Memory", @public: false);
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
    }
}
