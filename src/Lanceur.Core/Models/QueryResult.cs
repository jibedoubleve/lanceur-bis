using System.ComponentModel;
using System.Diagnostics;

namespace Lanceur.Core.Models;

/// <summary>
///     Base of a result displayed after a query.
/// </summary>
/// <remarks>
///     Note that <see cref="INotifyPropertyChanged" /> is partially implemented.
///     Only <see cref="Thumbnail" /> notifies the event as this is the only property
///     that is designed to react on modifications.
/// </remarks>
[DebuggerDisplay("({Id}) {Name} - Desc: {Description}")]
public abstract class QueryResult : ObservableModel
{
    #region Fields

    private int _count  ;

    private string _thumbnail;

    #endregion

    #region Properties

    protected static Task<IEnumerable<QueryResult>> NoResultAsync => Task.FromResult(NoResult);

    /// <summary>
    ///     Gets or sets the count of how many times this QueryResult has been executed.
    /// </summary>
    /// <remarks>
    ///     If the value is negative, it indicates that the counter is not used
    ///     (In this case the UI should not display this information).
    /// </remarks>
    public int Count
    {
        get => _count;
        set => SetField(ref _count, value);
    }

    public virtual string Description { get; set; }

    /// <summary>
    ///     Fall back for <see cref="Thumbnail" />. This property is expected to
    ///     contain information to display an icon (or whatever) in the UI
    ///     when <see cref="Thumbnail" /> is <c>null</c>.
    /// </summary>
    /// <remarks>
    ///     The fallback behaviour is expected to be handled in the UI
    /// </remarks>
    public virtual string Icon { get; set; } = "Rocket24";

    public long Id { get; set; }

    /// <summary>
    ///     Indicates whether the application should ask for confirmation from the
    ///     user before executing this alias.
    /// </summary>
    public bool IsExecutionConfirmationRequired { get; set; }

    /// <summary>
    ///     Indicates whether this item is the result of a search or if it is an
    ///     item used to provide information to the user.
    /// </summary>
    public virtual bool IsResult => true;

    /// <summary>
    ///     Indicates whether the thumbnail should be disabled. If disabled, then the
    ///     icon is used as a fallback.
    /// </summary>
    public bool IsThumbnailDisabled { get; set; }

    public string Name { get; set; } = string.Empty;

    public static IEnumerable<QueryResult> NoResult => new List<QueryResult>();

    public Cmdline Query { get; set; } = Cmdline.Empty;

    /// <summary>
    ///     Represents a thumbnail to display in the UI. Set to <c>null</c> to display the <see cref="Icon" /> instead.
    /// </summary>
    public string Thumbnail
    {
        get => _thumbnail;
        set => SetField(ref _thumbnail, value);
    }

    #endregion

    #region Methods

    public virtual string ToQuery() => $"{Name}";

    #endregion
}