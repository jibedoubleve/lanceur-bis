using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.LuaScripting;
using Shouldly;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Requests;
using Lanceur.Core.Services;
using Lanceur.Infra.LuaScripting;
using Lanceur.Infra.Macros;
using Lanceur.Infra.Repositories;
using Lanceur.Infra.Services;
using Lanceur.Infra.Stores;
using Lanceur.Infra.Wildcards;
using Lanceur.Infra.Win32.Services;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.Launchers;
using Lanceur.Ui.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

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
            new LuaManager(
                Substitute.For<IUserGlobalNotificationService>(),
                Substitute.For<ILogger<LuaManager>>()
            ),
            processLauncher ?? Substitute.For<IProcessLauncher>()
        );
        return executionService;
    }

    [Theory]
    [InlineData("$C$", "hello world", "hello+world")]
    [InlineData("$R$", "un deux / \\ - <", "un deux / \\ - <")]
    [InlineData("$I$", "un deux", "un deux")]
    [InlineData("$W$", "hello world", "hello+world")]
    public async Task When_using_replacements_for_FileName_Then_correct_values_applied(string actual, string parameters, string expected)
    {
        // arrange
        var outputFileName = string.Empty;

        var processLauncher = new ProcessLauncherVisitor(context => outputFileName = context.FileName);
        var originatingQuery = $"alias {parameters}";
        var cmdline = Cmdline.Parse(originatingQuery);

        var clipboard = Substitute.For<IClipboardService>();
        clipboard.RetrieveText().Returns(parameters);
        var executionService = CreateExecutionService(processLauncher, clipboard);

        var request = new ExecutionRequest(
            new AliasQueryResult { FileName = actual, OriginatingQuery = cmdline, Parameters = parameters },
            Cmdline.Parse(originatingQuery)
        );

        // act
        await executionService.ExecuteAsync(request);

        // assert
        outputFileName.ShouldBe(expected);
    }

    [Theory]
    [InlineData("--$C$", "hello world", "--hello+world")]
    [InlineData("--$R$", "un deux / \\ - <", "--un deux / \\ - <")]
    [InlineData("--$I$", "un deux", "--un deux")]
    [InlineData("--$W$", "hello world", "--hello+world")]
    public async Task When_using_replacements_for_Parameters_Then_correct_values_applied(
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

        var request = new ExecutionRequest(
            new AliasQueryResult { FileName = "alias", Parameters = parameters },
            cmdline
        );

        // act
        await executionService.ExecuteAsync(request);

        // assert
        outputParameters.ShouldBe(expectedParameters);
    }

    [Theory]
    [InlineData("application.exe", "-c aa -b bb", "app")]
    [InlineData("application.exe", "-c aa -b bb", "app undeux trois")]
    public async Task When_not_using_replacements_for_Parameters_Then_no_replacement_occured(
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

        var request = new ExecutionRequest(
            new AliasQueryResult { FileName = fileName, Parameters = parameters, OriginatingQuery = cmdline },
            Cmdline.Parse(originatingQuery),
            false
        );

        // act
        await executionService.ExecuteAsync(request);

        // assert
        Assert.Multiple(
            () => outputFileName.ShouldBe(fileName),
            () => outputParameters.ShouldBe(parameters)
        );
    }

    [Theory]
    [InlineData("ini", "thb@joplin@spotify")]
    public async Task When_executing_MultiMacro_Then_no_error(string cmd, string parameters)
    {
        var cmdline = new Cmdline(cmd, parameters);
        var executionService = CreateExecutionService();

        var sp = new ServiceCollection()
                 .AddMockSingleton<IExecutionService>()
                 .AddMockSingleton<ISearchService>()
                 .AddMacroServices()
                 .BuildServiceProvider();

        var macro = new MultiMacro(
            sp.GetService<IExecutionService>(),
            sp.GetService<Lazy<ISearchService>>()
        );
        var request = new ExecutionRequest(macro, cmdline, false);

        try { await executionService.ExecuteAsync(request); }
        catch (Exception) { Assert.Fail("This should not throw an exception"); }
    }
    
    [Theory]
    [InlineData("issue", "with some text as parameters")]
    public async Task When_executing_macro_with_parameters_Then_parameters_are_used(string cmd, string parameters)
    {
        var cmdline = new Cmdline(cmd, parameters);

        var githubService = Substitute.For<IGithubService>();
        var sp = new ServiceCollection().AddConfigurationSections()
                                        .AddTestOutputHelper(OutputHelper)
                                        .AddSingleton<IConfigurationFacade, ConfigurationFacadeService>()
                                        .AddMockSingleton<IInfrastructureSettingsProvider>()
                                        .AddMockSingleton<IApplicationSettingsProvider>((_, i) =>
                                        {
                                            var config = new ApplicationSettings
                                            {
                                                Github = { Token = $"{Guid.NewGuid()}" }
                                            };
                                            i.Current.Returns(config);
                                            return i;
                                        })
                                        .AddLogging(builder =>
                                            builder.AddXUnit(OutputHelper)
                                                   .SetMinimumLevel(LogLevel.Trace))
                                        .AddMockSingleton<IAliasRepository>()
                                        .AddMockSingleton<ILuaManager>()
                                        .AddSingleton<IWildcardService, ReplacementComposite>()
                                        .AddSingleton<IExecutionService, ExecutionService>()
                                        .AddSingleton<ReplacementComposite>()
                                        .AddMockSingleton<IClipboardService>()
                                        .AddTransient<IProcessLauncher, ProcessLauncherWin32>()
                                        .AddSingleton(githubService)
                                        .AddMockSingleton<IUserGlobalNotificationService>()
                                        .AddMockSingleton<IEnigma>()
                                        .AddSingleton<GithubIssueMacro>()
                                        .BuildServiceProvider();

        var macro = sp.GetService<GithubIssueMacro>();
        var request = new ExecutionRequest(macro, cmdline);

        try
        {
            var executionService = sp.GetService<IExecutionService>();
            await executionService.ExecuteAsync(request); 
        }
        catch (Exception ex) { Assert.Fail($"This should not throw an exception {ex.Message}"); }

        await githubService.Received()
                           .CreateIssue(
                               Arg.Is<string>(s => string.Equals(s, parameters)),
                               Arg.Any<string>()
                           );
    }

    [Fact]
    public async Task When_executing_alias_with_parameters_Then_parameters_are_used()
    {
        // arrange
        const string queryParameters = "hello world";
        const string originatingQuery = $"alias {queryParameters}";
        
        var processLauncher = Substitute.For<IProcessLauncher>();
        var executionService = CreateExecutionService(processLauncher);

        var request = new ExecutionRequest(
            new AliasQueryResult
            {
                FileName = "alias",
                LuaScript = """
                            if context.Parameters == nil then
                                context.Parameters = "wrongParameters"
                            end
                            return context
                            """
            },
            Cmdline.Parse(originatingQuery)
        );

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
    public async Task When_executing_multi_macro_Then_no_crash(string cmd, string parameters)
    {
        var cmdline = new Cmdline(cmd, parameters);
        var sp = new ServiceCollection()
                 .AddMockSingleton<IExecutionService>()
                 .AddMockSingleton<ISearchService>()
                 .BuildServiceProvider();

        var macro = new MultiMacro(
            sp.GetService<IExecutionService>(),
            sp.GetService<Lazy<ISearchService>>()
        );

        try { await macro.ExecuteAsync(cmdline); }
        catch (Exception) { Assert.Fail("This should not throw an exception"); }
    }

    [Theory]
    [InlineData("$i$")]
    [InlineData("$I$")]
    [InlineData("$w$")]
    [InlineData("$W$")]
    public async Task When_executing_lua_script_Then_file_name_replacement_is_done_after_script_execution(string fileName)
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
        var request = new ExecutionRequest(
            new AliasQueryResult { FileName = fileName, LuaScript = script, OriginatingQuery = cmdline },
            cmdline
        );

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
    public async Task When_executing_lua_script_Then_parameter_replacement_is_done_after_script_execution(string queryParameter)
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

        var request = new ExecutionRequest(
            new AliasQueryResult
            {
                FileName = "alias",
                LuaScript = script,
                OriginatingQuery = cmdline,
                Parameters = queryParameter
            },
            cmdline
        );

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