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
            if (instruction.Operands.Count != expected)
            {
                Fail(instruction, "Expected {0} operands but found {1}.", expected, instruction.Operands.Count);
            }
        }

        public static void OperandCountInRange(Instruction instruction, int low, int high)
        {
            if (instruction.Operands.Count < low || instruction.Operands.Count > high)
            {
                Fail(instruction, "Expected {0}-{1} operands but found {2}.", low, high, instruction.Operands.Count);
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
    }
}
