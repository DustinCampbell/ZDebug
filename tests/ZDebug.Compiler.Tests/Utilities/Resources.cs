using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ZDebug.Compiler.Tests.Utilities
{
    internal static class Resources
    {
        public const string Zork1 = "zork1.z3";
        public const string Zork1Script = "zork1_script.txt";
        public const string Zork1Transcript = "zork1_transcript.txt";

        public const string Rota = "rota.z8";
        public const string RotaScript = "rota_script.txt";
        public const string RotaTranscript = "rota_transcript.txt";

        public const string CZech = "czech.z5";
        public const string CZechTranscript = "czech_transcript.txt";

        public static Stream LoadStream(string name)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream("ZDebug.Compiler.Tests.Resources." + name);
        }

        public static string[] LoadLines(string name)
        {
            var list = new List<string>();

            using (var reader = new StreamReader(LoadStream(name)))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    list.Add(line);
                }
            }

            return list.ToArray();
        }

        public static string LoadText(string name)
        {
            using (var reader = new StreamReader(LoadStream(name)))
            {
                return reader.ReadToEnd().Replace("\r\n", "\n");
            }
        }
    }
}
