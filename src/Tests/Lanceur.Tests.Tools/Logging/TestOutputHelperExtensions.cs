﻿using System.Runtime.CompilerServices;
using Xunit.Abstractions;

namespace Lanceur.Tests.Tools.Logging;

public static class TestOutputHelperExtensions
{
    #region Fields

    private const string Tab = "  ";

    #endregion Fields

    #region Methods

    private static void Write(this ITestOutputHelper output, string message, [CallerMemberName] string? method = null) => output.WriteLine($"[{method,-6}] {message}");

    public static void Act(this ITestOutputHelper output) => output.Write("---- ACT", nameof(Info));

    public static void Arrange(this ITestOutputHelper output) => output.Write("---- ARRANGE", nameof(Info));

    public static void Assert(this ITestOutputHelper output) => output.Write("---- ASSERT", nameof(Info));

    public static void Debug(this ITestOutputHelper output, string message) => output.Write($"{Tab}{message}");

    public static void Error(this ITestOutputHelper output, string message) => output.Write($"{Tab}{message}");

    public static void Info(this ITestOutputHelper output, string message) => output.Write($"{Tab}{message}");

    public static void Warn(this ITestOutputHelper output, string message) => output.Write($"{Tab}{message}");

    #endregion Methods
}