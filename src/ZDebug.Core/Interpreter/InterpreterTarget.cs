using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZDebug.Core.Interpreter
{
    /// <summary>
    /// Specifies the target machine of the interpreter.
    /// </summary>
    public enum InterpreterTarget
    {
        DECSystem20 = 1,
        AppleIIe,
        Macintosh,
        Amiga,
        AtariST,
        IBMPC,
        Commodore128,
        Commodore64,
        AppleIIc,
        AppleIIgs,
        TandyColor
    }
}
