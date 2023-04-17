using System.Runtime.CompilerServices;
using Xunit.Abstractions;

namespace Lanceur.Tests.Logging
{
    public static class TestOutputHelperMixin
    {
        #region Fields

        private static string TAB = "  ";

        #endregion Fields

        #region Methods

        private static void Write(this ITestOutputHelper output, string message, [CallerMemberName] string method = null) => output.WriteLine($"[{method, -6}] {message}");

        public static void Act(this ITestOutputHelper output) => output.Write("---- ACT", nameof(Info));

        public static void Arrange(this ITestOutputHelper output) => output.Write("---- ARRANGE", nameof(Info));

        public static void Assert(this ITestOutputHelper output) => output.Write("---- ASSERT", nameof(Info));

        public static void Debug(this ITestOutputHelper output, string message) => output.Write($"{TAB}{message}");

        public static void Error(this ITestOutputHelper output, string message) => output.Write($"{TAB}{message}");

        public static void Info(this ITestOutputHelper output, string message) => output.Write($"{TAB}{message}");

        public static void Warn(this ITestOutputHelper output, string message) => output.Write($"{TAB}{message}");

        #endregion Methods
    }
}