using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Scripting;
using Lanceur.Core.Services;
using Lanceur.Infra.Scripting;
using Lanceur.Tests.Tools.Extensions;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Scripting;

public class CSharpScriptingManagerShould
{
    #region Fields

    private readonly ITestOutputHelper _outputHelper;

    #endregion

    #region Constructors

    public CSharpScriptingManagerShould(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    #endregion

    #region Properties

    private static IScriptEngine ScriptEngine
    {
        get
        {
            var settings = Substitute.For<ISection<ScriptingSection>>();
            settings.Value.Returns(new ScriptingSection());

            return new CSharpScriptEngine(
                Substitute.For<IUserGlobalNotificationService>(),
                settings
            );
        }
    }

    #endregion

    #region Methods

    [Theory]
    [InlineData("", "")]
    [InlineData("dev", "output_dev")]
    [InlineData("test", "output_test")]
    [InlineData("prod", "output_prod")]
    public async Task GetExpectedFileNameAfterScriptExecution(string parameter, string expectedFilename)
    {
        // arrange
        const string luaScript = """
                                 if(Context.Parameters == "dev")
                                 {
                                     Context.FileName = "output_dev";
                                     return;
                                 }
                                 if(Context.Parameters == "test")
                                 {
                                     Context.FileName = "output_test";
                                     return;
                                 }
                                 if(Context.Parameters == "prod")
                                 {
                                     Context.FileName = "output_prod";
                                     return;
                                 }
                                 """;

        var result = await ScriptEngine.ExecuteScriptAsync(
            new() { Code = luaScript, Context = new() { FileName = "", Parameters = parameter } }
        );

        HandleError(result);

        result.ShouldSatisfyAllConditions(
            r => r.ShouldNotBeNull(),
            r => r.Context.FileName.ShouldBe(expectedFilename)
        );
    }

    private void HandleError(ScriptResult result)
    {
        _outputHelper.WriteLine(result.IsOnError() 
            ? result.Exception.ToString() 
            : "No error when executing script");
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("dev", "output_dev")]
    [InlineData("test", "output_test")]
    [InlineData("prod", "output_prod")]
    public async Task GetExpectedParametersAfterScriptExecution(string parameter, string expectedParameter)
    {
        // arrange
        const string url = "https://random.url.com";
        const string luaScript = """
                                 if(Context.Parameters == "dev")
                                 {
                                     Context.Parameters = "output_dev";
                                     return;
                                 }
                                 if(Context.Parameters == "test")
                                 {
                                     Context.Parameters = "output_test";
                                     return;
                                 }
                                 if(Context.Parameters == "prod")
                                 {
                                     Context.Parameters = "output_prod";
                                     return;
                                 }
                                 """;

        var result = await ScriptEngine.ExecuteScriptAsync(
            new() { Code = luaScript, Context = new() { FileName = url, Parameters = parameter } }
        );
        
        HandleError(result);
        
        result.ShouldSatisfyAllConditions(
            r => r.ShouldNotBeNull(),
            r => r.Context.FileName.ShouldBe(url),
            r => r.Context.Parameters.ShouldBe(expectedParameter)
        );
    }

    [Fact]
    public async Task NotCrashWhenScriptIsNull()
    {
        var result = await ScriptEngine.ExecuteScriptAsync(
            new() { Code = null, Context = new() { FileName = null, Parameters = null } }
        );
        result.ShouldSatisfyAllConditions(
            r => r.ShouldNotBeNull(),
            r => r.Context.FileName.ShouldNotBeNull(),
            r => r.Context.Parameters.ShouldNotBeNull()
        );
    }

    [Fact]
    public async Task ReturnAnErrorWhenScriptDoesNotCompile()
    {
        // arrange
        const string url = "https://random.url.com";
        const string luaScript = "this is a failing script";

        var result = await ScriptEngine.ExecuteScriptAsync(
            new() { Code = luaScript, Context = new() { FileName   = url, Parameters = "unhandled_case" } }
        );
        result.ShouldSatisfyAllConditions(
            r => r.ShouldNotBeNull(),
            r => r.Exception.ShouldNotBeNull(),
            r => r.Context.FileName.ShouldBeEmpty(),
            r => r.Context.Parameters.ShouldBeEmpty()
        );
    }

    [Fact]
    public async Task ReturnEmptyContextWhenScriptDoNotReturnContext()
    {
        // arrange
        const string url = "https://random.url.com";
        const string parameters = "unhandled_case";
        const string luaScript = "return 145;";

        var result = await ScriptEngine.ExecuteScriptAsync(
            new() { Code = luaScript, Context = new() { FileName = url, Parameters = parameters } }
        );
        
        HandleError(result);
        
        result.ShouldSatisfyAllConditions(
            r => r.ShouldNotBeNull(),
            r => r.Context.ShouldNotBeNull(),
            r => r.Context.FileName.ShouldBe(url),
            r => r.Context.Parameters.ShouldBe(parameters)
        );
    }

    #endregion
}