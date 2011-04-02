using System.Collections.Generic;
using System.Diagnostics;
using ZDebug.Core.Instructions;
using ZDebug.Core.Routines;

namespace ZDebug.Compiler.Analysis.ControlFlow
{
    internal partial class ControlFlowGraph
    {
        private readonly ZRoutine routine;

        private readonly List<CodeBlock> codeBlocks;
        private readonly Block entry;
        private readonly Block exit;

        private int instructionCount;

        private ControlFlowGraph(ZRoutine routine)
        {
            this.routine = routine;

            this.entry = new Block(isEntry: true);
            this.exit = new Block(isExit: true);

            var instructions = new InstructionLinkedList(routine);
            this.codeBlocks = new List<CodeBlock>(BuildGraph(instructions));
        }

        private List<CodeBlock> BuildGraph(InstructionLinkedList instructions)
        {
            var jumpTargets = CollectJumpTargets(instructions);

            var results = new List<CodeBlock>(jumpTargets.Count);
            var codeBlocks = new Dictionary<int, CodeBlock>(jumpTargets.Count);

            foreach (var jumpTarget in jumpTargets)
            {
                var codeBlock = new CodeBlock(jumpTarget);
                results.Add(codeBlock);
                codeBlocks.Add(jumpTarget, codeBlock);
            }

            var node = instructions.First;
            var currentBlock = codeBlocks[node.Value.Address];
            this.entry.AddJumpTarget(currentBlock);

            while (node != null)
            {
                var instruction = node.Value;

                if (codeBlocks.ContainsKey(instruction.Address))
                {
                    currentBlock = codeBlocks[instruction.Address];
                }

                if (currentBlock != null)
                {
                    currentBlock.AddInstruction(instruction);
                    instructionCount++;

                    if (instruction.HasBranch)
                    {
                        // conditional branch
                        switch (instruction.Branch.Kind)
                        {
                            case BranchKind.Address:
                                var address = instruction.Address + instruction.Length + instruction.Branch.Offset - 2;
                                var blockAtAddress = codeBlocks[address];
                                currentBlock.AddJumpTarget(blockAtAddress);
                                break;

                            case BranchKind.RFalse:
                            case BranchKind.RTrue:
                                currentBlock.AddJumpTarget(exit);
                                break;
                        }

                        // add "else" portion of conditional (i.e. the next instruction).
                        if (node.Next != null)
                        {
                            var blockAtAddres = codeBlocks[node.Next.Value.Address];
                            currentBlock.AddJumpTarget(blockAtAddres);
                        }

                        // done with this block
                        currentBlock = null;
                    }
                    else if (instruction.Opcode.IsJump)
                    {
                        // unconditional jump
                        var address = instruction.Address + instruction.Length + (short)instruction.Operands[0].Value - 2;
                        var blockAtAddress = codeBlocks[address];
                        currentBlock.AddJumpTarget(blockAtAddress);

                        // done with this block
                        currentBlock = null;
                    }
                    else if (instruction.Opcode.IsReturn || instruction.Opcode.IsQuit)
                    {
                        currentBlock.AddJumpTarget(exit);

                        // done with this block
                        currentBlock = null;
                    }
                }
                else
                {
                    Debug.WriteLine("Unreachable code encountered at: " + instruction);
                }

                node = node.Next;
            }

            return results;
        }

        private SortedSet<int> CollectJumpTargets(InstructionLinkedList instructions)
        {
            var results = new SortedSet<int>();
            var node = instructions.First;

            // the first instruction is a jump target of the entry block.
            results.Add(node.Value.Address);

            while (node != null)
            {
                var instruction = node.Value;

                if (instruction.HasBranch)
                {
                    // conditional branch
                    if (instruction.Branch.Kind == BranchKind.Address)
                    {
                        var address = instruction.Address + instruction.Length + instruction.Branch.Offset - 2;
                        results.Add(address);
                    }

                    // add "else" portion of conditional (i.e. the next instruction).
                    if (node.Next != null)
                    {
                        results.Add(node.Next.Value.Address);
                    }
                }
                else if (node.Value.Opcode.IsJump)
                {
                    // unconditional jump
                    var address = instruction.Address + instruction.Length + (short)instruction.Operands[0].Value - 2;
                    results.Add(address);
                }

                node = node.Next;
            }

            return results;
        }

        public Block Entry
        {
            get
            {
                return entry;
            }
        }

        public Block Exit
        {
            get
            {
                return exit;
            }
        }

        public IEnumerable<CodeBlock> CodeBlocks
        {
            get
            {
                foreach (var codeBlock in this.codeBlocks)
                {
                    yield return codeBlock;
                }
            }
        }

        public IEnumerable<Instruction> Instructions
        {
            get
            {
                foreach (var codeBlock in this.codeBlocks)
                {
                    foreach (var instruction in codeBlock.Instructions)
                    {
                        yield return instruction;
                    }
                }
            }
        }

        public int InstructionCount
        {
            get
            {
                return this.instructionCount;
            }
        }

        public static ControlFlowGraph Build(ZRoutine routine)
        {
            return new ControlFlowGraph(routine);
        }
    }
}
