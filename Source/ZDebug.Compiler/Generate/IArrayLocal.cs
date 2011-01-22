using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZDebug.Compiler.Generate
{
    public interface IArrayLocal : ILocal
    {
        void Create(int length);
        void Create(ILocal length);

        void LoadLength();

        void LoadElement(CodeBuilder loadIndex);
        void StoreElement(CodeBuilder loadIndex, CodeBuilder loadValue);
    }
}
