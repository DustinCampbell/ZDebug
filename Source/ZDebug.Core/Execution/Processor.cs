using System;
using ZDebug.Core.Basics;
using ZDebug.Core.Instructions;
using ZDebug.Core.Objects;
using ZDebug.Core.Text;

namespace ZDebug.Core.Execution
{
    public sealed partial class Processor
    {
        private const int stackSize = 1024;

        private readonly Story story;
        private readonly Memory memory;
        private readonly byte version;
        private readonly ZText ztext;
        private readonly InstructionReader instructions;
        private readonly OpcodeTable opcodeTable;

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
        private byte? callStoreVariable;

        private readonly OutputStreams outputStreams;
        private Random random = new Random();
        private IScreen screen;
        private IMessageLog messageLog;

        private int instructionCount;
        private int callCount;

        internal Processor(Story story, ZText ztext, InstructionCache cache)
        {
            this.story = story;
            this.memory = story.Memory;
            this.version = story.Version;
            this.ztext = ztext;
            this.objectTable = story.ObjectTable;
            this.globalVariableTableAddress = this.memory.ReadGlobalVariableTableAddress();

            this.outputStreams = new OutputStreams(story);
            RegisterScreen(NullScreen.Instance);
            RegisterMessageLog(NullMessageLog.Instance);

            this.pc = this.memory.ReadMainRoutineAddress();
            this.opcodeTable = OpcodeTables.GetOpcodeTable(this.version);
            this.instructions = new InstructionReader(pc, this.memory, opcodeTable, cache);

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
            stack[++stackPointer] = callStoreVariable ?? -1;
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
            callStoreVariable = variableIndex >= 0 ? (byte?)variableIndex : null;

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
            if (variableIndex == 0x00) // stack
            {
                if (localStackSize == 0)
                {
                    throw new InvalidOperationException("Local stack is empty.");
                }

                if (indirect)
                {
                    return (ushort)stack[stackPointer];
                }
                else
                {
                    localStackSize--;
                    var value = (ushort)stack[stackPointer--];

                    var handler = StackPopped;
                    if (handler != null)
                    {
                        handler(this, new StackEventArgs(value));
                    }

                    return value;
                }
            }
            else if (variableIndex >= 0x01 && variableIndex <= 0x0f) // local
            {
                return locals[variableIndex - 0x01];
            }
            else // global: variableIndex >= 0x10 && variableIndex <= 0xff
            {
                return memory.ReadWord(this.globalVariableTableAddress + ((variableIndex - 0x10) * 2));
            }
        }

        private void WriteVariableValue(byte variableIndex, ushort value, bool indirect = false)
        {
            if (variableIndex == 0x00) // stack
            {
                if (indirect)
                {
                    if (localStackSize == 0)
                    {
                        throw new InvalidOperationException("Stack is empty.");
                    }

                    stack[stackPointer] = value;

                    var handler = StackWritten;
                    if (handler != null)
                    {
                        handler(this, new StackEventArgs(value));
                    }
                }
                else
                {
                    localStackSize++;
                    stack[++stackPointer] = value;

                    var handler = StackPushed;
                    if (handler != null)
                    {
                        handler(this, new StackEventArgs(value));
                    }
                }
            }
            else if (variableIndex >= 0x01 && variableIndex <= 0x0f) // local
            {
                var index = variableIndex - 0x01;
                var oldValue = locals[index];
                locals[index] = value;

                var handler = LocalVariableChanged;
                if (handler != null)
                {
                    handler(this, new VariableChangedEventArgs(index, oldValue, value));
                }
            }
            else // global: variableIndex >= 0x10 && variableIndex <= 0xff
            {
                var index = variableIndex - 0x10;
                var address = this.globalVariableTableAddress + (index * 2);
                var oldValue = memory.ReadWord(address);
                memory.WriteWord(address, value);

                var handler = GlobalVariableChanged;
                if (handler != null)
                {
                    handler(this, new VariableChangedEventArgs(index, oldValue, value));
                }
            }
        }

        private ushort GetOperandValue(Operand operand)
        {
            switch (operand.Kind)
            {
                case OperandKind.LargeConstant:
                    return operand.Value;
                case OperandKind.SmallConstant:
                    return (byte)operand.Value;
                case OperandKind.Variable:
                    return ReadVariableValue((byte)operand.Value);
                default:
                    throw new InvalidOperationException();
            }
        }

        private void WriteStoreVariable(byte? storeVarIndex, ushort value)
        {
            if (storeVarIndex != null)
            {
                WriteVariableValue(storeVarIndex.Value, value);
            }
        }

        private void Call(int address, ushort[] opValues, int opCount, byte? storeVarIndex = null)
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
                WriteStoreVariable(storeVarIndex, 0);
            }
            else
            {
                story.RoutineTable.Add(address);

                PushFrame();

                var returnAddress = pc;
                pc = address;

                argumentCount = opCount - 1;

                // read locals
                localCount = memory.ReadByte(ref pc);
                if (story.Version <= 4)
                {
                    for (int i = 0; i < localCount; i++)
                    {
                        locals[i] = memory.ReadWord(ref pc);
                    }
                }
                else
                {
                    for (int i = 0; i < localCount; i++)
                    {
                        locals[i] = 0;
                    }
                }

                var numberToCopy = Math.Min(argumentCount, locals.Length);
                for (int i = 0; i < numberToCopy; i++)
                {
                    locals[i] = opValues[i + 1];
                }

                callStoreVariable = storeVarIndex;
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
            var storeVar = callStoreVariable;
            var oldAddress = pc;

            PopFrame();

            WriteStoreVariable(storeVar, value);

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

        private void Jump(Branch branch)
        {
            if (branch.Kind == BranchKind.Address)
            {
                pc += branch.Offset - 2;
            }
            else if (branch.Kind == BranchKind.RFalse)
            {
                Return(0);
            }
            else if (branch.Kind == BranchKind.RTrue)
            {
                Return(1);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private void WriteProperty(int objNum, int propNum, ushort value)
        {
            var obj = this.objectTable.GetByNumber(objNum);
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

        private bool HasAttribute(int objNum, int attrNum)
        {
            var obj = this.objectTable.GetByNumber(objNum);

            return obj.HasAttribute(attrNum);
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

            this.outputStreams.RegisterScreen(screen);
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
            this.random = new Random(seed);
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

        /// <summary>
        /// Occurs when a local variable is written to.
        /// </summary>
        public event EventHandler<VariableChangedEventArgs> LocalVariableChanged;

        /// <summary>
        /// Occurs when a global variable is written to.
        /// </summary>
        public event EventHandler<VariableChangedEventArgs> GlobalVariableChanged;

        /// <summary>
        /// Occurs when a value is popped off of the local stack.
        /// </summary>
        public event EventHandler<StackEventArgs> StackPopped;

        /// <summary>
        /// Occurs when a value is pushed onto the local stack.
        /// </summary>
        public event EventHandler<StackEventArgs> StackPushed;

        /// <summary>
        /// Occurs when the top of the local stack is directly written to without popping or pushing.
        /// </summary>
        public event EventHandler<StackEventArgs> StackWritten;

        public event EventHandler Quit;
    }
}
