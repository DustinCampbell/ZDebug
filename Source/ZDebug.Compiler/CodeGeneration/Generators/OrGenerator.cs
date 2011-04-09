﻿using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class OrGenerator : BinaryOpGenerator
    {
        public OrGenerator(Operand op1, Operand op2, Variable store)
            : base(OpcodeGeneratorKind.Or, op1, op2, store, signed: false)
        {
        }

        protected override void Operation(ILBuilder il)
        {
            il.Math.Or();
        }
    }
}
