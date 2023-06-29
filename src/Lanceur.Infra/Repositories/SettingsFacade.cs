using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;

namespace Lanceur.Infra.Repositories
{
    public class SettingsFacade : ISettingsFacade
    {
        #region Fields

        private readonly IAppConfigRepository _appConfigRepository;
        private readonly IDatabaseConfigRepository _databaseConfigRepository;

        #endregion Fields

        #region Constructors

        public SettingsFacade(IDatabaseConfigRepository databaseConfigRepository, IAppConfigRepository appConfigRepository)
        {
            _databaseConfigRepository = databaseConfigRepository;
            _appConfigRepository = appConfigRepository;
        }

        #endregion Constructors

        #region Properties

        public AppConfig Application => _appConfigRepository.Current;
        public IDatabaseConfig Database => _databaseConfigRepository.Current;

        #endregion Properties

        #region Methods

        public void Save()
        {
            _databaseConfigRepository.Save();
            _appConfigRepository.Save();
        }

        #endregion Methods
    }
}