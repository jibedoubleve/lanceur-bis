using System.Data;
using Lanceur.Core.BusinessLogic;
using Lanceur.Core.Models;
using Lanceur.SharedKernel.Logging;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.SQLite.DbActions;

public sealed class AliasSaveDbAction
{
    #region Fields

    private readonly IDbActionFactory _dbActionFactory;
    private readonly ILogger<AliasSaveDbAction> _logger;

    #endregion

    #region Constructors

    public AliasSaveDbAction(IDbActionFactory dbActionFactory, ILoggerFactory factory)
    {
        _dbActionFactory = dbActionFactory;
        _logger = factory.CreateLogger<AliasSaveDbAction>();
    }

    #endregion

    #region Methods

    public void SaveOrUpdate(IDbTransaction tx, AliasQueryResult alias)
    {
        ArgumentNullException.ThrowIfNull(alias);
        ArgumentNullException.ThrowIfNull(alias.Synonyms);

        alias.SanitizeSynonyms();
        alias.SanitizeFileName();

        using var _ = _logger.BeginSingleScope("UpdatedAlias", alias);

        _dbActionFactory.AliasManagement.SaveOrUpdate(tx, alias);

        // Reset state after save
        alias.SynonymsWhenLoaded = alias.Synonyms;
    }

    public void SaveOrUpdate(IDbTransaction tx, IEnumerable<AliasQueryResult> aliases)
    {
        foreach (var alias in aliases)
        {
            SaveOrUpdate(tx, alias);
        }
    }

    #endregion
}