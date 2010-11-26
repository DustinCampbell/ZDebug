using System.IO;
using System.Reflection;

namespace ZDebug.Core.Tests.Utilities
{
    internal static class ZCode
    {
        private const string CZech = "czech.z5";
        private const string Curses = "curses.z5";
        private const string Dreamhold = "dreamhold.z8";
        private const string Jigsaw = "jigsaw.z8";
        private const string Zork1 = "zork1.z3";

        private static Stream LoadZCodeStream(string name)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream("ZDebug.Core.Tests.ZCode." + name);
        }

        public static Stream LoadCurses()
        {
            return LoadZCodeStream(Curses);
        }

        public static Stream LoadCZech()
        {
            return LoadZCodeStream(CZech);
        }

        public static Stream LoadDreamhold()
        {
            return LoadZCodeStream(Dreamhold);
        }

        public static Stream LoadJigsaw()
        {
            return LoadZCodeStream(Jigsaw);
        }

        public static Stream LoadZork1()
        {
            return LoadZCodeStream(Zork1);
        }
    }
}
