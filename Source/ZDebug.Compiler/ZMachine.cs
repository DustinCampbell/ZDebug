using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZDebug.Core;
using ZDebug.Core.Basics;

namespace ZDebug.Compiler
{
    public sealed class ZMachine
    {
        private readonly byte[] memory;

        public ZMachine(byte[] memory)
        {
            this.memory = memory;
        }

        private ushort Init()
        {
            var stack = new ushort[1024];
            var sp = 0;

            return stack[sp++];
        }

        private string StringFormat1(string format, ushort arg0)
        {
            return string.Format(format, arg0);
        }

        private string StringFormat2(string format, ushort arg0, ushort arg1)
        {
            return string.Format(format, arg0, arg1);
        }

        private string StringFormat3(string format, ushort arg0, ushort arg1, ushort arg2)
        {
            return string.Format(format, arg0, arg1, arg2);
        }

        private string StringFormat4(string format, ushort arg0, ushort arg1, ushort arg2, ushort arg3)
        {
            return string.Format(format, arg0, arg1, arg2, arg3);
        }

        private string StringFormatAny(string format, params ushort[] args)
        {
            return string.Format(format, args);
        }
    }
}
