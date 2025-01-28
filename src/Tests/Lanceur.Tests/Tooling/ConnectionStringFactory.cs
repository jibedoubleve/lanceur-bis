using Lanceur.Core.Models.Settings;
using Lanceur.Core.Utils;
using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Tests.Tooling;

public static class ConnectionStringFactory
{
    #region Properties

    /// <summary>
    /// Creates a connection string for a SQLite database stored on the desktop in the "Databases" folder.
    /// The file is named "Output_Test_Database__{date-and-time}.sqlite", where {date-and-time} represents the current timestamp.
    /// This configuration is intended for debugging purposes only and should not be used in CI/CD environments.
    /// </summary>
    public static IConnectionString InDesktop
    {
        get
        {
            var desktop = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Databases");
            if (!Directory.Exists(desktop)) Directory.CreateDirectory(desktop);
            var path = Path.Join(
                desktop,
                $"Output_Test_Database__{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.sqlite"
            ).ToSQLiteConnectionString();
            return new ConnectionString(path);
        }
    }

    /// <summary>
    /// Creates a connection string for an in-memory SQLite database.
    /// </summary>
    public static IConnectionString InMemory => new ConnectionString("Data Source =:memory:");

    #endregion

    private class ConnectionString : IConnectionString
    {
        #region Fields

        private readonly string _connectionString;

        #endregion

        #region Constructors

        public ConnectionString(string connectionString) => _connectionString = connectionString;

        #endregion

        #region Methods

        public override string ToString() => _connectionString;

        #endregion
    }
}