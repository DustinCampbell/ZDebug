namespace ZDebug.PerfHarness;

internal class Program
{
    private const string BRONZE = @"..\..\ZCode\bronze\bronze.z8";
    private const string DREAMHOLD = @"..\..\ZCode\dreamhold\dreamhold.z8";
    private const string SANDDANC = @"..\..\ZCode\sanddanc\sanddanc.z5";
    private const string ROTA = @"..\..\ZCode\rota\RoTA.z8";
    private const string ZORK1 = @"..\..\ZCode\zork1\zork1.z3";
    private const string HITCHHIK = @"..\..\ZCode\hitchhik\hitchhik.z5";

    private const string BRONZE_SCRIPT = @"..\..\ZCode\bronze\bronze_script.txt";
    private const string ROTA_SCRIPT = @"..\..\ZCode\rota\rota_script.txt";
    private const string ZORK1_SCRIPT = @"..\..\ZCode\zork1\zork1_script.txt";
    private const string HITCHHIK_SCRIPT = @"..\..\ZCode\hitchhik\hitchhik_script.txt";

    private static void Main()
    {
        var runner = new CompiledRunner(ROTA, ROTA_SCRIPT);
        runner.Run();
    }
}
