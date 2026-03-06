namespace Lanceur.Core.Models;

public class DisplayQueryResult : QueryResult
{
    #region Constructors

    public DisplayQueryResult(string name, string? description = null, string? iconKind = null)
    {
        Name = name;
        Description = description ?? string.Empty;
        if (iconKind is not null) { base.Icon = iconKind; }
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets a value indicating whether this <see cref="QueryResult" /> represents an actual result to display
    ///     or a message indicating that there are no results to show.
    /// </summary>
    public override bool IsResult => false;

    public static IEnumerable<QueryResult> NoResultFound
        => SingleFromResult("No result found", iconKind: "AlertCircleOutline");

    #endregion

    #region Methods

    public static IEnumerable<QueryResult> SingleFromResult(
        string text, string? subtext = null, string? iconKind = null)
        => new List<QueryResult> { new DisplayQueryResult(text, subtext, iconKind) };

    #endregion
}