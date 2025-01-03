using Lanceur.Core.Models.Settings;
using Lanceur.Core.Services;

namespace Lanceur.Core.Repositories.Config;

public interface IDatabaseConfigurationService : IConfigurationService<DatabaseConfiguration>
{
    #region Methods

    void Edit(Action<DatabaseConfiguration> action);

    #endregion Methods
}