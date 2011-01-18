using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using ZDebug.Core.Instructions;
using ZDebug.Compiler.Generate;

namespace ZDebug.Compiler
{
    public partial class ZCompiler
    {
        /// <summary>
        /// Calculates the address of an object given its 1-based object number.
        /// </summary>
        private ushort CalculateObjectAddress(int objNum)
        {
            return (ushort)(machine.ObjectEntriesAddress + ((objNum - 1) * machine.ObjectEntrySize));
        }

        /// <summary>
        /// Calculates the address of an object its 1-based object number and returns it on the evaluation stack.
        /// </summary>
        /// <param name="objNum">The local variable containing the object number.
        /// If null, the object number is retrieved from the top of the evaluation stack.</param>
        private void CalculateObjectAddress(ILocal objNum = null)
        {
            if (objNum != null)
            {
                objNum.Load();
            }

            il.Subtract(1);
            il.Multiply(machine.ObjectEntrySize);
            il.Add(machine.ObjectEntriesAddress);
            il.ConvertToUInt16();
        }

        private void ReadObjectNumber(ushort address)
        {
            if (machine.Version < 4)
            {
                ReadByte(address);
            }
            else
            {
                ReadWord(address);
            }
        }

        private void ReadObjectNumber()
        {
            if (machine.Version < 4)
            {
                ReadByte();
            }
            else
            {
                ReadWord();
            }
        }

        private void ReadValidObjectNumber(int operandIndex, ILabel failed)
        {
            ReadOperand(operandIndex);

            // Check to see if object number is 0.
            var objNumOk = il.NewLabel();
            il.Duplicate();
            objNumOk.BranchIf(Condition.True, @short: true);

            // TODO: Emit warning messsage to log. For now, just pop the number off the stack.
            il.Pop();

            // Jump to failure branch
            failed.Branch();

            objNumOk.Mark();
        }

        /// <summary>
        /// Reads the number of an object's parent, given its 1-based object number.
        /// </summary>
        private void ReadObjectParent(int objNum)
        {
            var address = (ushort)(CalculateObjectAddress(objNum) + machine.ObjectParentOffset);

            ReadObjectNumber(address);
        }

        /// <summary>
        /// Reads the number of an object's parent, given its 1-based object number.
        /// </summary>
        /// <param name="objNum">The local variable containing the object number.
        /// If null, the object number is retrieved from the top of the evaluation stack.</param>
        private void ReadObjectParent(ILocal objNum = null)
        {
            CalculateObjectAddress(objNum);

            // Add parent number offset to the address on the evaluation stack.
            il.Add(machine.ObjectParentOffset);

            ReadObjectNumber();
        }

        private void ReadObjectParentFromOperand(int operandIndex)
        {
            var op = GetOperand(operandIndex);

            switch (op.Kind)
            {
                case OperandKind.LargeConstant:
                case OperandKind.SmallConstant:
                    ReadObjectParent(op.Value);
                    break;

                case OperandKind.Variable:
                    ReadOperand(operandIndex);
                    ReadObjectParent();
                    break;
            }
        }

        /// <summary>
        /// Reads the number of an object's sibling, given its 1-based object number.
        /// </summary>
        private void ReadObjectSibling(int objNum)
        {
            var address = (ushort)(CalculateObjectAddress(objNum) + machine.ObjectSiblingOffset);

            ReadObjectNumber(address);
        }

        /// <summary>
        /// Reads the number of an object's sibling, given its 1-based object number.
        /// </summary>
        /// <param name="objNum">The local variable containing the object number.
        /// If null, the object number is retrieved from the top of the evaluation stack.</param>
        private void ReadObjectSibling(ILocal objNum = null)
        {
            CalculateObjectAddress(objNum);

            // Add sibling number offset to the address on the evaluation stack.
            il.Add(machine.ObjectSiblingOffset);

            ReadObjectNumber();
        }

        private void ReadObjectSiblingFromOperand(int operandIndex)
        {
            var op = GetOperand(operandIndex);

            switch (op.Kind)
            {
                case OperandKind.LargeConstant:
                case OperandKind.SmallConstant:
                    ReadObjectSibling(op.Value);
                    break;

                case OperandKind.Variable:
                    ReadOperand(operandIndex);
                    ReadObjectSibling();
                    break;
            }
        }

        /// <summary>
        /// Reads the number of an object's child, given its 1-based object number.
        /// </summary>
        private void ReadObjectChild(int objNum)
        {
            var address = (ushort)(CalculateObjectAddress(objNum) + machine.ObjectChildOffset);

            ReadObjectNumber(address);
        }

        /// <summary>
        /// Reads the number of an object's child, given its 1-based object number.
        /// </summary>
        /// <param name="objNum">The local variable containing the object number.
        /// If null, the object number is retrieved from the top of the evaluation stack.</param>
        private void ReadObjectChild(ILocal objNum = null)
        {
            CalculateObjectAddress(objNum);

            // Add child number offset to the address on the evaluation stack.
            il.Add(machine.ObjectChildOffset);

            ReadObjectNumber();
        }

        private void ReadObjectChildFromOperand(int operandIndex)
        {
            var op = GetOperand(operandIndex);

            switch (op.Kind)
            {
                case OperandKind.LargeConstant:
                case OperandKind.SmallConstant:
                    ReadObjectChild(op.Value);
                    break;

                case OperandKind.Variable:
                    ReadOperand(operandIndex);
                    ReadObjectChild();
                    break;
            }
        }

        /// <summary>
        /// Reads the address of an object's property table, given its 1-based object number.
        /// </summary>
        private void ReadObjectPropertyTableAddress(int objNum)
        {
            var address = (ushort)(CalculateObjectAddress(objNum) + machine.ObjectPropertyTableAddressOffset);

            ReadWord(address);
        }

        /// <summary>
        /// Reads the address of an object's property, given its 1-based object number.
        /// </summary>
        /// <param name="objNum">The local variable containing the object number.
        /// If null, the object number is retrieved from the top of the evaluation stack.</param>
        private void ReadObjectPropertyTableAddress(ILocal objNum = null)
        {
            CalculateObjectAddress(objNum);

            // Add property table address offset to the address on the evaluation stack.
            il.Add(machine.ObjectPropertyTableAddressOffset);

            ReadWord();
        }

        private void ObjectHasAttribute(ILocal objNum, ILocal attribute)
        {
            memory.LoadElement(
                loadIndex: il.Generate(() =>
                {
                    CalculateObjectAddress(objNum);
                    attribute.Load();
                    il.Divide(8);
                    il.Add();
                }));

            // bit index
            il.LoadConstant(1);
            il.LoadConstant(7);
            attribute.Load();
            il.LoadConstant(8);
            il.Remainder();
            il.Subtract();
            il.And(0x1f);
            il.Shl();

            // (byte & bit index) != 0;
            il.And();
            il.LoadConstant(0);
            il.CompareEqual();
            il.LoadConstant(0);
            il.CompareEqual();

#if DEBUG
            using (var result = il.NewLocal<bool>())
            {
                result.Store();
                il.DebugWrite("Object {0} has attribute {1} = {2}", objNum, attribute, result);
                result.Load();
            }
#endif
        }

        /// <summary>
        /// Given a property table address on the evaluation stack, puts the address of
        /// its first property on the stack.
        /// </summary>
        private void GetAddressOfFirstProperty()
        {
            using (var propAddress = il.NewLocal<ushort>())
            {
                propAddress.Store();

                ReadByte(propAddress); // name-length
                il.ConvertToUInt16();
                il.Multiply(2);
                propAddress.Load();
                il.Add();
                il.Add(1);
                il.ConvertToUInt16();
            }
        }

        private void FirstProperty(int objNum)
        {
            ReadObjectPropertyTableAddress(objNum);

            GetAddressOfFirstProperty();
        }

        private void FirstProperty(ILocal objNum = null)
        {
            ReadObjectPropertyTableAddress(objNum);

            GetAddressOfFirstProperty();
        }

        private void NextProperty()
        {
            using (var propAddress = il.NewLocal<ushort>())
            using (var size = il.NewLocal<byte>())
            {
                // Stack should contain last property address
                propAddress.Store();

                // read size byte
                ReadByte(propAddress);
                size.Store();

                // increment propAddress
                propAddress.Load();
                il.Add(1);
                propAddress.Store();

                if (machine.Version < 4)
                {
                    // size >>= 5
                    size.Load();
                    il.Shr(5);
                    il.ConvertToUInt8();
                    size.Store();
                }
                else
                {
                    // if ((size & 0x80) != 0x80)
                    size.Store();
                    il.And(0x80);
                    il.LoadConstant(0x80);

                    var secondSizeByte = il.NewLabel();
                    secondSizeByte.BranchIf(Condition.Equal, @short: true);

                    // size >>= 6
                    size.Load();
                    il.Shr(6);
                    il.ConvertToUInt8();
                    size.Store();

                    var done = il.NewLabel();
                    done.Branch(@short: true);

                    secondSizeByte.Mark();

                    // read second size byte
                    ReadByte(propAddress);
                    size.Store();

                    // size &= 0x3f
                    size.Load();
                    il.And(0x3f);
                    il.ConvertToUInt8();
                    size.Store();

                    // if (size == 0)
                    size.Load();
                    done.BranchIf(Condition.True, @short: true);

                    // size = 64
                    il.LoadConstant(64);
                    size.Store();

                    done.Mark();
                }

                // (ushort)(propAddress + size + 1)
                propAddress.Load();
                size.Load();
                il.Add();
                il.Add(1);
                il.ConvertToUInt16();
            }
        }
    }
}
