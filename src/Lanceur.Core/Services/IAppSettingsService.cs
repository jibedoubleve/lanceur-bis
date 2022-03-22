using Lanceur.Core.Models.Settings;

namespace Lanceur.Core.Services
{
    public interface IAppSettingsService
    {
        #region Methods

        void Edit(Action<AppSettings> action);

        AppSettings Load();

        void Save(AppSettings settings);

        #endregion Methods
    }
}