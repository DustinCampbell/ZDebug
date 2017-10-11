using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZDebug.Compiler.Generate
{
    public interface ILabel
    {
        void Mark();

        void Branch(bool @short = false);
        void BranchIf(Condition condition, bool @short = false);
    }
}
