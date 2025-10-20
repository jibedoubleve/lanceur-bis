namespace Lanceur.Core.Configuration.Configurations;

public static class DatabaseConfigurationExtensions
{
    #region Methods

    public static void SetHotKey(this DatabaseConfiguration databaseConfiguration, int key, int modifierKey) 
        => databaseConfiguration.HotKey = new(modifierKey, key);

    #endregion
}