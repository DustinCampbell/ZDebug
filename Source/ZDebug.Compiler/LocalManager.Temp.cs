using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;

namespace ZDebug.Compiler
{
    public partial class LocalManager
    {
        public class Temp<T> : IDisposable
        {
            private readonly LocalManager localManager;
            private readonly LocalBuilder local;

            internal Temp(LocalManager localManager)
            {
                this.localManager = localManager;
                this.local = localManager.Allocate<T>();
            }

            public void Dispose()
            {
                this.localManager.Release(this.local);
            }

            public LocalBuilder Local
            {
                get { return local; }
            }

            public static implicit operator LocalBuilder(Temp<T> temp)
            {
                return temp.Local;
            }
        }
    }
}
