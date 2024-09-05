using FluentAssertions;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Requests;
using Lanceur.Core.Services;
using Lanceur.Infra.Macros;
using Lanceur.Infra.Managers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Lanceur.Tests.Functional;

public class ExecutionManagerShould
{
    #region Methods

    [Theory, InlineData("ini", "thb@joplin@spotify")]
    public async Task ExecuteMultiMacro(string cmd, string parameters)
    {
        var cmdline = new Cmdline(cmd, parameters);
        var executionManager = new ExecutionManager(
            Substitute.For<ILoggerFactory>(),
            Substitute.For<IWildcardManager>(),
            Substitute.For<IDbRepository>()
        );

        var macro =new MultiMacro(0, Substitute.For<IExecutionManager>(),Substitute.For<IAsyncSearchService>());
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