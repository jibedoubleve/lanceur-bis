using Lanceur.Core.Constants;

namespace Lanceur.Core.Configuration.Sections.Infrastructure;

public sealed class DatabaseSection
{
    #region Properties

    /// <summary>
    ///     Gets or sets the file system path to the SQLite database.
    ///     Defaults to <c>"%appdata%\probel\lanceur2\data.sqlite"</c>.
    /// </summary>
    public string DbPath { get; set; } = Paths.DefaultDb;

    #endregion
}