using System;
using ZDebug.Core.Basics;
using ZDebug.Core.Instructions;
using ZDebug.Core.Objects;
using ZDebug.Core.Text;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Execution
{
    public sealed partial class Processor
    {
        private const int stackSize = 1024;

        private readonly Story story;
        private readonly Memory memory;
        private readonly byte version;
        private readonly byte[] bytes;
        private readonly ZText ztext;
        private readonly Opcode[] opcodes;

        private int pc;

        private readonly ZObjectTable objectTable;
        private readonly ushort globalVariableTableAddress;

        // stack and routine call state
        private readonly int[] stack = new int[stackSize];
        private int stackPointer = -1;
        private readonly ushort[] locals = new ushort[15];
        private int localCount;
        private int localStackSize = 0;
        private int argumentCount = 0;
        private bool hasCallStoreVariable;
        private byte callStoreVariable;

        private readonly OutputStreams outputStreams;
        private Random random = new Random();
        private IScreen screen;
        private IMessageLog messageLog;

        private int instructionCount;
        private int callCount;

        internal Processor(Story story, ZText ztext)
        {
            this.story = story;
            this.memory = story.Memory;
            this.version = story.Version;
            this.bytes = this.memory.Bytes;
            this.ztext = ztext;
            this.objectTable = story.ObjectTable;
            this.globalVariableTableAddress = this.memory.ReadGlobalVariableTableAddress();

            this.outputStreams = new OutputStreams(story);
            RegisterScreen(NullScreen.Instance);
            RegisterMessageLog(NullMessageLog.Instance);

            this.pc = this.memory.ReadMainRoutineAddress();
            this.opcodes = OpcodeTables.GetOpcodeTable(this.version).opcodes;

            this.localCount = memory.ReadByte(ref pc);
        }

        /// <summary>
        /// Push values of current frame to the stack. They are pushed in the following order:
        /// 
        /// * argument count
        /// * local stack size
        /// * local variable values (in reverse order)
        /// * local variable count
        /// * return address
        /// * store variable (encoded as byte; -1 for no variable)
        /// </summary>
        private void PushFrame()
        {
            stack[++stackPointer] = argumentCount;
            stack[++stackPointer] = localStackSize;

            for (int i = localCount - 1; i >= 0; i--)
            {
                stack[++stackPointer] = locals[i];
            }

            stack[++stackPointer] = localCount;
            stack[++stackPointer] = pc;
            stack[++stackPointer] = hasCallStoreVariable ? callStoreVariable : -1;
        }

        /// <summary>
        /// Pop values from the stack to set up the current frame. They are popped in the following order:
        /// 
        /// * store variable (encoded as byte; -1 for no variable)
        /// * return address
        /// * local variable count
        /// * local variable values
        /// * local stack size
        /// * argument count
        /// </summary>
        private void PopFrame()
        {
            // First, throw away any existing local stack
            while (localStackSize-- > 0)
            {
                stackPointer--;
            }

            var variableIndex = stack[stackPointer--];
            hasCallStoreVariable = variableIndex >= 0;
            if (hasCallStoreVariable)
            {
                callStoreVariable = (byte)variableIndex;
            }

            pc = stack[stackPointer--];
            localCount = stack[stackPointer--];

            for (int i = 0; i < localCount; i++)
            {
                locals[i] = (ushort)stack[stackPointer--];
            }

            localStackSize = stack[stackPointer--];
            argumentCount = stack[stackPointer--];
        }

        private ushort ReadVariableValue(byte variableIndex, bool indirect = false)
        {
            if (variableIndex < 16)
            {
                if (variableIndex > 0)
                {
                    return locals[variableIndex - 0x01];
                }
                else
                {
                    if (localStackSize == 0)
                    {
                        throw new InvalidOperationException("Local stack is empty.");
                    }

                    if (!indirect)
                    {
                        localStackSize--;
                        return (ushort)stack[stackPointer--];
                    }
                    else
                    {
                        return (ushort)stack[stackPointer];
                    }
                }
            }
            else // global: variableIndex >= 0x10 && variableIndex <= 0xff
            {
                return bytes.ReadWord(this.globalVariableTableAddress + ((variableIndex - 0x10) * 2));
            }
        }

        private void WriteVariableValue(byte variableIndex, ushort value, bool indirect = false)
        {
            if (variableIndex == 0x00) // stack
            {
                if (!indirect)
                {
                    localStackSize++;
                    stack[++stackPointer] = value;
                }
                else
                {
                    if (localStackSize == 0)
                    {
                        throw new InvalidOperationException("Stack is empty.");
                    }

                    stack[stackPointer] = value;
                }
            }
            else if (variableIndex >= 0x01 && variableIndex <= 0x0f) // local
            {
                locals[variableIndex - 0x01] = value;
            }
            else // global: variableIndex >= 0x10 && variableIndex <= 0xff
            {
                var address = this.globalVariableTableAddress + ((variableIndex - 0x10) * 2);
                bytes.WriteWord(address, value);
            }
        }

        private void Call(int address, short storeVarIndex = -1)
        {
            if (address < 0)
            {
                throw new ArgumentOutOfRangeException("address");
            }

            if (address == 0)
            {
                // SPECIAL CASE: A routine call to packed address 0 is legal: it does nothing and returns false (0). Otherwise it is
                // illegal to call a packed address where no routine is present.

                // If there is a store variable, write 0 to it.
                if (storeVarIndex >= 0)
                {
                    WriteVariableValue((byte)storeVarIndex, 0);
                }
            }
            else
            {
                //story.RoutineTable.Add(address);

                PushFrame();

                var returnAddress = pc;
                pc = address;

                argumentCount = operandCount - 1;

                // read locals
                this.localCount = bytes[pc++];
                if (version <= 4)
                {
                    for (int i = 0; i < localCount; i++)
                    {
                        locals[i] = memory.ReadWord(ref pc);
                    }
                }
                else
                {
                    Array.Clear(locals, 0, localCount);
                }

                var numberToCopy = Math.Min(argumentCount, locals.Length);
                Array.Copy(operandValues, 1, locals, 0, numberToCopy);

                hasCallStoreVariable = storeVarIndex >= 0;
                if (hasCallStoreVariable)
                {
                    callStoreVariable = (byte)storeVarIndex;
                }

                localStackSize = 0;

                var handler = EnterStackFrame;
                if (handler != null)
                {
                    handler(this, new StackFrameEventArgs(address, returnAddress));
                }
            }

            callCount++;
        }

        private void Return(ushort value)
        {
            var hasStoreVar = hasCallStoreVariable;
            var storeVar = callStoreVariable;
            var oldAddress = pc;

            PopFrame();

            if (hasStoreVar)
            {
                WriteVariableValue(storeVar, value);
            }

            var handler = ExitStackFrame;
            if (handler != null)
            {
                handler(this, new StackFrameEventArgs(pc, oldAddress));
            }
        }

        private void Jump(short offset)
        {
            pc += offset - 2;
        }

        private void Branch(bool condition)
        {
            if (branchCondition == condition)
            {
                if (branchOffset == 0)
                {
                    Return(0);
                }
                else if (branchOffset == 1)
                {
                    Return(1);
                }
                else
                {
                    pc += branchOffset - 2;
                }
            }
        }

        private void Store(ushort value)
        {
            WriteVariableValue(storeVariable, value);
        }

        private void WriteProperty(int objNum, int propNum, ushort value)
        {
            var obj = objectTable.GetByNumber(objNum);
            var prop = obj.PropertyTable.GetByNumber(propNum);

            if (prop.DataLength == 2)
            {
                story.Memory.WriteWord(prop.DataAddress, value);
            }
            else if (prop.DataLength == 1)
            {
                story.Memory.WriteByte(prop.DataAddress, (byte)(value & 0x00ff));
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public int Step()
        {
            var steppingHandler = Stepping;
            if (steppingHandler != null)
            {
                steppingHandler(this, new ProcessorSteppingEventArgs(pc));
            }

            ReadNextInstruction();

            Execute();

            var steppedHandler = Stepped;
            if (steppedHandler != null)
            {
                steppedHandler(this, new ProcessorSteppedEventArgs(startAddress, pc));
            }

            instructionCount++;

            return pc;
        }

        private void SetScreenDimensions()
        {
            if (story.Version >= 4)
            {
                story.Memory.WriteScreenHeightInLines(screen.ScreenHeightInLines);
                story.Memory.WriteScreenWidthInColumns(screen.ScreenWidthInColumns);
            }

            if (story.Version >= 5)
            {
                story.Memory.WriteScreenHeightInUnits(screen.ScreenHeightInUnits);
                story.Memory.WriteScreenWidthInUnits(screen.ScreenWidthInUnits);
                story.Memory.WriteFontHeightInUnits(screen.FontHeightInUnits);
                story.Memory.WriteFontWidthInUnits(screen.FontWidthInUnits);
            }
        }

        public void RegisterScreen(IScreen screen)
        {
            if (screen == null)
            {
                throw new ArgumentNullException("screen");
            }

            this.screen = screen;
            SetScreenDimensions();

            if (story.Version >= 5)
            {
                var flags1 = story.Memory.ReadByte(0x01);
                flags1 = screen.SupportsColors
                    ? (byte)(flags1 | 0x01)
                    : (byte)(flags1 & ~0x01);
                story.Memory.WriteByte(0x01, flags1);

                story.Memory.WriteByte(0x2c, (byte)screen.DefaultBackgroundColor);
                story.Memory.WriteByte(0x2d, (byte)screen.DefaultForegroundColor);
            }

            if (story.Version >= 4)
            {
                var flags1 = story.Memory.ReadByte(0x01);

                flags1 = screen.SupportsBold
                    ? (byte)(flags1 | 0x04)
                    : (byte)(flags1 & ~0x04);

                flags1 = screen.SupportsItalic
                    ? (byte)(flags1 | 0x08)
                    : (byte)(flags1 & ~0x08);

                flags1 = screen.SupportsFixedFont
                    ? (byte)(flags1 | 0x10)
                    : (byte)(flags1 & ~0x10);

                story.Memory.WriteByte(0x01, flags1);
            }

            outputStreams.RegisterScreen(screen);
        }

        public void RegisterMessageLog(IMessageLog messageLog)
        {
            if (messageLog == null)
            {
                throw new ArgumentNullException("messageLog");
            }

            this.messageLog = messageLog;
        }

        public void SetRandomSeed(int seed)
        {
            random = new Random(seed);
        }

        public int PC
        {
            get { return pc; }
        }

        public ushort[] Locals
        {
            get { return locals; }
        }

        public int LocalCount
        {
            get { return localCount; }
        }

        public ushort[] GetStackValues()
        {
            var result = new ushort[localStackSize];

            for (int i = localStackSize - 1; i >= 0; i--)
            {
                result[i] = (ushort)stack[stackPointer - i];
            }

            return result;
        }

        public int InstructionCount
        {
            get { return instructionCount; }
        }

        public int CallCount
        {
            get { return callCount; }
        }

        /// <summary>
        /// The address of the instruction that is being executed (only valid during a step).
        /// </summary>
        public int ExecutingAddress
        {
            get { return startAddress; }
        }

        public event EventHandler<ProcessorSteppingEventArgs> Stepping;
        public event EventHandler<ProcessorSteppedEventArgs> Stepped;

        public event EventHandler<StackFrameEventArgs> EnterStackFrame;
        public event EventHandler<StackFrameEventArgs> ExitStackFrame;

        public event EventHandler Quit;
    }
}
