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

            il.Math.Subtract(1);
            il.Math.Multiply(machine.ObjectEntrySize);
            il.Math.Add(machine.ObjectEntriesAddress);
            il.Convert.ToUInt16();
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

        private void WriteObjectNumber(ushort address, ushort value)
        {
            if (machine.Version < 4)
            {
                WriteByte(address, value);
            }
            else
            {
                WriteWord(address, value);
            }
        }

        private void WriteObjectNumber(ushort address, ILocal value)
        {
            if (machine.Version < 4)
            {
                WriteByte(address, value);
            }
            else
            {
                WriteWord(address, value);
            }
        }

        private void WriteObjectNumber(ILocal address, ILocal value)
        {
            if (machine.Version < 4)
            {
                WriteByte(address, value);
            }
            else
            {
                WriteWord(address, value);
            }
        }

        private void WriteObjectNumber(ILocal address, ushort value)
        {
            if (machine.Version < 4)
            {
                WriteByte(address, value);
            }
            else
            {
                WriteWord(address, value);
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
            il.Math.Add(machine.ObjectParentOffset);

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
        /// Writes the number of an object's parent, given its 1-based object number.
        /// </summary>
        private void WriteObjectParent(int objNum, ILocal value)
        {
            var address = (ushort)(CalculateObjectAddress(objNum) + machine.ObjectParentOffset);

            WriteObjectNumber(address, value);
        }

        /// <summary>
        /// Writes the number of an object's parent, given its 1-based object number.
        /// </summary>
        private void WriteObjectParent(ILocal objNum, ILocal value)
        {
            CalculateObjectAddress(objNum);

            // Add parent number offset to the address on the evaluation stack.
            il.Math.Add(machine.ObjectParentOffset);

            using (var address = il.NewLocal<ushort>())
            {
                address.Store();
                WriteObjectNumber(address, value);
            }
        }

        /// <summary>
        /// Writes the number of an object's parent, given its 1-based object number.
        /// </summary>
        private void WriteObjectParent(ILocal objNum, ushort value)
        {
            CalculateObjectAddress(objNum);

            // Add parent number offset to the address on the evaluation stack.
            il.Math.Add(machine.ObjectParentOffset);

            using (var address = il.NewLocal<ushort>())
            {
                address.Store();
                WriteObjectNumber(address, value);
            }
        }

        private void WriteObjectParentFromOperand(int operandIndex, ILocal value)
        {
            var op = GetOperand(operandIndex);

            switch (op.Kind)
            {
                case OperandKind.LargeConstant:
                case OperandKind.SmallConstant:
                    WriteObjectParent(op.Value, value);
                    break;

                case OperandKind.Variable:
                    ReadOperand(operandIndex);
                    using (var objNum = il.NewLocal<ushort>())
                    {
                        WriteObjectParent(objNum, value);
                    }
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
            il.Math.Add(machine.ObjectSiblingOffset);

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
        /// Writes the number of an object's sibling, given its 1-based object number.
        /// </summary>
        private void WriteObjectSibling(int objNum, ILocal value)
        {
            var address = (ushort)(CalculateObjectAddress(objNum) + machine.ObjectSiblingOffset);

            WriteObjectNumber(address, value);
        }

        /// <summary>
        /// Writes the number of an object's sibling, given its 1-based object number.
        /// </summary>
        private void WriteObjectSibling(ILocal objNum, ILocal value)
        {
            CalculateObjectAddress(objNum);

            // Add sibling number offset to the address on the evaluation stack.
            il.Math.Add(machine.ObjectSiblingOffset);

            using (var address = il.NewLocal<ushort>())
            {
                address.Store();
                WriteObjectNumber(address, value);
            }
        }

        /// <summary>
        /// Writes the number of an object's sibling, given its 1-based object number.
        /// </summary>
        private void WriteObjectSibling(ILocal objNum, ushort value)
        {
            CalculateObjectAddress(objNum);

            // Add sibling number offset to the address on the evaluation stack.
            il.Math.Add(machine.ObjectSiblingOffset);

            using (var address = il.NewLocal<ushort>())
            {
                address.Store();
                WriteObjectNumber(address, value);
            }
        }

        private void WriteObjectSiblingFromOperand(int operandIndex, ILocal value)
        {
            var op = GetOperand(operandIndex);

            switch (op.Kind)
            {
                case OperandKind.LargeConstant:
                case OperandKind.SmallConstant:
                    WriteObjectSibling(op.Value, value);
                    break;

                case OperandKind.Variable:
                    ReadOperand(operandIndex);
                    using (var objNum = il.NewLocal<ushort>())
                    {
                        WriteObjectSibling(objNum, value);
                    }
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
            il.Math.Add(machine.ObjectChildOffset);

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
        /// Writes the number of an object's child, given its 1-based object number.
        /// </summary>
        private void WriteObjectChild(int objNum, ILocal value)
        {
            var address = (ushort)(CalculateObjectAddress(objNum) + machine.ObjectChildOffset);

            WriteObjectNumber(address, value);
        }

        /// <summary>
        /// Writes the number of an object's child, given its 1-based object number.
        /// </summary>
        private void WriteObjectChild(ILocal objNum, ILocal value)
        {
            CalculateObjectAddress(objNum);

            // Add child number offset to the address on the evaluation stack.
            il.Math.Add(machine.ObjectChildOffset);

            using (var address = il.NewLocal<ushort>())
            {
                address.Store();
                WriteObjectNumber(address, value);
            }
        }

        /// <summary>
        /// Writes the number of an object's child, given its 1-based object number.
        /// </summary>
        private void WriteObjectChild(ILocal objNum, ushort value)
        {
            CalculateObjectAddress(objNum);

            // Add child number offset to the address on the evaluation stack.
            il.Math.Add(machine.ObjectChildOffset);

            using (var address = il.NewLocal<ushort>())
            {
                address.Store();
                WriteObjectNumber(address, value);
            }
        }

        private void WriteObjectChildFromOperand(int operandIndex, ILocal value)
        {
            var op = GetOperand(operandIndex);

            switch (op.Kind)
            {
                case OperandKind.LargeConstant:
                case OperandKind.SmallConstant:
                    WriteObjectChild(op.Value, value);
                    break;

                case OperandKind.Variable:
                    ReadOperand(operandIndex);
                    using (var objNum = il.NewLocal<ushort>())
                    {
                        WriteObjectChild(objNum, value);
                    }
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
            il.Math.Add(machine.ObjectPropertyTableAddressOffset);

            ReadWord();
        }

        private void ReadObjectPropertyTableAddressFromOperand(int operandIndex)
        {
            var op = GetOperand(operandIndex);

            switch (op.Kind)
            {
                case OperandKind.LargeConstant:
                case OperandKind.SmallConstant:
                    ReadObjectPropertyTableAddress(op.Value);
                    break;

                default: // OperandKind.Variable
                    ReadVariable((byte)op.Value);
                    ReadObjectPropertyTableAddress();
                    break;
            }
        }

        private void ReadObjectShortName()
        {
            // An object's short name is the first thing store in its property table.
            // It is simply a byte indicated the length in words of the short name,
            // followed immediately the words.

            using (var address = il.NewLocal<ushort>())
            using (var length = il.NewLocal<byte>())
            using (var zwords = il.NewArrayLocal<ushort>())
            using (var index = il.NewArrayLocal<int>())
            using (var value = il.NewArrayLocal<ushort>())
            {
                address.Store();

                ReadByte(address);
                length.Store();
                zwords.Create(length);

                il.Math.Increment(address);

                var loopStart = il.NewLabel();
                var loopDone = il.NewLabel();

                il.Load(0);
                index.Store();

                loopStart.Mark();

                index.Load();
                length.Load();
                loopDone.BranchIf(Condition.AtLeast);

                ReadWord(address);
                value.Store();

                zwords.StoreElement(
                    loadIndex: il.GenerateLoad(index),
                    loadValue: il.GenerateLoad(value));

                il.Math.Increment(index);
                il.Math.Increment(address, 2);
                loopStart.Branch();

                loopDone.Mark();

                zwords.Load();
            }
        }

        private void ReadObjectShortName(int objNum)
        {
            ReadObjectPropertyTableAddress(objNum);
            ReadObjectShortName();
        }

        private void ReadObjectShortName(ILocal objNum)
        {
            ReadObjectPropertyTableAddress(objNum);
            ReadObjectShortName();
        }

        private void ReadObjectShortNameFromOperand(int operandIndex)
        {
            ReadObjectPropertyTableAddressFromOperand(operandIndex);
            ReadObjectShortName();
        }

        private void ObjectHasAttribute(ILocal objNum, ILocal attribute)
        {
            memory.LoadElement(
                loadIndex: il.Generate(() =>
                {
                    CalculateObjectAddress(objNum);
                    attribute.Load();
                    il.Math.Divide(8);
                    il.Math.Add();
                }));

            // bit index
            il.Load(1);
            il.Load(7);
            attribute.Load();
            il.Load(8);
            il.Math.Remainder();
            il.Math.Subtract();
            il.Math.And(0x1f);
            il.Math.Shl();

            // (byte & bit index) != 0;
            il.Math.And();
            il.Load(0);
            il.Compare.Equal();
            il.Load(0);
            il.Compare.Equal();

#if DEBUG
            using (var result = il.NewLocal<bool>())
            {
                result.Store();
                il.DebugWrite("Object {0} has attribute {1} = {2}", objNum, attribute, result);
                result.Load();
            }
#endif
        }

        private void ObjectSetAttribute(ILocal objNum, ILocal attribute, bool value)
        {
            using (var address = il.NewLocal<int>())
            using (var bitMask = il.NewLocal<int>())
            using (var byteValue = il.NewLocal<byte>())
            {
                // address
                CalculateObjectAddress(objNum);
                attribute.Load();
                il.Math.Divide(8);
                il.Math.Add();
                address.Store();

                // bit mask
                il.Load(1);
                il.Load(7);
                attribute.Load();
                il.Load(8);
                il.Math.Remainder();
                il.Math.Subtract();
                il.Math.And(0x1f);
                il.Math.Shl();
                bitMask.Store();

                // load byte value
                memory.LoadElement(il.GenerateLoad(address));
                byteValue.Store();

                // calculate
                byteValue.Load();
                bitMask.Load();

                if (value)
                {
                    il.Math.Or();
                }
                else
                {
                    il.Math.Not();
                    il.Math.And();
                }

                il.Convert.ToUInt8();

                byteValue.Store();
                memory.StoreElement(
                    loadIndex: il.GenerateLoad(address), 
                    loadValue: il.GenerateLoad(byteValue));
            }
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
                il.Convert.ToUInt16();
                il.Math.Multiply(2);
                propAddress.Load();
                il.Math.Add();
                il.Math.Add(1);
                il.Convert.ToUInt16();
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
                il.Math.Add(1);
                propAddress.Store();

                if (machine.Version < 4)
                {
                    // size >>= 5
                    size.Load();
                    il.Math.Shr(5);
                    il.Convert.ToUInt8();
                    size.Store();
                }
                else
                {
                    // if ((size & 0x80) != 0x80)
                    size.Load();
                    il.Math.And(0x80);
                    il.Load(0x80);

                    var secondSizeByte = il.NewLabel();
                    secondSizeByte.BranchIf(Condition.Equal, @short: true);

                    // size >>= 6
                    size.Load();
                    il.Math.Shr(6);
                    il.Convert.ToUInt8();
                    size.Store();

                    var done = il.NewLabel();
                    done.Branch(@short: true);

                    secondSizeByte.Mark();

                    // read second size byte
                    ReadByte(propAddress);
                    size.Store();

                    // size &= 0x3f
                    size.Load();
                    il.Math.And(0x3f);
                    il.Convert.ToUInt8();
                    size.Store();

                    // if (size == 0)
                    size.Load();
                    done.BranchIf(Condition.True, @short: true);

                    // size = 64
                    il.Load(64);
                    size.Store();

                    done.Mark();
                }

                // (ushort)(propAddress + size + 1)
                propAddress.Load();
                size.Load();
                il.Math.Add();
                il.Math.Add(1);
                il.Convert.ToUInt16();
            }
        }

        private void ReadObjectLeftSibling(ILocal objNum)
        {
            var done = il.NewLabel();

            using (var parentNum = il.NewLocal<ushort>())
            using (var parentChildNum = il.NewLocal<ushort>())
            using (var next = il.NewLocal<ushort>())
            using (var siblingNum = il.NewLocal<ushort>())
            {
                ReadObjectParent(objNum);
                parentNum.Store();

                var hasParent = il.NewLabel();

                // parentNum != 0?
                parentNum.Load();
                hasParent.BranchIf(Condition.True, @short: true);

                il.Load(0);
                done.Branch();

                hasParent.Mark();

                ReadObjectChild(parentNum);
                parentChildNum.Store();

                var isNotFirstChild = il.NewLabel();
                parentChildNum.Load();
                objNum.Load();
                isNotFirstChild.BranchIf(Condition.NotEqual, @short: true);

                il.Load(0);
                done.Branch();

                isNotFirstChild.Mark();

                parentChildNum.Load();
                next.Store();

                var loopStart = il.NewLabel();
                loopStart.Mark();

                ReadObjectSibling(next);
                siblingNum.Store();

                var continueLoop = il.NewLabel();
                siblingNum.Load();
                objNum.Load();
                continueLoop.BranchIf(Condition.NotEqual, @short: true);

                next.Load();
                done.Branch();

                continueLoop.Mark();

                siblingNum.Load();
                next.Store();

                next.Load();
                loopStart.BranchIf(Condition.True);

                next.Load();

                done.Mark();
            }
        }

        private void RemoveObjectFromParent(ILocal objNum)
        {
            using (var leftSiblingNum = il.NewLocal<ushort>())
            using (var rightSiblingNum = il.NewLocal<ushort>())
            using (var parentNum = il.NewLocal<ushort>())
            {
                ReadObjectLeftSibling(objNum);
                leftSiblingNum.Store();

                ReadObjectSibling(objNum);
                rightSiblingNum.Store();

                // leftSiblingNum != 0?
                var secondStep = il.NewLabel();
                leftSiblingNum.Load();
                secondStep.BranchIf(Condition.False, @short: true);

                // first, unhook left sibling from right sibling
                WriteObjectSibling(leftSiblingNum, rightSiblingNum);

                secondStep.Mark();

                // next, unhook obj from parent, if it is the first child
                ReadObjectParent(objNum);
                parentNum.Store();

                // parentNum != 0?
                var thirdStep = il.NewLabel();
                parentNum.Load();
                thirdStep.BranchIf(Condition.False, @short: true);

                // objNum == parentNum.Child
                objNum.Load();
                ReadObjectChild(parentNum);
                thirdStep.BranchIf(Condition.NotEqual, @short: true);

                WriteObjectChild(parentNum, rightSiblingNum);

                thirdStep.Mark();

                // finally, set parent and sibling to 0
                WriteObjectParent(objNum, 0);
                WriteObjectSibling(objNum, 0);
            }
        }

        private void MoveObjectToDestination(ILocal objNum, ILocal destNum)
        {
            RemoveObjectFromParent(objNum);

            var done = il.NewLabel();

            destNum.Load();
            done.BranchIf(Condition.False);

            WriteObjectParent(objNum, destNum);

            using (var parentChildNum = il.NewLocal<ushort>())
            {
                ReadObjectChild(destNum);
                parentChildNum.Store();

                WriteObjectSibling(objNum, parentChildNum);
                WriteObjectChild(destNum, objNum);
            }

            done.Mark();
        }
    }
}
