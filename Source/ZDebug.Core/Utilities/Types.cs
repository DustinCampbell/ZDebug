using System;

namespace ZDebug.Core.Utilities
{
    public static class Types
    {
        public readonly static Type[] None = new Type[0];

        public static Type[] One<T>()
        {
            return new Type[] { typeof(T) };
        }

        public static Type[] Two<T1, T2>()
        {
            return new Type[] { typeof(T1), typeof(T2) };
        }

        public static Type[] Three<T1, T2, T3>()
        {
            return new Type[] { typeof(T1), typeof(T2), typeof(T3) };
        }

        public static Type[] Four<T1, T2, T3, T4>()
        {
            return new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) };
        }

        public static Type[] Five<T1, T2, T3, T4, T5>()
        {
            return new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) };
        }

        public static Type[] Six<T1, T2, T3, T4, T5, T6>()
        {
            return new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) };
        }

        public static Type[] Seven<T1, T2, T3, T4, T5, T6, T7>()
        {
            return new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) };
        }

        public static Type[] Eight<T1, T2, T3, T4, T5, T6, T7, T8>()
        {
            return new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) };
        }
    }
}
