using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace ZDebug.Compiler.Tests.Utilities
{
    internal static class ZCode
    {
        private const string Zork1 = "zork1.z3";

        private static Stream LoadZCodeStream(string name)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream("ZDebug.Compiler.Tests.ZCode." + name);
        }

        public static Stream LoadZork1()
        {
            return LoadZCodeStream(Zork1);
        }

        public static byte[] ReadZork1()
        {
            using (var stream = ZCode.LoadZork1())
            {
                return stream.ReadFully();
            }
        }
    }
}
