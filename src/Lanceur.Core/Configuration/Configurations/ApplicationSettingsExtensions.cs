using Lanceur.Core.Configuration.Sections;

namespace Lanceur.Core.Configuration.Configurations;

public static class ApplicationSettingsExtensions
{
    #region Methods

    public static void SetHotKey(this ApplicationSettings applicationSettings, int key, int modifierKey)
        => applicationSettings.HotKey = new HotKeySection(key, modifierKey);

    #endregion
}