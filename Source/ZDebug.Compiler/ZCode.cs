using System;
using System.Collections.Generic;
using ZDebug.Core.Utilities;

namespace ZDebug.Compiler
{
    internal delegate ushort ZCodeDelegate0(int argCount);
    internal delegate ushort ZCodeDelegate1(int argCount, ushort loc1);
    internal delegate ushort ZCodeDelegate2(int argCount, ushort loc1, ushort loc2);
    internal delegate ushort ZCodeDelegate3(int argCount, ushort loc1, ushort loc2, ushort loc3);
    internal delegate ushort ZCodeDelegate4(int argCount, ushort loc1, ushort loc2, ushort loc3, ushort loc4);
    internal delegate ushort ZCodeDelegate5(int argCount, ushort loc1, ushort loc2, ushort loc3, ushort loc4, ushort loc5);
    internal delegate ushort ZCodeDelegate6(int argCount, ushort loc1, ushort loc2, ushort loc3, ushort loc4, ushort loc5, ushort loc6);
    internal delegate ushort ZCodeDelegate7(int argCount, ushort loc1, ushort loc2, ushort loc3, ushort loc4, ushort loc5, ushort loc6, ushort loc7);
    internal delegate ushort ZCodeDelegate8(int argCount, ushort loc1, ushort loc2, ushort loc3, ushort loc4, ushort loc5, ushort loc6, ushort loc7, ushort loc8);
    internal delegate ushort ZCodeDelegate9(int argCount, ushort loc1, ushort loc2, ushort loc3, ushort loc4, ushort loc5, ushort loc6, ushort loc7, ushort loc8, ushort loc9);
    internal delegate ushort ZCodeDelegate10(int argCount, ushort loc1, ushort loc2, ushort loc3, ushort loc4, ushort loc5, ushort loc6, ushort loc7, ushort loc8, ushort loc9, ushort loc10);
    internal delegate ushort ZCodeDelegate11(int argCount, ushort loc1, ushort loc2, ushort loc3, ushort loc4, ushort loc5, ushort loc6, ushort loc7, ushort loc8, ushort loc9, ushort loc10, ushort loc11);
    internal delegate ushort ZCodeDelegate12(int argCount, ushort loc1, ushort loc2, ushort loc3, ushort loc4, ushort loc5, ushort loc6, ushort loc7, ushort loc8, ushort loc9, ushort loc10, ushort loc11, ushort loc12);
    internal delegate ushort ZCodeDelegate13(int argCount, ushort loc1, ushort loc2, ushort loc3, ushort loc4, ushort loc5, ushort loc6, ushort loc7, ushort loc8, ushort loc9, ushort loc10, ushort loc11, ushort loc12, ushort loc13);
    internal delegate ushort ZCodeDelegate14(int argCount, ushort loc1, ushort loc2, ushort loc3, ushort loc4, ushort loc5, ushort loc6, ushort loc7, ushort loc8, ushort loc9, ushort loc10, ushort loc11, ushort loc12, ushort loc13, ushort loc14);
    internal delegate ushort ZCodeDelegate15(int argCount, ushort loc1, ushort loc2, ushort loc3, ushort loc4, ushort loc5, ushort loc6, ushort loc7, ushort loc8, ushort loc9, ushort loc10, ushort loc11, ushort loc12, ushort loc13, ushort loc14, ushort loc15);

    internal static class ZCodeParameterTypes
    {
        private static readonly Dictionary<Type, Type[]> typeMap = new Dictionary<Type, Type[]>()
        {
            {typeof(ZCodeDelegate0), Types.Array<int>()},
            {typeof(ZCodeDelegate1), Types.Array<int, ushort>()},
            {typeof(ZCodeDelegate2), Types.Array<int, ushort, ushort>()},
            {typeof(ZCodeDelegate3), Types.Array<int, ushort, ushort, ushort>()},
            {typeof(ZCodeDelegate4), Types.Array<int, ushort, ushort, ushort, ushort>()},
            {typeof(ZCodeDelegate5), Types.Array<int, ushort, ushort, ushort, ushort, ushort>()},
            {typeof(ZCodeDelegate6), Types.Array<int, ushort, ushort, ushort, ushort, ushort, ushort>()},
            {typeof(ZCodeDelegate7), Types.Array<int, ushort, ushort, ushort, ushort, ushort, ushort, ushort>()},
            {typeof(ZCodeDelegate8), Types.Array<int, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort>()},
            {typeof(ZCodeDelegate9), Types.Array<int, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort>()},
            {typeof(ZCodeDelegate10), Types.Array<int, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort>()},
            {typeof(ZCodeDelegate11), Types.Array<int, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort>()},
            {typeof(ZCodeDelegate12), Types.Array<int, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort>()},
            {typeof(ZCodeDelegate13), Types.Array<int, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort>()},
            {typeof(ZCodeDelegate14), Types.Array<int, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort>()},
            {typeof(ZCodeDelegate15), Types.Array<int, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort, ushort>()}
        };

        public static Type[] ForDelegate<T>()
        {
            return typeMap[typeof(T)];
        }
    }

}
