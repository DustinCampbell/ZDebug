namespace ZDebug.Core.Instructions
{
    internal static class Strict
    {
        private static void Fail(Instruction instruction, string message)
        {
            throw new StrictException(instruction, message);
        }

        private static void Fail(Instruction instruction, string format, params object[] args)
        {
            Fail(instruction, string.Format(format, args));
        }

        public static void OperandCountIs(Instruction instruction, int expected)
        {
            if (instruction.OperandCount != expected)
            {
                Fail(instruction, "Expected {0} operands but found {1}.", expected, instruction.OperandCount);
            }
        }

        public static void OperandCountInRange(Instruction instruction, int low, int high)
        {
            var opCount = instruction.OperandCount;
            if (opCount < low || opCount > high)
            {
                Fail(instruction, "Expected {0}-{1} operands but found {2}.", low, high, opCount);
            }
        }

        public static void HasStoreVariable(Instruction instruction)
        {
            if (!instruction.HasStoreVariable)
            {
                Fail(instruction, "Does not have store variable.");
            }
        }

        public static void HasBranch(Instruction instruction)
        {
            if (!instruction.HasBranch)
            {
                Fail(instruction, "Does not have branch.");
            }
        }

        public static void HasZText(Instruction instruction)
        {
            if (!instruction.HasZText)
            {
                Fail(instruction, "Does not have ztext.");
            }
        }

        public static void IsByte(Instruction instruction, ushort value)
        {
            if (value >= 256)
            {
                Fail(instruction, "Expected byte value but was {0:x4}.", value);
            }
        }
    }
}
