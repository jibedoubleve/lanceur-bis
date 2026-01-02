namespace Lanceur.Core.Configuration.Configurations;

public static class ApplicationSettingsExtensions
{
    #region Methods

    public static void SetHotKey(this ApplicationSettings applicationSettings, int key, int modifierKey)
    {
        applicationSettings.HotKey.Key = key;
        applicationSettings.HotKey.ModifierKey = modifierKey;
    }

    #endregion
}