using System.Data;
using Lanceur.Core.BusinessLogic;
using Lanceur.Core.Models;
using Lanceur.SharedKernel.Logging;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.SQLite.DbActions;

public class AliasSaveDbAction
{
    #region Fields

    private readonly IDbActionFactory _dbActionFactory;
    private readonly ILogger<AliasSaveDbAction> _logger;

    #endregion

    #region Constructors

    public AliasSaveDbAction(IDbActionFactory dbActionFactory, ILoggerFactory factory)
    {
        _dbActionFactory = dbActionFactory;
        _logger = factory.GetLogger<AliasSaveDbAction>();
    }

    #endregion

    #region Methods

    public void SaveOrUpdate(IDbTransaction tx, ref AliasQueryResult alias)
    {
        ArgumentNullException.ThrowIfNull(alias);
        ArgumentNullException.ThrowIfNull(alias.Synonyms);
        ArgumentNullException.ThrowIfNull(alias.Id);

        alias.SanitizeSynonyms();
       
        using var _ = _logger.BeginSingleScope("UpdatedAlias", alias);

        _dbActionFactory.AliasManagement.SaveOrUpdate(tx, ref alias);

        // Reset state after save
        alias.SynonymsWhenLoaded = alias.Synonyms;
    }

    public void SaveOrUpdate(IDbTransaction tx, IEnumerable<AliasQueryResult> aliases)
    {
        foreach (var alias in aliases)
        {
            var a = alias;
            SaveOrUpdate(tx, ref a);
        }
    }

    #endregion
}