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
        private const int stackSize = 32768;

        private readonly Story story;
        private readonly Memory memory;
        private readonly byte version;
        private readonly byte[] bytes;
        private readonly ZText ztext;
        private readonly Opcode[] opcodes;

        private int pc;

        private readonly ushort objectTableAddress;
        private readonly ushort maxObjects;
        private readonly byte maxProperties;
        private readonly byte propertyDefaultsTableSize;
        private readonly ushort objectEntriesAddress;
        private readonly byte entrySize;
        private readonly byte attributeBytesSize;
        private readonly byte attributeCount;
        private readonly byte numberSize;
        private readonly byte parentOffset;
        private readonly byte siblingOffset;
        private readonly byte childOffset;
        private readonly byte propertyTableAddressOffset;

        private readonly ZObjectTable objectTable;
        private readonly ushort globalVariableTableAddress;

        // stack and routine call state
        private readonly uint[] stack = new uint[stackSize];
        private int sp = -1;
        private int callFrame = -1;
        private readonly int[] callFrames = new int[stackSize];
        private int callFramePointer = -1;
        private readonly ushort[] locals = new ushort[16];
        private int localCount;
        private uint callAddress;
        private readonly ushort[] arguments = new ushort[8];
        private int argumentCount;
        private bool hasCallStoreVariable;
        private byte callStoreVariable;

        private readonly ushort[] empty = new ushort[0];

        private readonly OutputStreams outputStreams;
        private Random random = new Random();
        private IScreen screen;
        private ISoundEngine soundEngine;
        private IMessageLog messageLog;

        private int instructionCount;
        private int callCount;

        public Processor(Story story)
        {
            this.story = story;
            this.memory = story.Memory;
            this.version = story.Version;
            this.bytes = this.memory.Bytes;
            this.ztext = new ZText(story.Memory);
            this.objectTable = story.ObjectTable;
            this.globalVariableTableAddress = this.memory.ReadGlobalVariableTableAddress();

            this.objectTableAddress = this.memory.ReadObjectTableAddress();
            this.maxObjects = (ushort)(version <= 3 ? 255 : 65535);
            this.maxProperties = (byte)(version <= 3 ? 31 : 63);
            this.propertyDefaultsTableSize = (byte)(maxProperties * 2);
            this.objectEntriesAddress = (ushort)(this.objectTableAddress + propertyDefaultsTableSize);
            this.entrySize = (byte)(version <= 3 ? 9 : 14);
            this.attributeBytesSize = (byte)(version <= 3 ? 4 : 6);
            this.attributeCount = (byte)(version <= 3 ? 32 : 48);
            this.numberSize = (byte)(version <= 3 ? 1 : 2);
            this.parentOffset = (byte)(version <= 3 ? 4 : 6);
            this.siblingOffset = (byte)(version <= 3 ? 5 : 8);
            this.childOffset = (byte)(version <= 3 ? 6 : 10);
            this.propertyTableAddressOffset = (byte)(version <= 3 ? 7 : 12);

            this.outputStreams = new OutputStreams(story);
            RegisterScreen(NullScreen.Instance);
            RegisterSoundEngine(NullSoundEngine.Instance);
            RegisterMessageLog(NullMessageLog.Instance);

            this.pc = this.memory.ReadMainRoutineAddress();
            this.opcodes = OpcodeTables.GetOpcodeTable(this.version).opcodes;

            this.callAddress = (uint)this.pc;
            this.localCount = memory.ReadByte(ref pc);
        }

        private ushort ReadVariableValue(byte variableIndex)
        {
            if (variableIndex < 16)
            {
                if (variableIndex > 0)
                {
                    return locals[variableIndex - 0x01];
                }
                else
                {
                    if (sp == callFrame)
                    {
                        throw new InvalidOperationException("Local stack is empty.");
                    }

                    return (ushort)stack[sp--];
                }
            }
            else // global: variableIndex >= 0x10 && variableIndex <= 0xff
            {
                return bytes.ReadWord(globalVariableTableAddress + ((variableIndex - 0x10) * 2));
            }
        }

        private ushort ReadVariableValueIndirectly(byte variableIndex)
        {
            if (variableIndex < 16)
            {
                if (variableIndex > 0)
                {
                    return locals[variableIndex - 0x01];
                }
                else
                {
                    if (sp == callFrame)
                    {
                        throw new InvalidOperationException("Local stack is empty.");
                    }

                    return (ushort)stack[sp];
                }
            }
            else // global: variableIndex >= 0x10 && variableIndex <= 0xff
            {
                return bytes.ReadWord(globalVariableTableAddress + ((variableIndex - 0x10) * 2));
            }
        }

        private void WriteVariableValue(byte variableIndex, ushort value)
        {
            if (variableIndex == 0x00) // stack
            {
                stack[++sp] = value;
            }
            else if (variableIndex >= 0x01 && variableIndex <= 0x0f) // local
            {
                locals[variableIndex - 0x01] = value;
            }
            else // global: variableIndex >= 0x10 && variableIndex <= 0xff
            {
                var address = globalVariableTableAddress + ((variableIndex - 0x10) * 2);
                bytes.WriteWord(address, value);
            }
        }

        private void WriteVariableValueIndirectly(byte variableIndex, ushort value)
        {
            if (variableIndex == 0x00) // stack
            {
                if (sp == callFrame)
                {
                    throw new InvalidOperationException("Stack is empty.");
                }

                stack[sp] = value;
            }
            else if (variableIndex >= 0x01 && variableIndex <= 0x0f) // local
            {
                locals[variableIndex - 0x01] = value;
            }
            else // global: variableIndex >= 0x10 && variableIndex <= 0xff
            {
                var address = globalVariableTableAddress + ((variableIndex - 0x10) * 2);
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
                // Push values of current frame to the stack. They are pushed in the following order:
                // * call address
                // * argument values (in reverse order)
                // * argument count
                // * local variable values (in reverse order)
                // * local variable count
                // * store variable (encoded as byte; -1 for no variable)
                // * return address

                callFrames[++callFramePointer] = callFrame;

                // reduce field access by storing in local variables
                var stack = this.stack;
                var sp = this.sp;

                stack[++sp] = (uint)callAddress;

                // calculate number of stack slots used to store arguments
                var argCount = this.argumentCount;
                var argSlotCount = (argCount / 2) + (argCount % 2 != 0 ? 1 : 0);

                for (int i = argSlotCount - 1; i >= 0; i--)
                {
                    var index = i * 2;
                    var arg = (uint)((arguments[index] << 16) | arguments[index + 1]);
                    stack[++sp] = arg;
                }

                stack[++sp] = (uint)argCount;

                // calculate number of stack slots used to store locals
                var localCount = this.localCount;
                var locSlotCount = (localCount / 2) + (localCount % 2 != 0 ? 1 : 0);

                for (int i = locSlotCount - 1; i >= 0; i--)
                {
                    var index = i * 2;
                    var loc = (uint)((locals[index] << 16) | locals[index + 1]);
                    stack[++sp] = loc;
                }

                stack[++sp] = (uint)localCount;
                stack[++sp] = hasCallStoreVariable ? (uint)callStoreVariable : unchecked((uint)-1);
                stack[++sp] = (uint)pc;

                callFrame = sp;

                // propogate stack pointer back to field.
                this.sp = sp;

                callAddress = (uint)address;
                var returnAddress = pc;
                pc = address;

                argCount = operandCount - 1;
                this.argumentCount = argCount;

                // read locals
                var locCount = bytes[pc++];
                this.localCount = locCount;

                for (int i = 0; i < argCount; i++)
                {
                    locals[i] = operandValues[i + 1];
                }

                Array.Copy(locals, 0, arguments, 0, argCount);

                if (version <= 4)
                {
                    pc += argCount * 2;
                }

                if (argCount < locCount)
                {
                    if (version <= 4)
                    {
                        for (int i = argCount; i < locCount; i++)
                        {
                            locals[i] = memory.ReadWord(ref pc);
                        }
                    }
                    else
                    {
                        for (int i = argCount; i < locCount; i++)
                        {
                            locals[i] = 0;
                        }
                    }
                }

                hasCallStoreVariable = storeVarIndex >= 0;
                if (hasCallStoreVariable)
                {
                    callStoreVariable = (byte)storeVarIndex;
                }

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

            // Pop values from the stack to set up the current frame. They are popped in the following order:
            // * return address
            // * store variable (encoded as byte; -1 for no variable)
            // * local variable count
            // * local variable values
            // * argument count
            // * argument values (in reverse order)
            // * call address

            // reduce field access by storing in local variables
            var stack = this.stack;
            var sp = this.sp;

            // First, throw away any existing local stack
            sp = callFrame;

            this.pc = (int)stack[sp--];

            var variableIndex = stack[sp--];
            this.hasCallStoreVariable = variableIndex >= 0;
            if (this.hasCallStoreVariable)
            {
                this.callStoreVariable = (byte)variableIndex;
            }

            var localCount = (int)stack[sp--];
            this.localCount = localCount;

            // calculate number of stack slots used to store locals
            var locSlotCount = (localCount / 2) + (localCount % 2 != 0 ? 1 : 0);

            for (int i = 0; i < locSlotCount; i++)
            {
                var loc = stack[sp--];
                var index = i * 2;
                locals[index] = (ushort)((loc >> 16) & 0xffff);
                locals[index + 1] = (ushort)(loc & 0xffff);
            }

            var argCount = (int)stack[sp--];
            this.argumentCount = argCount;

            // calculate number of stack slots used to store locals
            var argSlotCount = (argCount / 2) + (argCount % 2 != 0 ? 1 : 0);

            for (int i = 0; i < argSlotCount; i++)
            {
                var arg = stack[sp--];
                var index = i * 2;
                arguments[index] = (ushort)((arg >> 16) & 0xffff);
                arguments[index + 1] = (ushort)(arg & 0xffff);
            }

            this.callAddress = stack[sp--];

            this.callFrame = callFrames[callFramePointer--];

            // propogate stack pointer back to field.
            this.sp = sp;

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

        private void Branch(bool condition)
        {
            /* Instructions which test a condition are called "branch" instructions. The branch information is
             * stored in one or two bytes, indicating what to do with the result of the test. If bit 7 of the first
             * byte is 0, a branch occurs when the condition was false; if 1, then branch is on true. If bit 6 is set,
             * then the branch occupies 1 byte only, and the "offset" is in the range 0 to 63, given in the bottom
             * 6 bits. If bit 6 is clear, then the offset is a signed 14-bit number given in bits 0 to 5 of the first
             * byte followed by all 8 of the second. */

            byte specifier = bytes[pc++];

            byte offset1 = (byte)(specifier & 0x3f);

            if (!condition)
            {
                specifier ^= 0x80;
            }

            ushort offset;
            if ((specifier & 0x40) == 0) // long branch
            {
                if ((offset1 & 0x20) != 0) // propogate sign bit
                {
                    offset1 |= 0xc0;
                }

                byte offset2 = bytes[pc++];

                offset = (ushort)((offset1 << 8) | offset2);
            }
            else // short branchOffset
            {
                offset = offset1;
            }

            if ((specifier & 0x80) != 0)
            {
                if (offset > 1)
                {
                    pc += (short)offset - 2;
                }
                else
                {
                    Return(offset);
                }
            }
        }

        private void Store(ushort value)
        {
            var storeVariable = bytes[pc++];

            WriteVariableValue(storeVariable, value);
        }

        private string DecodeEmbeddedText()
        {
            int count = 0;
            while (true)
            {
                var zword = bytes.ReadWord(pc + (count++ * 2));
                if ((zword & 0x8000) != 0)
                {
                    break;
                }
            }

            var zwords = memory.ReadWords(ref pc, count);

            return ztext.ZWordsAsString(zwords, ZTextFlags.All);
        }

        private ushort GetObjectAddress(ushort objNum)
        {
            if (objNum < 1)
            {
                throw new ArgumentOutOfRangeException("objNum");
            }

            return (ushort)(this.objectEntriesAddress + ((objNum - 1) * this.entrySize));
        }

        /// <summary>
        /// Given an object number, retrieves the address in memory of its short name.
        /// </summary>
        private ushort GetObjectName(ushort objNum)
        {
            ushort objAddress = GetObjectAddress(objNum);
            objAddress += this.propertyTableAddressOffset;

            return bytes.ReadWord(objAddress);
        }

        /// <summary>
        /// Given an object number, retrieves the address in memory of its first property.
        /// </summary>
        private ushort GetFirstProperty(ushort objNum)
        {
            ushort propAddress = GetObjectName(objNum);
            byte nameLength = bytes[propAddress++];

            return (ushort)(propAddress + (ushort)(nameLength * 2));
        }

        /// <summary>
        /// Given a property address, retrieves the address in memory of the next property.
        /// </summary>
        private ushort GetNextProperty(ushort propAddress)
        {
            byte size = bytes[propAddress++];

            if (this.version <= 3)
            {
                size >>= 5;
            }
            else if ((size & 0x80) != 0x80)
            {
                size >>= 6;
            }
            else
            {
                size = bytes[propAddress];
                size &= 0x3f;

                if (size == 0)
                {
                    size = 64;
                }
            }

            return (ushort)(propAddress + size + 1);
        }

        public int Step()
        {
            ReadNextInstruction();

            Execute();

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
                story.Memory.WriteByte(0x2c, (byte)screen.DefaultBackgroundColor);
                story.Memory.WriteByte(0x2d, (byte)screen.DefaultForegroundColor);
            }

            outputStreams.RegisterScreen(screen);
        }

        public void RegisterSoundEngine(ISoundEngine soundEngine)
        {
            if (soundEngine == null)
            {
                throw new ArgumentNullException("soundEngine");
            }

            this.soundEngine = soundEngine;
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

        public ushort GetOperandValue(int index)
        {
            return operandValues[index];
        }

        private ushort[] GetValues(ref int sp)
        {
            var count = (int)stack[sp--];
            ushort[] values = new ushort[count];

            // calculate number of stack slots used to store locals
            var slotCount = (count / 2) + (count % 2 != 0 ? 1 : 0);

            for (int i = 0; i < slotCount; i++)
            {
                var value = stack[sp--];
                var index = i * 2;
                values[index] = (ushort)((value >> 16) & 0xffff);

                if (index + 1 < count)
                {
                    values[index + 1] = (ushort)(value & 0xffff);
                }
            }

            return values;
        }

        private StackFrame GetStackFrameFromSP(int sp, uint returnAddress)
        {
            var storeVariableIndex = stack[sp--];
            var storeVariable = storeVariableIndex >= 0 ? Variable.FromByte((byte)storeVariableIndex) : null;
            var locals = GetValues(ref sp);
            var arguments = GetValues(ref sp);
            var callAddress = stack[sp--];

            return new StackFrame(callAddress, arguments, locals, returnAddress, storeVariable);
        }

        public StackFrame GetStackFrame(int index)
        {
            if (index == 0)
            {
                var localsCopy = new ushort[localCount];
                Array.Copy(this.locals, localsCopy, localCount);
                var argumentsCopy = new ushort[argumentCount];
                Array.Copy(this.arguments, argumentsCopy, argumentCount);
                var returnAddress = callFrame >= 0 ? stack[callFrame] : 0;
                var storeVariable = hasCallStoreVariable ? Variable.FromByte(callStoreVariable) : null;

                return new StackFrame(this.callAddress, argumentsCopy, localsCopy, returnAddress, storeVariable);
            }
            else if (index == 1 && index < callFramePointer + 2)
            {
                var sp = callFrame - 1; // skipping return address
                var returnAddress = callFramePointer > 0 ? stack[callFrames[callFramePointer]] : 0;

                return GetStackFrameFromSP(sp, returnAddress);
            }
            else if (index > 1 && index < callFramePointer + 2)
            {
                var cfp = callFramePointer - index + 2;
                var sp = callFrames[cfp] - 1; // skipping return address

                var nextSP = callFrames[cfp - 1];
                var returnAddress = nextSP >= 0 ? stack[nextSP] : 0;

                return GetStackFrameFromSP(sp, returnAddress);
            }
            else
            {
                throw new ArgumentOutOfRangeException("index");
            }
        }

        public StackFrame[] GetStackFrames()
        {
            var count = GetStackFrameCount();
            var result = new StackFrame[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = GetStackFrame(i);
            }

            return result;
        }

        public int GetStackFrameCount()
        {
            return callFramePointer + 2;
        }

        public ushort[] GetStackValues()
        {
            var localStackSize = sp - callFrame;
            if (localStackSize == 0)
            {
                return empty;
            }

            var result = new ushort[localStackSize];

            var low = sp == stackSize ? 1 : 0;

            for (int i = localStackSize - 1; i >= low; i--)
            {
                result[i] = (ushort)stack[sp - i];
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

        public event EventHandler<StackFrameEventArgs> EnterStackFrame;
        public event EventHandler<StackFrameEventArgs> ExitStackFrame;

        public event EventHandler Quit;
    }
}