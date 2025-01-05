using System.Data;
using Lanceur.Core.BusinessLogic;
using Lanceur.Core.Models;
using Lanceur.Infra.Logging;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.SQLite.DbActions;

public class AliasSaveDbAction
{
    private readonly IDbActionFactory _dbActionFactory;
    private readonly ILogger<AliasSaveDbAction> _logger;

    public AliasSaveDbAction(IDbActionFactory dbActionFactory, ILoggerFactory factory)
    {
        _dbActionFactory = dbActionFactory;
        _logger = factory.GetLogger<AliasSaveDbAction>();
    }
    #region Methods

    public void SaveOrUpdate(IDbTransaction tx, ref AliasQueryResult alias)
    {
        ArgumentNullException.ThrowIfNull(alias);
        ArgumentNullException.ThrowIfNull(alias.Synonyms);
        ArgumentNullException.ThrowIfNull(alias.Id);

        alias.SanitizeSynonyms();
        var action = _dbActionFactory.AliasManagement;

        using var _ = _logger.BeginSingleScope("UpdatedAlias", alias);

        switch (alias.Id)
        {
            case 0 when !action.HasNames(alias, tx):
                action.Create(tx, ref alias);
                _logger.LogInformation("Created new alias {AliasName}", alias.Name);
                break;

            case > 0:
                _logger.LogInformation("Updating alias {AliasName}", alias.Name);
                action.Update(alias, tx);
                break;
        }

        // Reset state after save
        alias.SynonymsWhenLoaded = alias.Synonyms;
    }

    public void Update(IDbTransaction tx, IEnumerable<AliasQueryResult> aliases)
    {
        foreach (var alias in aliases)
        {
            var a = alias; // In this case, this is only an update.
            SaveOrUpdate(tx, ref a);
        }
    }

    #endregion
}