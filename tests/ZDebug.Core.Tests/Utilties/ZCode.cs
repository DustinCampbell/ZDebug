using System.IO;
using System.Reflection;

namespace ZDebug.Core.Tests.Utilities;

internal static class ZCode
{
    private const string CZech = "czech.z5";
    private const string Curses = "curses.z5";
    private const string Dreamhold = "dreamhold.z8";
    private const string Jigsaw = "jigsaw.z8";
    private const string Zork1 = "zork1.z3";

    private static Stream LoadZCodeStream(string name) => Assembly.GetExecutingAssembly().GetManifestResourceStream("ZDebug.Core.Tests.ZCode." + name);

    public static Stream LoadCurses() => LoadZCodeStream(Curses);

    public static Stream LoadCZech() => LoadZCodeStream(CZech);

    public static Stream LoadDreamhold() => LoadZCodeStream(Dreamhold);

    public static Stream LoadJigsaw() => LoadZCodeStream(Jigsaw);

    public static Stream LoadZork1() => LoadZCodeStream(Zork1);
}
