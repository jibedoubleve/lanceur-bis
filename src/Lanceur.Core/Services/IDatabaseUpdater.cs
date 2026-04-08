namespace Lanceur.Core.Services;

/// <summary>
///     This interface responsible for ensuring that the SQLite database is updated to the latest version.
/// </summary>
public interface IDatabaseUpdater
{
    /// <summary>
    ///     Checks whether the database specified by the connection string needs an update.
    ///     If an update is required, applies the necessary Data Definition Language (DDL) scripts
    ///     to bring the database to the latest version.
    /// </summary>
    /// <param name="connectionString">The connection string pointing to the SQLite database.</param>
    void Update(string connectionString);
}