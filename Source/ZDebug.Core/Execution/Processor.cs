using System.Collections.Generic;
using ZDebug.Core.Basics;
using ZDebug.Core.Instructions;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Execution
{
    public sealed class Processor
    {
        private readonly Story story;
        private readonly IMemoryReader reader;
        private readonly InstructionReader instructions;
        private readonly Stack<StackFrame> callStack;

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

        public int PC
        {
            get { return reader.Address; }
        }
    }
}
