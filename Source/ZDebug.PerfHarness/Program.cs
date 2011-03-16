namespace ZDebug.PerfHarness
{
    internal class Program
    {
        const string BRONZE = @"..\..\ZCode\bronze\bronze.z8";
        const string DREAMHOLD = @"..\..\ZCode\dreamhold\dreamhold.z8";
        const string SANDDANC = @"..\..\ZCode\sanddanc\sanddanc.z5";
        const string ROTA = @"..\..\ZCode\rota\RoTA.z8";
        const string ZORK1 = @"..\..\ZCode\zork1\zork1.z3";

        const string BRONZE_SCRIPT = @"..\..\ZCode\bronze\bronze_script.txt";
        const string ROTA_SCRIPT = @"..\..\ZCode\rota\rota_script.txt";
        const string ZORK1_SCRIPT = @"..\..\ZCode\zork1\zork1_script.txt";

        static void Main()
        {
            var runner = new CompiledRunner(BRONZE, BRONZE_SCRIPT);
            runner.Run();
        }
    }
}
