using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZDebug.Core;
using ZDebug.Core.Instructions;
using System.Collections.ObjectModel;
using ZDebug.Core.Basics;
using System.Collections;
using ZDebug.Core.Utilities;
using ZDebug.Compiler.Collections;

namespace ZDebug.Compiler
{
    public sealed class ZRoutine
    {
        private readonly int address;
        private readonly int length;
        private readonly ReadOnlyArray<Instruction> instructions;
        private readonly ReadOnlyArray<ushort> locals;

        private ZRoutine(int address, Instruction[] instructions, ushort[] locals)
        {
            this.address = address;
            this.instructions = new ReadOnlyArray<Instruction>(instructions);
            this.locals = new ReadOnlyArray<ushort>(locals);

            if (instructions.Length > 0)
            {
                var last = instructions[instructions.Length - 1];
                this.length = last.Address + last.Length - address + 1;
            }
            else
            {
                this.length = 0;
            }
        }

        public int Address
        {
            get { return address; }
        }

        public int Length
        {
            get { return length; }
        }

        public ReadOnlyArray<Instruction> Instructions
        {
            get { return instructions; }
        }

        public ReadOnlyArray<ushort> Locals
        {
            get { return locals; }
        }

        private static ushort[] ReadLocals(ref int address, byte[] memory, byte version)
        {
            var localCount = memory.ReadByte(ref address);
            if (version <= 4)
            {
                var locals = memory.ReadWords(ref address, localCount);
                return locals;
            }
            else
            {
                return new ushort[localCount];
            }
        }

        private static Instruction[] ReadInstructions(int address, byte[] memory)
        {
            var reader = new InstructionReader(address, memory);
            var instructions = new List<Instruction>();
            var lastKnownAddress = address;

            while (true)
            {
                var i = reader.NextInstruction();

                instructions.Add(i);

                if ((i.Opcode.IsReturn || i.Opcode.IsQuit) && reader.Address > lastKnownAddress)
                {
                    break;
                }
                else if (i.Opcode.IsJump)
                {
                    var jumpOffset = (short)i.Operands[0].Value;
                    var jumpAddress = reader.Address + jumpOffset - 2;
                    if (jumpAddress > lastKnownAddress)
                    {
                        lastKnownAddress = jumpAddress;
                    }

                    if (reader.Address > lastKnownAddress)
                    {
                        break;
                    }
                }
                else if (i.HasBranch && i.Branch.Kind == BranchKind.Address)
                {
                    var branchAddress = reader.Address + i.Branch.Offset - 2;
                    if (branchAddress > lastKnownAddress)
                    {
                        lastKnownAddress = branchAddress;
                    }
                }
            }

            return instructions.ToArray();
        }

        public static ZRoutine Create(int address, byte[] memory)
        {
            var version = memory.ReadByte(0);

            var startAddress = address;
            var locals = ReadLocals(ref address, memory, version);
            var instructions = ReadInstructions(address, memory);

            return new ZRoutine(startAddress, instructions, locals);
        }
    }
}
