using System;
using System.Reflection.Emit;
using System.Collections.Generic;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler
{
    public static class ZCompiler
    {
        private static string GetName(ZRoutine routine)
        {
            return "ZRoutine_" + routine.Address.ToString("x4");
        }

        public static Action Compile(ZRoutine routine, ZMachine machine)
        {
            var dm = new DynamicMethod(GetName(routine), typeof(void), new Type[] { typeof(ZMachine) });
            var il = dm.GetILGenerator();

            // first pass, create labels for branches and jumps
            var addressToLabelMap = new Dictionary<int, Label>();
            foreach (var i in routine.Instructions)
            {
                if (i.HasBranch && i.Branch.Kind == BranchKind.Address)
                {
                    var address = i.Address + i.Length + i.Branch.Offset - 2;
                    addressToLabelMap.Add(address, il.DefineLabel());
                }
                else if (i.Opcode.IsJump)
                {
                    var address = i.Address + i.Length + (short)i.Operands[0].Value - 2;
                    addressToLabelMap.Add(address, il.DefineLabel());
                }
            }

            return (Action)dm.CreateDelegate(typeof(Action), machine);
        }
    }
}
