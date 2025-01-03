namespace Lanceur.Core.Models.Settings;

/// <summary>
/// Defines the settings that are not intended to be stored in the database, 
/// typically including the database file path.
/// </summary>
public interface IApplicationSettings
{
    #region Properties

    /// <summary>
    /// Gets or sets the file path to the database that stores the application's configuration data.
    /// This path is used to locate the database but is not stored within the database itself.
    /// </summary>
    string DbPath { get; set; }

    #endregion Properties
}