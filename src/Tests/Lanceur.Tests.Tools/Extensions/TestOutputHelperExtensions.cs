using Xunit.Abstractions;

namespace Lanceur.Tests.Tooling.Extensions;

public static class TestOutputHelperExtensions
{
    #region Methods

    public static void Title(this ITestOutputHelper outputHelper, string output)
    {
        outputHelper.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        outputHelper.WriteLine($"~~~ {output}");
        outputHelper.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
    }

    #endregion
}