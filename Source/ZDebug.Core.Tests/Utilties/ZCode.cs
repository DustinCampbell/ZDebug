using System.IO;
using System.Reflection;

namespace ZDebug.Core.Tests.Utilities
{
    internal static class ZCode
    {
        private const string CZech = "czech.z5";

        private static Stream LoadZCodeStream(string name)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream("ZDebug.Core.Tests.ZCode." + name);
        }

        public static Stream LoadCZech()
        {
            return LoadZCodeStream(CZech);
        }
    }
}
