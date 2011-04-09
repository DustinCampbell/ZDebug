﻿using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class AndGenerator : BinaryOpGenerator
    {
        public AndGenerator(Operand op1, Operand op2, Variable store)
            : base(OpcodeGeneratorKind.And, op1, op2, store, signed: false)
        {
        }

        protected override void Operation(ILBuilder il)
        {
            il.Math.And();
        }
    }
}
