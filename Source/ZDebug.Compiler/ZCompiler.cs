using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using ZDebug.Compiler.Analysis.ControlFlow;
using ZDebug.Compiler.CodeGeneration;
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
    internal partial class ZCompiler
    {
        private readonly ZRoutine routine;
        private readonly CompiledZMachine machine;
        private bool debugging;

        private ILBuilder il;
        private ControlFlowGraph controlFlowGraph;
        private Dictionary<int, ILabel> addressToLabelMap;
        private List<ZRoutineCall> calls;

        private bool usesStack;
        private bool usesMemory;

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
                }
            }

            var instructionStatistics = new List<InstructionStatistics>(this.routine.Instructions.Length);

            // Emit IL
            foreach (var codeBlock in this.controlFlowGraph.CodeBlocks)
            {
                var generators = codeBlock.Instructions
                    .Select(i => OpcodeGenerator.GetGenerator(i, machine.Version))
                    .ToList();

                Optimize(generators);

                foreach (var generator in generators)
                {
                    ILabel label;
                    if (this.addressToLabelMap.TryGetValue(generator.Instruction.Address, out label))
                    {
                        label.Mark();
                    }

                    if (machine.Debugging)
                    {
                        il.Arguments.LoadMachine();
                        il.Call(Reflection<CompiledZMachine>.GetMethod("Tick", @public: false));
                    }

                    Profiler_ExecutingInstruction(generator.Instruction);
                    il.DebugWrite(generator.Instruction.PrettyPrint(machine));

                    var offset = il.Size;

                    generator.Generate(il, this);

                    instructionStatistics.Add(new InstructionStatistics(generator.Instruction, offset, il.Size - offset));
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
                calculatedStoreVariableCount,
                instructionStatistics);

            return new ZCompilerResult(this.routine, calls.ToArray(), code, statistics);
        }

        private static void Optimize(List<OpcodeGenerator> generators)
        {
            for (int i = 0; i < generators.Count - 1; i++)
            {
                var generator = generators[i];
                var nextGenerator = generators[i + 1];

                if (generator.CanReuseStoreVariable)
                {
                    OptimizeReuseStoreVariable(generator, nextGenerator);
                }
                else if (generator.CanReuseByRefOperand)
                {
                    OptimizeReuseByRefOperand(generator, nextGenerator);
                }

                if (generator.CanLeaveStoreVariableSigned)
                {
                    OptimizeStoreVariableSign(generator, nextGenerator);
                }
            }
        }

        private static void OptimizeStoreVariableSign(OpcodeGenerator generator, OpcodeGenerator nextGenerator)
        {
            Debug.Assert(generator.Instruction.HasStoreVariable);

            if (nextGenerator.SignsOperands)
            {
                Debug.WriteLine("{0:x4}: Optimizing store variable sign between {1} and {2}",
                    generator.Instruction.Address,
                    generator.Instruction.Opcode.Name,
                    nextGenerator.Instruction.Opcode.Name);

                generator.LeaveStoreVariableSigned = true;
            }
        }

        private static void OptimizeReuseByRefOperand(OpcodeGenerator generator, OpcodeGenerator nextGenerator)
        {
            Debug.Assert(generator.Instruction.Opcode.IsFirstOpByRef);
            Debug.Assert(generator.Instruction.OperandCount > 0);

            var byRefOperand = generator.Instruction.Operands[0];
            if (byRefOperand.Kind == OperandKind.SmallConstant)
            {
                if (nextGenerator.CanReuseFirstOperand || nextGenerator.CanReuseSecondOperand)
                {
                    if (nextGenerator.CanReuseFirstOperand)
                    {
                        Debug.Assert(nextGenerator.Instruction.OperandCount > 0);

                        var firstOperand = nextGenerator.Instruction.Operands[0];
                        if (firstOperand.IsVariable)
                        {
                            if (firstOperand.Value == byRefOperand.Value)
                            {
                                Debug.WriteLine("{0:x4}: Optimizing {1} between {2} and {3}",
                                    generator.Instruction.Address,
                                    Variable.FromByte((byte)byRefOperand.Value),
                                    generator.Instruction.Opcode.Name,
                                    nextGenerator.Instruction.Opcode.Name);

                                generator.ReuseByRefOperand = true;
                                nextGenerator.ReuseFirstOperand = true;
                            }
                        }
                    }

                    if (nextGenerator.CanReuseSecondOperand && !nextGenerator.ReuseFirstOperand)
                    {
                        Debug.Assert(nextGenerator.Instruction.OperandCount > 1);

                        var secondOperand = nextGenerator.Instruction.Operands[1];
                        if (secondOperand.IsVariable)
                        {
                            if (secondOperand.Value == byRefOperand.Value)
                            {
                                Debug.WriteLine("{0:x4}: Optimizing {1} between {2} and {3}",
                                    generator.Instruction.Address,
                                    Variable.FromByte((byte)byRefOperand.Value),
                                    generator.Instruction.Opcode.Name,
                                    nextGenerator.Instruction.Opcode.Name);

                                generator.ReuseByRefOperand = true;
                                nextGenerator.ReuseSecondOperand = true;
                            }
                        }
                    }
                }
                else if (nextGenerator.CanReuseStack && byRefOperand.Value == 0)
                {
                    Debug.WriteLine("{0:x4}: Optimizing {1} between {2} and {3}",
                        generator.Instruction.Address,
                        Variable.FromByte((byte)byRefOperand.Value),
                        generator.Instruction.Opcode.Name,
                        nextGenerator.Instruction.Opcode.Name);

                    generator.ReuseByRefOperand = true;
                    nextGenerator.ReuseStack = true;
                }
            }
        }

        private static void OptimizeReuseStoreVariable(OpcodeGenerator generator, OpcodeGenerator nextGenerator)
        {
            Debug.Assert(generator.Instruction.HasStoreVariable);

            if (nextGenerator.CanReuseFirstOperand || nextGenerator.CanReuseSecondOperand)
            {
                if (nextGenerator.CanReuseFirstOperand)
                {
                    Debug.Assert(nextGenerator.Instruction.OperandCount > 0);

                    var firstOperand = nextGenerator.Instruction.Operands[0];
                    if (firstOperand.IsVariable)
                    {
                        if (firstOperand.Value == generator.Instruction.StoreVariable.ToByte())
                        {
                            Debug.WriteLine("{0:x4}: Optimizing {1} between {2} and {3}",
                                generator.Instruction.Address,
                                generator.Instruction.StoreVariable,
                                generator.Instruction.Opcode.Name,
                                nextGenerator.Instruction.Opcode.Name);

                            generator.ReuseStoreVariable = true;
                            nextGenerator.ReuseFirstOperand = true;
                        }
                    }
                }

                if (nextGenerator.CanReuseSecondOperand && !nextGenerator.ReuseFirstOperand)
                {
                    Debug.Assert(nextGenerator.Instruction.OperandCount > 1);

                    var secondOperand = nextGenerator.Instruction.Operands[1];
                    if (secondOperand.IsVariable)
                    {
                        if (secondOperand.Value == generator.Instruction.StoreVariable.ToByte())
                        {
                            Debug.WriteLine("{0:x4}: Optimizing {1} between {2} and {3}",
                                generator.Instruction.Address,
                                generator.Instruction.StoreVariable,
                                generator.Instruction.Opcode.Name,
                                nextGenerator.Instruction.Opcode.Name);

                            generator.ReuseStoreVariable = true;
                            nextGenerator.ReuseSecondOperand = true;
                        }
                    }
                }
            }
            else if (nextGenerator.CanReuseStack && generator.Instruction.StoreVariable.Kind == VariableKind.Stack)
            {
                Debug.WriteLine("{0:x4}: Optimizing {1} between {2} and {3}",
                    generator.Instruction.Address,
                    generator.Instruction.StoreVariable,
                    generator.Instruction.Opcode.Name,
                    nextGenerator.Instruction.Opcode.Name);

                generator.ReuseStoreVariable = true;
                nextGenerator.ReuseStack = true;
            }
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

        private void Profiler_ExecutingInstruction(Instruction instruction)
        {
            if (machine.Profiling)
            {
                il.Arguments.LoadMachine();
                il.Load(instruction.Address);
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

        public static ZCompilerResult Compile(ZRoutine routine, CompiledZMachine machine)
        {
            return new ZCompiler(routine, machine).Compile();
        }
    }
}
