﻿using FluentAssertions;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Requests;
using Lanceur.Infra.Managers;
using Lanceur.Macros;
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
        var cmdlineManager = Substitute.For<ICmdlineManager>();
        cmdlineManager
            .BuildFromText(cmdline.ToString())
            .Returns(cmdline);

        var mgr = new ExecutionManager(
            Substitute.For<ILoggerFactory>(),
            Substitute.For<IWildcardManager>(),
            Substitute.For<IDbRepository>(),
            cmdlineManager
        );

        var macro = Substitute.For<MultiMacro>();
        await macro.ExecuteAsync(
            Arg.Do<Cmdline>(
                x => x.Should().NotBeNull()
            )
        );

        var request = new ExecutionRequest { Query = cmdline.ToString(), ExecuteWithPrivilege = false, QueryResult = macro };

        await mgr.ExecuteAsync(request);
    }

    #endregion Methods
}