using System.ComponentModel;
using System.Diagnostics;

namespace Lanceur.Core.Models;

/// <summary>
/// Base of a result displayed after a query.
/// </summary>
/// <remarks>
/// Note that <see cref="INotifyPropertyChanged"/> is partially implemented.
/// Only <see cref="Thumbnail"/> notifies the event as this is the only property
/// that is designed to react on modifications.
/// </remarks>
[DebuggerDisplay("({Id}) {Name} - Desc: {Description}")]
public abstract class QueryResult : ObservableModel
{
    #region Fields

    private string _thumbnail;

    #endregion Fields

    #region Properties

    protected static Task<IEnumerable<QueryResult>> NoResultAsync => Task.FromResult(NoResult);

    public static IEnumerable<QueryResult> NoResult => new List<QueryResult>();
    public int Count { get; set; }
    public virtual string Description { get; set; }

    /// <summary>
    /// Fall back for <see cref="Thumbnail"/>. This property is expected to
    /// contain information to display an icon (or whatever) in the UI
    /// when <see cref="Thumbnail"/> is <c>null</c>.
    /// </summary>
    /// <remarks>
    /// The fallback behaviour is expected to be handled in the UI
    /// </remarks>
    public virtual string Icon { get; set; } = "RocketLaunchOutline";

    public long Id { get; set; }

    /// <summary>
    /// Indicates whether this item is the result of a search or if it is an
    /// item used to provide information to the user.
    /// </summary>
    public virtual bool IsResult => true;

    public string Name { get; set; } = string.Empty;
    public Cmdline Query { get; set; } = Cmdline.Empty;

    /// <summary>
    /// Contains a thumbnail that can be displayed on the UI. Put <c>null</c> if you want
    /// to let <see cref="Icon"/> handle this.
    /// </summary>
    public string Thumbnail
    {
        get => _thumbnail;
        set => Set(ref _thumbnail, value);
    }

    #endregion Properties

    #region Methods

    public virtual string ToQuery() => $"{Name}";

    #endregion Methods
}