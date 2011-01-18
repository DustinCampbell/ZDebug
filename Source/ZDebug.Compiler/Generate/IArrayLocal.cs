using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZDebug.Compiler.Generate
{
    public interface IArrayLocal : ILocal
    {
        void LoadLength();

        void LoadElement(CodeBuilder loadIndex);
        void StoreElement(CodeBuilder loadIndex, CodeBuilder loadValue);
    }
}
