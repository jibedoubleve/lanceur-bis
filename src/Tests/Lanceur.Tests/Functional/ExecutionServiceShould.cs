using FluentAssertions;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Requests;
using Lanceur.Core.Services;
using Lanceur.Infra.Macros;
using Lanceur.Infra.Managers;
using Lanceur.Infra.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Lanceur.Tests.Functional;

public class ExecutionServiceShould
{
    #region Methods

    [Theory, InlineData("ini", "thb@joplin@spotify")]
    public async Task ExecuteMultiMacro(string cmd, string parameters)
    {
        var cmdline = new Cmdline(cmd, parameters);
        var executionManager = new ExecutionService(
            Substitute.For<ILoggerFactory>(),
            Substitute.For<IWildcardService>(),
            Substitute.For<IAliasRepository>()
        );

        var macro =new MultiMacro(Substitute.For<IExecutionService>(),Substitute.For<ISearchService>(), 0);
        await macro.ExecuteAsync(cmdline);

        var request = new ExecutionRequest
        {
            Query = cmdline,
            ExecuteWithPrivilege = false, 
            QueryResult = macro
        };

        await executionManager.ExecuteAsync(request);
    }

    #endregion Methods
}