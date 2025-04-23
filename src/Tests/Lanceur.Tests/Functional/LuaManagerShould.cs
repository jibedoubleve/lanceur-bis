using FluentAssertions;
using FluentAssertions.Execution;
using Lanceur.Core.LuaScripting;
using Lanceur.Core.Services;
using Lanceur.Infra.LuaScripting;
using NSubstitute;
using Xunit;

namespace Lanceur.Tests.Functional;

public class LuaManagerShould
{
    #region Properties

    private static ILuaManager LuaManager => new LuaManager(Substitute.For<IUserGlobalNotificationService>());

    #endregion

    #region Methods

    [Fact]
    public void GetDefaultValueWhenReturnNullContext()
    {
        // arrange
        const string url = "https://random.url.com";
        const string luaScript = """
                                 if context.Parameters == "dev" or context.Parameters == "int" then
                                     context.FileName = "output_dev"
                                     return context
                                 end
                                 if context.Parameters == "test" then
                                     context.FileName = "output_test"
                                     return context
                                 end
                                 if context.Parameters == "prod" then
                                     context.FileName = "output_prod"
                                     return context
                                 end
                                 """;

        var result = LuaManager.ExecuteScript(new() { Code = luaScript, Context = new() { FileName   = url, Parameters = "unhandled_case" } });
        using var _ = new AssertionScope();
        result.Should().NotBeNull();

        result.Context.FileName.Should().Be(url);
        result.Context.Parameters.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void NotCrashWhenScriptIsNull()
    {
        var result = LuaManager.ExecuteScript(new() { Code = null, Context = new() { FileName = null, Parameters = null } });
        using var _ = new AssertionScope();
        result.Should().NotBeNull();
        result.Context.FileName.Should().NotBeNull();
        result.Context.Parameters.Should().NotBeNull();
    }

    [Fact]
    public void ReturnAnErrorWhenScriptDoesNotCompile()
    {
        // arrange
        const string url = "https://random.url.com";
        const string luaScript = "this is a failing script";

        var result = LuaManager.ExecuteScript(new() { Code = luaScript, Context = new() { FileName   = url, Parameters = "unhandled_case" } });
        using var _ = new AssertionScope();
        result.Should().NotBeNull();
        result.Exception.Should().NotBeNull();

        result.Context.FileName.Should().BeEmpty();
        result.Context.Parameters.Should().BeEmpty();
    }

    [Fact]
    public void ReturnEmptyContextWhenScriptDoNotReturnContext()
    {
        // arrange
        const string url = "https://random.url.com";
        const string parameters = "unhandled_case";
        const string luaScript = "return 145";

        var result = LuaManager.ExecuteScript(new() { Code = luaScript, Context = new() { FileName   = url, Parameters = parameters } });
        using var _ = new AssertionScope();
        result.Should().NotBeNull();
        result.Context.Should().NotBeNull();
        result.Context.FileName.Should().Be(url);
        result.Context.Parameters.Should().Be(parameters);
    }

    #endregion
}