using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;

namespace Lanceur.Ui.Core.ReservedAliases;

[ReservedAlias("help")]
[Description("Open help page")]
public sealed class HelpAlias : SelfExecutableQueryResult
{
    #region Fields

    private const string Url = "https://jibedoubleve.github.io/lanceur-bis/";

    private readonly IProcessLauncher _processLauncher;

    #endregion

    #region Constructors

    public HelpAlias(IProcessLauncher processLauncher) => _processLauncher = processLauncher;

    #endregion

    #region Properties

    public override string Icon => "ChatHelp24";

    #endregion

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        _processLauncher.Open(Url);
        return NoResultAsync;
    }

    #endregion
}