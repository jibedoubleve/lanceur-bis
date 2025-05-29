using FluentAssertions;
using FluentAssertions.Execution;
using Lanceur.Core.Models;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Requests;
using Lanceur.Core.Services;
using Lanceur.Infra.LuaScripting;
using Lanceur.Infra.Macros;
using Lanceur.Infra.Repositories;
using Lanceur.Infra.Services;
using Lanceur.Infra.Wildcards;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.Services;

public class ExecutionServiceShould : TestBase
{
    #region Constructors

    public ExecutionServiceShould(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    private ExecutionService CreateExecutionService(
        IProcessLauncher processLauncher = null,
        IClipboardService clipboardService = null
    )
    {
        var executionService = new ExecutionService(
            LoggerFactory,
            new ReplacementComposite(
                clipboardService ?? Substitute.For<IClipboardService>(),
                LoggerFactory.CreateLogger<ReplacementComposite>()
            ),
            Substitute.For<IAliasRepository>(),
            new LuaManager(Substitute.For<IUserGlobalNotificationService>()),
            processLauncher ?? Substitute.For<IProcessLauncher>()
        );
        return executionService;
    }

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
        var executionService = CreateExecutionService(processLauncher, clipboard);

        var request = new ExecutionRequest
        {
            OriginatingQuery = originatingQuery,
            QueryResult = new AliasQueryResult
            {
                FileName = actual, OriginatingQuery = cmdline, Parameters = parameters
            }
        };

        // act
        await executionService.ExecuteAsync(request);

        // assert
        using (new AssertionScope()) { outputFileName.Should().Be(expected); }
    }

    [Theory]
    [InlineData("--$C$", "hello world", "--hello+world")]
    [InlineData("--$R$", "un deux / \\ - <", "--un deux / \\ - <")]
    [InlineData("--$I$", "un deux", "--un deux")]
    [InlineData("--$W$", "hello world", "--hello+world")]
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

        var executionService = CreateExecutionService(processLauncher, clipboard);

        var request = new ExecutionRequest
        {
            OriginatingQuery = originatingQuery,
            QueryResult = new AliasQueryResult
            {
                FileName = "alias", Parameters = parameters, OriginatingQuery = cmdline
            }
        };

        // act
        await executionService.ExecuteAsync(request);

        // assert
        using (new AssertionScope()) { outputParameters.Should().Be(expectedParameters); }
    }

    [Theory]
    [InlineData("application.exe", "-c aa -b bb", "app")]
    [InlineData("application.exe", "-c aa -b bb", "app undeux trois")]
    public async Task ExecuteAliasWithCorrectParametersWithoutReplacements(
        string fileName,
        string parameters,
        string originatingQuery
    )
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
        var executionService = CreateExecutionService(processLauncher);

        var request = new ExecutionRequest
        {
            OriginatingQuery = originatingQuery,
            ExecuteWithPrivilege = false,
            QueryResult = new AliasQueryResult
            {
                FileName = fileName, Parameters = parameters, OriginatingQuery = cmdline
            }
        };

        // act
        await executionService.ExecuteAsync(request);

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
        var executionService = CreateExecutionService();

        var macro = new MultiMacro(Substitute.For<IExecutionService>(), Substitute.For<ISearchService>(), 0);
        var request = new ExecutionRequest
        {
            OriginatingQuery = cmdline, ExecuteWithPrivilege = false, QueryResult = macro
        };

        try { await executionService.ExecuteAsync(request); }
        catch (Exception) { Assert.Fail("This should not throw an exception"); }
    }
    
    [Theory]
    [InlineData("issue", "with some text as paramters")]
    public async Task ExecuteMacroWithParameters(string cmd, string parameters)
    {
        var cmdline = new Cmdline(cmd, parameters);
        var executionService = CreateExecutionService();

        var githubService = Substitute.For<IGithubService>();
        var sp = new ServiceCollection().AddLoggingForTests<GithubIssueMacro>(OutputHelper)
                                        .AddSingleton<ISettingsFacade, SettingsFacadeService>()
                                        .AddMockSingleton<IApplicationConfigurationService>()
                                        .AddMockSingleton<IDatabaseConfigurationService>((_, i) =>
                                        {
                                            var config = new DatabaseConfiguration
                                            {
                                                Github = { Token = $"{Guid.NewGuid()}" }
                                            };
                                            i.Current.Returns(config);
                                            return i;
                                        })
                                        .AddSingleton(githubService)
                                        .AddMockSingleton<IUserGlobalNotificationService>()
                                        .AddMockSingleton<IEnigma>()
                                        .BuildServiceProvider();

        var macro = new GithubIssueMacro(sp);
        var request = new ExecutionRequest
        {
            OriginatingQuery = cmdline, ExecuteWithPrivilege = false, QueryResult = macro
        };

        try
        {
            await executionService.ExecuteAsync(request); 
            
        }
        catch (Exception) { Assert.Fail("This should not throw an exception"); }

        await githubService.Received()
                           .CreateIssue(
                               Arg.Is<string>(s => s.Contains("some text as paramters")),
                               Arg.Any<string>()
                           );
    }

    [Fact]
    public async Task HandleLuaScriptWithParameters()
    {
        // arrange
        const string queryParameters = "hello world";
        const string originatingQuery = $"alias {queryParameters}";

        var cmdline = Cmdline.Parse(originatingQuery);
        var processLauncher = Substitute.For<IProcessLauncher>();
        var executionService = CreateExecutionService(processLauncher);

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
        await executionService.ExecuteAsync(request);

        // assert
        processLauncher.Received()
                       .Start(
                           Arg.Is<ProcessContext>(psi => psi.Arguments == queryParameters)
                       );
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

    [Theory]
    [InlineData("$i$")]
    [InlineData("$I$")]
    [InlineData("$w$")]
    [InlineData("$W$")]
    public async Task UpdateFileNameAfterScriptExecution(string fileName)
    {
        // arrange
        const string prefix = "prefix:";
        fileName = $"{prefix}{fileName}";
        
        const string numericParameters = "12";
        const string updatedFileName = $"{prefix}updated-{numericParameters}";
        const string script = $"""
                               if tonumber(context.Parameters) ~= nil then
                                   context.Parameters = "updated-" .. context.Parameters
                               end

                               return context
                               """;
        const string originatingQuery = $"alias {numericParameters}";

        var processLauncher = Substitute.For<IProcessLauncher>();
        var executionService = CreateExecutionService(processLauncher);
        var cmdline = Cmdline.Parse(originatingQuery);
        var request = new ExecutionRequest
        {
            OriginatingQuery = originatingQuery,
            QueryResult = new AliasQueryResult
            {
                FileName = fileName, LuaScript = script, OriginatingQuery = cmdline
            }
        };

        // act
        await executionService.ExecuteAsync(request);

        // assert
        processLauncher.Received()
                       .Start(
                           Arg.Is<ProcessContext>(psi => psi.FileName == updatedFileName)
                       );
    }

    [Theory]
    [InlineData("$i$")]
    [InlineData("$I$")]
    [InlineData("$w$")]
    [InlineData("$W$")]
    public async Task UpdateParametersAfterScriptExecution(string queryParameter)
    {
        // arrange
        const string numericParameters = "12";
        const string updatedParameters = $"updated-{numericParameters}";
        const string script =
            $"""
             if tonumber(context.Parameters) ~= nil then
                 context.Parameters = "{updatedParameters}"
             end

             return context
             """;
        var originatingQuery = $"alias {numericParameters}";

        var cmdline = Cmdline.Parse(originatingQuery);
        var processLauncher = Substitute.For<IProcessLauncher>();
        var executionService = CreateExecutionService(processLauncher);

        var request = new ExecutionRequest
        {
            OriginatingQuery = originatingQuery,
            QueryResult = new AliasQueryResult
            {
                FileName = "alias",
                LuaScript = script,
                OriginatingQuery = cmdline,
                Parameters = queryParameter
            }
        };

        // act
        await executionService.ExecuteAsync(request);

        // assert
        processLauncher.Received()
                       .Start(
                           Arg.Is<ProcessContext>(psi => psi.Arguments == updatedParameters)
                       );
    }

    #endregion
}