using FluentAssertions;
using FluentAssertions.Execution;
using Lanceur.Core.LuaScripting;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Requests;
using Lanceur.Core.Services;
using Lanceur.Infra.LuaScripting;
using Lanceur.Infra.Macros;
using Lanceur.Infra.Services;
using Lanceur.Infra.Wildcards;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Lanceur.Tests.Services;

public class ExecutionServiceShould
{
    #region Methods

    [Theory]
    [InlineData("$C$", "hello world", "hello+world")]
    [InlineData("$R$", "un deux / \\ - <", "un deux / \\ - <")]
    [InlineData("$I$", "un deux", "un deux")]
    [InlineData("$W$", "hello world", "hello+world")]
    public async Task ExecuteAliasWithCorrectFileNameReplacements(string actual, string parameters, string expected)
    {
        // arrange
        var outputFileName = string.Empty;

        var processLauncher = new ProcessLauncherVisitor(context => outputFileName = context.FileName);
        var originatingQuery = $"alias {parameters}";
        var cmdline = Cmdline.Parse(originatingQuery);

        var clipboard = Substitute.For<IClipboardService>();
        clipboard.RetrieveText().Returns(parameters);

        var executionManager = new ExecutionService(
            Substitute.For<ILoggerFactory>(),
            new ReplacementComposite(
                clipboard,
                Substitute.For<ILogger<ReplacementComposite>>()
            ),
            Substitute.For<IAliasRepository>(),
            Substitute.For<ILuaManager>(),
            processLauncher
        );

        var request = new ExecutionRequest
        {
            OriginatingQuery = originatingQuery,
            ExecuteWithPrivilege = false,
            QueryResult = new AliasQueryResult
            {
                FileName = actual, 
                OriginatingQuery = cmdline
            }
        };

        // act
        await executionManager.ExecuteAsync(request);

        // assert
        using (new AssertionScope()) { outputFileName.Should().Be(expected); }
    }

    [Theory]
    [InlineData("$C$", "hello world", "hello+world")]
    [InlineData("$R$", "un deux / \\ - <", "un deux / \\ - <")]
    [InlineData("$I$", "un deux", "un deux")]
    [InlineData("$W$", "hello world", "hello+world")]
    public async Task ExecuteAliasWithCorrectParametersReplacements(
        string parameters,
        string queryParameters,
        string expectedParameters
    )
    {
        // arrange
        var outputParameters = string.Empty;

        var processLauncher = new ProcessLauncherVisitor(context => outputParameters = context.Arguments);
        var originatingQuery = $"alias {queryParameters}";
        var cmdline = Cmdline.Parse(originatingQuery);

        var clipboard = Substitute.For<IClipboardService>();
        clipboard.RetrieveText().Returns(queryParameters);

        var executionManager = new ExecutionService(
            Substitute.For<ILoggerFactory>(),
            new ReplacementComposite(
                clipboard,
                Substitute.For<ILogger<ReplacementComposite>>()
            ),
            Substitute.For<IAliasRepository>(),
            Substitute.For<ILuaManager>(),
            processLauncher
        );

        var request = new ExecutionRequest
        {
            OriginatingQuery = originatingQuery,
            ExecuteWithPrivilege = false,
            QueryResult = new AliasQueryResult
            {
                FileName = "alias",
                Parameters = parameters,
                OriginatingQuery = cmdline
            }
        };

        // act
        await executionManager.ExecuteAsync(request);

        // assert
        using (new AssertionScope()) { outputParameters.Should().Be(expectedParameters); }
    }

    [Theory]
    [InlineData("application.exe", "-c aa -b bb", "app")]
    [InlineData("application.exe", "-c aa -b bb", "app undeux trois")]
    public async Task ExecuteAliasWithCorrectParametersWithoutReplacements(string fileName, string parameters, string originatingQuery)
    {
        // arrange
        var outputFileName = string.Empty;
        var outputParameters = string.Empty;

        var processLauncher = new ProcessLauncherVisitor(context =>
            {
                outputFileName = context.FileName;
                outputParameters = context.Arguments;
            }
        );
        var cmdline = Cmdline.Parse(originatingQuery);
        var executionManager = new ExecutionService(
            Substitute.For<ILoggerFactory>(),
            new ReplacementComposite(
                Substitute.For<IClipboardService>(),
                Substitute.For<ILogger<ReplacementComposite>>()
            ),
            Substitute.For<IAliasRepository>(),
            Substitute.For<ILuaManager>(),
            processLauncher
        );

        var request = new ExecutionRequest
        {
            OriginatingQuery = originatingQuery,
            ExecuteWithPrivilege = false,
            QueryResult = new AliasQueryResult
            {
                FileName = fileName, 
                Parameters = parameters, 
                OriginatingQuery = cmdline
            }
        };

        // act
        await executionManager.ExecuteAsync(request);

        // assert
        using (new AssertionScope())
        {
            outputFileName.Should().Be(fileName);
            outputParameters.Should().Be(parameters);
        }
    }

    [Theory]
    [InlineData("ini", "thb@joplin@spotify")]
    public async Task ExecuteMultiMacro(string cmd, string parameters)
    {
        var cmdline = new Cmdline(cmd, parameters);
        var executionManager = new ExecutionService(
            Substitute.For<ILoggerFactory>(),
            Substitute.For<IWildcardService>(),
            Substitute.For<IAliasRepository>(),
            Substitute.For<ILuaManager>(),
            Substitute.For<IProcessLauncher>()
        );

        var macro = new MultiMacro(Substitute.For<IExecutionService>(), Substitute.For<ISearchService>(), 0);
        var request = new ExecutionRequest
        {
            OriginatingQuery = cmdline, ExecuteWithPrivilege = false, QueryResult = macro
        };

        try { await executionManager.ExecuteAsync(request); }
        catch (Exception) { Assert.Fail("This should not throw an exception"); }
    }

    [Theory]
    [InlineData("ini", "thb@joplin@spotify")]
    public async Task SelfExecuteMacroWithoutCrash(string cmd, string parameters)
    {
        var cmdline = new Cmdline(cmd, parameters);
        var macro = new MultiMacro(Substitute.For<IExecutionService>(), Substitute.For<ISearchService>(), 0);

        try { await macro.ExecuteAsync(cmdline); }
        catch (Exception) { Assert.Fail("This should not throw an exception"); }
    }

    [Fact]
    public async Task HandleLuaScriptWithParameters()
    {
        // arrange
        const string queryParameters = "hello world";
        const string originatingQuery = $"alias {queryParameters}";
        
        var cmdline = Cmdline.Parse(originatingQuery);
        var processLauncher = Substitute.For<IProcessLauncher>();

        var executionManager = new ExecutionService(
            Substitute.For<ILoggerFactory>(),
            new ReplacementComposite(
                Substitute.For<IClipboardService>(),
                Substitute.For<ILogger<ReplacementComposite>>()
            ),
            Substitute.For<IAliasRepository>(),
            new LuaManager(Substitute.For<IUserGlobalNotificationService>()),
            processLauncher
        );
        
        
        var request = new ExecutionRequest
        {
            OriginatingQuery = originatingQuery,
            ExecuteWithPrivilege = false,
            QueryResult = new AliasQueryResult
            {
                FileName = "alias",
                LuaScript = """
                            if context.Parameters == nil then
                                context.Parameters = "wrongParameters"
                            end
                            return context
                            """,
                OriginatingQuery = cmdline
            }
        };
        
        // act
        await executionManager.ExecuteAsync(request);
        
        // assert
        processLauncher.Received().Start(
            Arg.Is<ProcessContext>(psi => psi.Arguments == queryParameters)
        );
    }

    #endregion
}