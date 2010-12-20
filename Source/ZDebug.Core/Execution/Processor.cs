using System;
using System.Collections.Generic;
using ZDebug.Core.Basics;
using ZDebug.Core.Dictionary;
using ZDebug.Core.Instructions;
using ZDebug.Core.Text;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Execution
{
    public sealed partial class Processor : IExecutionContext
    {
        private const int stackSize = 1024;

        private readonly Story story;
        private readonly Memory memory;
        private readonly MemoryReader reader;
        private readonly InstructionReader instructions;

        private readonly ushort globalVariableTableAddress;

        // stack and routine call state
        private readonly int[] stack = new int[stackSize];
        private int stackPointer = -1;
        private readonly ushort[] locals = new ushort[15];
        private int localCount;
        private int localStackSize = 0;
        private int argumentCount = 0;
        private Variable storeVariable;

        private ushort[] operandValues = new ushort[8];
        private int operandCount;

        private readonly OutputStreams outputStreams;
        private Random random = new Random();
        private IScreen screen;
        private IMessageLog messageLog;

        private int instructionCount;
        private int callCount;

        private Instruction executingInstruction;

        internal Processor(Story story, InstructionCache cache)
        {
            this.story = story;
            this.memory = story.Memory;
            this.globalVariableTableAddress = memory.ReadGlobalVariableTableAddress();

            this.outputStreams = new OutputStreams(story);
            RegisterScreen(NullScreen.Instance);
            RegisterMessageLog(NullMessageLog.Instance);

            var mainRoutineAddress = story.Memory.ReadMainRoutineAddress();
            this.reader = story.Memory.CreateReader(mainRoutineAddress);
            this.instructions = new InstructionReader(reader, OpcodeTables.GetOpcodeTable(story.Version), cache);

            localCount = reader.NextByte();
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
            stack[++stackPointer] = reader.Address;
            stack[++stackPointer] = storeVariable != null ? storeVariable.ToByte() : -1;
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
            storeVariable = variableIndex >= 0 ? Variable.FromByte((byte)variableIndex) : null;

            reader.Address = stack[stackPointer--];
            localCount = stack[stackPointer--];

            for (int i = 0; i < localCount; i++)
            {
                locals[i] = (ushort)stack[stackPointer--];
            }

            localStackSize = stack[stackPointer--];
            argumentCount = stack[stackPointer--];
        }

        private ushort ReadVariable(Variable variable, bool indirect = false)
        {
            switch (variable.Kind)
            {
                case VariableKind.Stack:
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

                case VariableKind.Local:
                    return locals[variable.Index];

                case VariableKind.Global:
                    return memory.ReadWord(this.globalVariableTableAddress + (variable.Index * 2));

                default:
                    throw new InvalidOperationException();
            }
        }

        private void WriteVariable(Variable variable, ushort value, bool indirect = false)
        {
            switch (variable.Kind)
            {
                case VariableKind.Stack:
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

                    break;

                case VariableKind.Local:
                    {
                        var index = variable.Index;
                        var oldValue = locals[index];
                        locals[index] = value;

                        var handler = LocalVariableChanged;
                        if (handler != null)
                        {
                            handler(this, new VariableChangedEventArgs(index, oldValue, value));
                        }
                    }

                    break;

                case VariableKind.Global:
                    {
                        var index = variable.Index;
                        var address = this.globalVariableTableAddress + (index * 2);
                        var oldValue = memory.ReadWord(address);
                        memory.WriteWord(address, value);

                        var handler = GlobalVariableChanged;
                        if (handler != null)
                        {
                            handler(this, new VariableChangedEventArgs(index, oldValue, value));
                        }
                    }

                    break;

                default:
                    throw new InvalidOperationException();
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
                    return ReadVariable(Variable.FromByte((byte)operand.Value));
                default:
                    throw new InvalidOperationException();
            }
        }

        private void WriteStoreVariable(Variable storeVar, ushort value)
        {
            if (storeVar != null)
            {
                WriteVariable(storeVar, value);
            }
        }

        private void Call(int address, ushort[] opValues, int opCount, Variable storeVar)
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
                WriteStoreVariable(storeVar, 0);
            }
            else
            {
                story.RoutineTable.Add(address);

                PushFrame();

                var returnAddress = reader.Address;
                reader.Address = address;

                argumentCount = opCount - 1;

                // read locals
                localCount = reader.NextByte();
                if (story.Version <= 4)
                {
                    for (int i = 0; i < localCount; i++)
                    {
                        locals[i] = reader.NextWord();
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

                storeVariable = storeVar;
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
            var storeVar = storeVariable;
            var oldAddress = reader.Address;

            PopFrame();

            WriteStoreVariable(storeVar, value);

            var handler = ExitStackFrame;
            if (handler != null)
            {
                handler(this, new StackFrameEventArgs(reader.Address, oldAddress));
            }
        }

        private void Jump(short offset)
        {
            reader.Address += offset - 2;
        }

        private void Jump(Branch branch)
        {
            if (branch.Kind == BranchKind.Address)
            {
                reader.Address += branch.Offset - 2;
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
            var obj = story.ObjectTable.GetByNumber(objNum);
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
            var obj = story.ObjectTable.GetByNumber(objNum);

            return obj.HasAttribute(attrNum);
        }

        public int Step()
        {
            var oldPC = reader.Address;

            var i = instructions.NextInstruction();

            executingInstruction = i;

            var steppingHandler = Stepping;
            if (steppingHandler != null)
            {
                steppingHandler(this, new ProcessorSteppingEventArgs(oldPC));
            }

            var operands = i.Operands;
            operandCount = operands.Length;
            for (int j = 0; j < i.Operands.Length; j++)
            {
                operandValues[j] = GetOperandValue(operands[j]);
            }

            i.Opcode.Execute(i, operandValues, operandCount, this);

            var newPC = reader.Address;

            var steppedHandler = Stepped;
            if (steppedHandler != null)
            {
                steppedHandler(this, new ProcessorSteppedEventArgs(oldPC, newPC));
            }

            executingInstruction = null;

            instructionCount++;

            return newPC;
        }

        private void Screen_DimensionsChanged(object sender, EventArgs e)
        {
            SetScreenDimensions();
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

            if (this.screen != null)
            {
                this.screen.DimensionsChanged -= Screen_DimensionsChanged;
            }

            this.screen = screen;
            this.screen.DimensionsChanged += Screen_DimensionsChanged;
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
            get { return reader.Address; }
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
        /// The Instruction that is being executed (only valid during a step).
        /// </summary>
        public Instruction ExecutingInstruction
        {
            get { return executingInstruction; }
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

        ushort IExecutionContext.GetOperandValue(Operand operand)
        {
            return GetOperandValue(operand);
        }

        byte IExecutionContext.ReadByte(int address)
        {
            return story.Memory.ReadByte(address);
        }

        ushort IExecutionContext.ReadVariable(Variable variable)
        {
            return ReadVariable(variable);
        }

        ushort IExecutionContext.ReadVariableIndirectly(Variable variable)
        {
            return ReadVariable(variable, indirect: true);
        }

        ushort IExecutionContext.ReadWord(int address)
        {
            return story.Memory.ReadWord(address);
        }

        void IExecutionContext.WriteByte(int address, byte value)
        {
            story.Memory.WriteByte(address, value);
        }

        void IExecutionContext.WriteProperty(int objNum, int propNum, ushort value)
        {
            WriteProperty(objNum, propNum, value);
        }

        void IExecutionContext.WriteVariable(Variable variable, ushort value)
        {
            WriteVariable(variable, value);
        }

        void IExecutionContext.WriteVariableIndirectly(Variable variable, ushort value)
        {
            WriteVariable(variable, value, indirect: true);
        }

        void IExecutionContext.WriteWord(int address, ushort value)
        {
            story.Memory.WriteWord(address, value);
        }

        void IExecutionContext.Call(int address, ushort[] opValues, int opCount, Variable storeVariable)
        {
            Call(address, opValues, opCount, storeVariable);
        }

        int IExecutionContext.GetArgumentCount()
        {
            return argumentCount;
        }

        void IExecutionContext.Jump(short offset)
        {
            Jump(offset);
        }

        void IExecutionContext.Jump(Branch branch)
        {
            Jump(branch);
        }

        void IExecutionContext.Return(ushort value)
        {
            Return(value);
        }

        int IExecutionContext.UnpackRoutineAddress(ushort byteAddress)
        {
            return story.UnpackRoutineAddress(byteAddress);
        }

        int IExecutionContext.UnpackStringAddress(ushort byteAddress)
        {
            return story.UnpackStringAddress(byteAddress);
        }

        int IExecutionContext.GetChild(int objNum)
        {
            var obj = story.ObjectTable.GetByNumber(objNum);
            if (!obj.HasChild)
            {
                return 0;
            }
            else
            {
                return obj.Child.Number;
            }
        }

        int IExecutionContext.GetParent(int objNum)
        {
            var obj = story.ObjectTable.GetByNumber(objNum);
            if (!obj.HasParent)
            {
                return 0;
            }
            else
            {
                return obj.Parent.Number;
            }
        }

        int IExecutionContext.GetSibling(int objNum)
        {
            var obj = story.ObjectTable.GetByNumber(objNum);
            if (!obj.HasSibling)
            {
                return 0;
            }
            else
            {
                return obj.Sibling.Number;
            }
        }

        string IExecutionContext.GetShortName(int objNum)
        {
            var obj = story.ObjectTable.GetByNumber(objNum);
            return obj.ShortName;
        }

        int IExecutionContext.GetNextProperty(int objNum, int propNum)
        {
            var obj = story.ObjectTable.GetByNumber(objNum);

            int nextIndex = 0;
            if (propNum > 0)
            {
                var prop = obj.PropertyTable.GetByNumber(propNum);
                if (prop == null)
                {
                    throw new InvalidOperationException();
                }

                nextIndex = prop.Index + 1;
            }

            if (nextIndex == obj.PropertyTable.Count)
            {
                return 0;
            }

            return obj.PropertyTable[nextIndex].Number;
        }

        int IExecutionContext.GetPropertyData(int objNum, int propNum)
        {
            var obj = story.ObjectTable.GetByNumber(objNum);
            var prop = obj.PropertyTable.GetByNumber(propNum);

            if (prop == null)
            {
                return story.ObjectTable.GetPropertyDefault(propNum);
            }

            if (prop.DataLength == 1)
            {
                return story.Memory.ReadByte(prop.DataAddress);
            }
            else if (prop.DataLength == 2)
            {
                return story.Memory.ReadWord(prop.DataAddress);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        int IExecutionContext.GetPropertyDataAddress(int objNum, int propNum)
        {
            var obj = story.ObjectTable.GetByNumber(objNum);
            var prop = obj.PropertyTable.GetByNumber(propNum);

            return prop != null
                ? prop.DataAddress
                : 0;
        }

        int IExecutionContext.GetPropertyDataLength(int dataAddress)
        {
            return story.Memory.ReadPropertyDataLength(dataAddress);
        }

        bool IExecutionContext.HasAttribute(int objNum, int attrNum)
        {
            return HasAttribute(objNum, attrNum);
        }

        void IExecutionContext.ClearAttribute(int objNum, int attrNum)
        {
            var obj = story.ObjectTable.GetByNumber(objNum);
            obj.ClearAttribute(attrNum);
        }

        void IExecutionContext.SetAttribute(int objNum, int attrNum)
        {
            var obj = story.ObjectTable.GetByNumber(objNum);
            obj.SetAttribute(attrNum);
        }

        void IExecutionContext.RemoveFromParent(int objNum)
        {
            story.Memory.RemoveObjectFromParentByNumber(objNum);
        }

        void IExecutionContext.MoveTo(int objNum, int destNum)
        {
            story.Memory.MoveObjectToDestinationByNumber(objNum, destNum);
        }

        ushort[] IExecutionContext.ReadZWords(int address)
        {
            return story.Memory.ReadZWords(address);
        }

        string IExecutionContext.ParseZWords(IList<ushort> zwords)
        {
            return ZText.ZWordsAsString(zwords, ZTextFlags.All, story.Memory);
        }

        void IExecutionContext.SelectScreenStream()
        {
            outputStreams.SelectScreenStream();
        }

        void IExecutionContext.DeselectScreenStream()
        {
            outputStreams.DeselectScreenStream();
        }

        void IExecutionContext.SelectTranscriptStream()
        {
            outputStreams.SelectTranscriptStream();
        }

        void IExecutionContext.DeselectTranscriptStream()
        {
            outputStreams.DeselectTranscriptStream();
        }

        void IExecutionContext.SelectMemoryStream(int address)
        {
            outputStreams.SelectMemoryStream(story.Memory, address);
        }

        void IExecutionContext.DeselectMemoryStream()
        {
            outputStreams.DeselectMemoryStream();
        }

        void IExecutionContext.Print(string text)
        {
            outputStreams.Print(text);
        }

        void IExecutionContext.Print(char ch)
        {
            outputStreams.Print(ch);
        }

        ZCommandToken[] IExecutionContext.TokenizeCommand(string commandText)
        {
            return ZText.TokenizeCommand(commandText, story.Dictionary.WordSeparators.ToArray());
        }

        bool IExecutionContext.TryLookupWord(string word, out ZDictionaryEntry entry)
        {
            return story.Dictionary.TryLookupWord(word, out entry);
        }

        void IExecutionContext.Randomize(int seed)
        {
            random = new Random(seed);
        }

        int IExecutionContext.NextRandom(int range)
        {
            // range should be inclusive, so we need to subtract 1 since System.Range.Next makes it exclusive
            var minValue = 1;
            var maxValue = Math.Max(minValue, range - 1);
            return random.Next(minValue, maxValue);
        }

        void IExecutionContext.Quit()
        {
            var handler = Quit;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        bool IExecutionContext.VerifyChecksum()
        {
            return story.ActualChecksum == story.Memory.ReadChecksum();
        }

        IScreen IExecutionContext.Screen
        {
            get { return screen; }
        }

        IMessageLog IExecutionContext.MessageLog
        {
            get { return messageLog; }
        }
    }
}
