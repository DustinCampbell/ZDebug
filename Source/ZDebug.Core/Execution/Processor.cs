using System;
using System.Collections.Generic;
using ZDebug.Core.Basics;
using ZDebug.Core.Instructions;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Execution
{
    public sealed class Processor : IExecutionContext
    {
        private readonly Story story;
        private readonly IMemoryReader reader;
        private readonly InstructionReader instructions;
        private readonly Stack<StackFrame> callStack;

        private Instruction executingInstruction;

        internal Processor(Story story)
        {
            this.story = story;

            this.callStack = new Stack<StackFrame>();

            // create "call" to main routine
            var mainRoutineAddress = story.Memory.ReadMainRoutineAddress();
            this.reader = story.Memory.CreateReader(mainRoutineAddress);
            this.instructions = reader.AsInstructionReader(story.Version);

            var localCount = reader.NextByte();
            var locals = ArrayEx.Create(localCount, i => Value.Zero);

            callStack.Push(
                new StackFrame(
                    mainRoutineAddress,
                    arguments: ArrayEx.Empty<Value>(),
                    locals: locals,
                    returnAddress: null,
                    storeVariable: null));
        }

        public void Step()
        {
            var oldPC = reader.Address;
            executingInstruction = instructions.NextInstruction();

            var steppingHandler = Stepping;
            if (steppingHandler != null)
            {
                steppingHandler(this, new ProcessorSteppingEventArgs(oldPC));
            }

            executingInstruction.Opcode.Execute(executingInstruction, this);

            var newPC = reader.Address;

            var steppedHandler = Stepped;
            if (steppedHandler != null)
            {
                steppedHandler(this, new ProcessorSteppedEventArgs(oldPC, newPC));
            }

            executingInstruction = null;
        }

        public int PC
        {
            get { return reader.Address; }
        }

        /// <summary>
        /// The Instruction that is being executed (only valid during a step).
        /// </summary>
        public Instruction ExecutingInstruction
        {
            get { return executingInstruction; }
        }

        public event EventHandler<ProcessorSteppingEventArgs> Stepping;
        public event EventHandler<ProcessorSteppedEventArgs> Stepped;
    }
}
