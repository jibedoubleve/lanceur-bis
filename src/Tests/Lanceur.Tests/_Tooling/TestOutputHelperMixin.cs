using Newtonsoft.Json;
using Xunit.Abstractions;

namespace Lanceur.Tests.Tooling;

public static class TestOutputHelperMixin
{
    public static void WriteJson(this ITestOutputHelper output,string message, object toDump)
    {
        var json = JsonConvert.SerializeObject(toDump, Formatting.Indented);
        output.WriteLine($"Dump object '{toDump.GetType()}'\n{message}\n{json}");
    }
}