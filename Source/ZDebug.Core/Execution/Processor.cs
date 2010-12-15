using System;
using System.Collections.Generic;
using ZDebug.Core.Basics;
using ZDebug.Core.Instructions;
using ZDebug.Core.Text;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Execution
{
    public sealed partial class Processor : IExecutionContext
    {
        private const int stackSize = 1024;

        private readonly Story story;
        private readonly MemoryReader reader;
        private readonly InstructionReader instructions;

        private readonly StackFrame[] callStack;
        private int sp;

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

            this.callStack = new StackFrame[stackSize];
            this.sp = stackSize;

            this.outputStreams = new OutputStreams(story);
            RegisterScreen(NullScreen.Instance);
            RegisterMessageLog(NullMessageLog.Instance);

            // create "call" to main routine
            var mainRoutineAddress = story.Memory.ReadMainRoutineAddress();
            this.reader = story.Memory.CreateReader(mainRoutineAddress);
            this.instructions = reader.AsInstructionReader(story.Version, cache);

            var localCount = reader.NextByte();
            var locals = ArrayEx.Create(localCount, i => Value.Zero);

            callStack[--sp] =
                new StackFrame(
                    mainRoutineAddress,
                    arguments: new Value[0],
                    locals: locals,
                    returnAddress: null,
                    storeVariable: null);
        }

        private Value ReadVariable(Variable variable, bool indirect = false)
        {
            switch (variable.Kind)
            {
                case VariableKind.Stack:
                    if (indirect)
                    {
                        return callStack[sp].PeekValue();
                    }
                    else
                    {
                        return callStack[sp].PopValue();
                    }

                case VariableKind.Local:
                    return callStack[sp].Locals[variable.Index];

                case VariableKind.Global:
                    return story.GlobalVariablesTable[variable.Index];

                default:
                    throw new InvalidOperationException();
            }
        }

        private void WriteVariable(Variable variable, Value value, bool indirect = false)
        {
            switch (variable.Kind)
            {
                case VariableKind.Stack:
                    if (indirect)
                    {
                        callStack[sp].PopValue();
                    }

                    callStack[sp].PushValue(value);
                    break;

                case VariableKind.Local:
                    var oldValue = callStack[sp].Locals[variable.Index];
                    callStack[sp].SetLocal(variable.Index, value);
                    OnLocalVariableChanged(variable.Index, oldValue, value);
                    break;

                case VariableKind.Global:
                    story.GlobalVariablesTable[variable.Index] = value;
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        private Value GetOperandValue(Operand operand)
        {
            switch (operand.Kind)
            {
                case OperandKind.LargeConstant:
                    return operand.AsLargeConstant();
                case OperandKind.SmallConstant:
                    return operand.AsSmallConstant();
                case OperandKind.Variable:
                    return ReadVariable(operand.AsVariable());
                default:
                    throw new InvalidOperationException();
            }
        }

        private void WriteStoreVariable(Variable storeVariable, Value value)
        {
            if (storeVariable != null)
            {
                WriteVariable(storeVariable, value);
            }
        }

        private void Call(int address, Operand[] operands, Variable storeVariable)
        {
            if (address < 0)
            {
                throw new ArgumentOutOfRangeException("address");
            }

            // NOTE: argument values must be retrieved in case they manipulate the stack
            Value[] argValues;
            if (operands != null)
            {
                argValues = new Value[operands.Length];
                for (int i = 0; i < operands.Length; i++)
                {
                    argValues[i] = GetOperandValue(operands[i]);
                }
            }
            else
            {
                argValues = new Value[0];
            }

            if (address == 0)
            {
                // SPECIAL CASE: A routine call to packed address 0 is legal: it does nothing and returns false (0). Otherwise it is
                // illegal to call a packed address where no routine is present.

                // If there is a store variable, write 0 to it.
                WriteStoreVariable(storeVariable, Value.Zero);
            }
            else
            {
                if (sp == 0)
                {
                    throw new InvalidOperationException("Stack underflow");
                }

                story.RoutineTable.Add(address);

                var returnAddress = reader.Address;
                reader.Address = address;

                // read locals
                var localCount = reader.NextByte();
                var locals = new Value[localCount];
                if (story.Version <= 4)
                {
                    for (int i = 0; i < localCount; i++)
                    {
                        locals[i] = Value.Number(reader.NextWord());
                    }
                }
                else
                {
                    for (int i = 0; i < localCount; i++)
                    {
                        locals[i] = Value.Zero;
                    }
                }

                var numberToCopy = Math.Min(argValues.Length, locals.Length);
                Array.Copy(argValues, 0, locals, 0, numberToCopy);

                var oldFrame = callStack[sp];
                var newFrame = callStack[--sp];
                if (newFrame != null)
                {
                    newFrame.Initialize(address, argValues, locals, returnAddress, storeVariable);
                }
                else
                {
                    newFrame = new StackFrame(address, argValues, locals, returnAddress, storeVariable);
                    callStack[sp] = newFrame;
                }

                var enterFrameHandler = EnterFrame;
                if (enterFrameHandler != null)
                {
                    enterFrameHandler(this, new StackFrameEventArgs(oldFrame, newFrame));
                }
            }

            callCount++;
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
                Return(Value.Zero);
            }
            else if (branch.Kind == BranchKind.RTrue)
            {
                Return(Value.One);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private void Return(Value value)
        {
            var oldFrame = callStack[sp++];
            var newFrame = callStack[sp];

            var exitFrameHandler = ExitFrame;
            if (exitFrameHandler != null)
            {
                exitFrameHandler(this, new StackFrameEventArgs(oldFrame, newFrame));
            }

            reader.Address = oldFrame.ReturnAddress;

            WriteStoreVariable(oldFrame.StoreVariable, value);
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

        public void Step()
        {
            var oldPC = reader.Address;

            var i = instructions.NextInstruction(); ;

            executingInstruction = i;

            var steppingHandler = Stepping;
            if (steppingHandler != null)
            {
                steppingHandler(this, new ProcessorSteppingEventArgs(oldPC));
            }

            i.Opcode.Execute(i, this);

            var newPC = reader.Address;

            var steppedHandler = Stepped;
            if (steppedHandler != null)
            {
                steppedHandler(this, new ProcessorSteppedEventArgs(oldPC, newPC));
            }

            executingInstruction = null;

            instructionCount++;
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

        public StackFrame CurrentFrame
        {
            get { return callStack[sp]; }
        }

        public int PC
        {
            get { return reader.Address; }
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

        private void OnLocalVariableChanged(int index, Value oldValue, Value newValue)
        {
            var handler = LocalVariableChanged;
            if (handler != null)
            {
                handler(this, new LocalVariableChangedEventArgs(index, oldValue, newValue));
            }
        }

        private void OnQuit()
        {
            var handler = Quit;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public event EventHandler<ProcessorSteppingEventArgs> Stepping;
        public event EventHandler<ProcessorSteppedEventArgs> Stepped;

        public event EventHandler<StackFrameEventArgs> EnterFrame;
        public event EventHandler<StackFrameEventArgs> ExitFrame;

        public event EventHandler<LocalVariableChangedEventArgs> LocalVariableChanged;

        public event EventHandler Quit;

        Value IExecutionContext.GetOperandValue(Operand operand)
        {
            return GetOperandValue(operand);
        }

        Value IExecutionContext.ReadByte(int address)
        {
            return Value.Number(story.Memory.ReadByte(address));
        }

        Value IExecutionContext.ReadVariable(Variable variable)
        {
            return ReadVariable(variable);
        }

        Value IExecutionContext.ReadVariableIndirectly(Variable variable)
        {
            return ReadVariable(variable, indirect: true);
        }

        Value IExecutionContext.ReadWord(int address)
        {
            return Value.Number(story.Memory.ReadWord(address));
        }

        void IExecutionContext.WriteByte(int address, byte value)
        {
            story.Memory.WriteByte(address, value);
        }

        void IExecutionContext.WriteProperty(int objNum, int propNum, ushort value)
        {
            WriteProperty(objNum, propNum, value);
        }

        void IExecutionContext.WriteVariable(Variable variable, Value value)
        {
            WriteVariable(variable, value);
        }

        void IExecutionContext.WriteVariableIndirectly(Variable variable, Value value)
        {
            WriteVariable(variable, value, indirect: true);
        }

        void IExecutionContext.WriteWord(int address, ushort value)
        {
            story.Memory.WriteWord(address, value);
        }

        void IExecutionContext.Call(int address, Operand[] operands, Variable storeVariable)
        {
            Call(address, operands, storeVariable);
        }

        int IExecutionContext.GetArgumentCount()
        {
            return callStack[sp].Arguments.Length;
        }

        void IExecutionContext.Jump(short offset)
        {
            Jump(offset);
        }

        void IExecutionContext.Jump(Branch branch)
        {
            Jump(branch);
        }

        void IExecutionContext.Return(Value value)
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
            var reader = story.Memory.CreateReader(address);
            return reader.NextZWords();
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

        void IExecutionContext.ReadChar(Action<char> callback)
        {
            screen.ReadChar(callback);
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
            OnQuit();
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
