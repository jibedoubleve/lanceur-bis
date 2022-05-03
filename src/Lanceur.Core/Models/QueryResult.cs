using System.ComponentModel;
using System.Diagnostics;

namespace Lanceur.Core.Models
{
    /// <summary>
    /// Base of a result displayed after a query.
    /// </summary>
    /// <remarks>
    /// Note that <see cref="INotifyPropertyChanged"/> is partially implemented.
    /// Only <see cref="Thumbnail"/> notifies the event as this is the only property
    /// that is designed to react on modifications.
    /// </remarks>
    [DebuggerDisplay("{Name} - Desc: {Description}")]
    public abstract class QueryResult : INotifyPropertyChanged
    {
        #region Fields

        private string _name = string.Empty;
        private string _oldName;

        private object _thumbnail;

        #endregion Fields

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public static IEnumerable<QueryResult> NoResult => new List<QueryResult>();
        public static Task<IEnumerable<QueryResult>> NoResultAsync => Task.FromResult(NoResult);
        public int Count { get; set; } = 0;
        public virtual string Description { get; }

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

        public string Name
        {
            get => _name;
            set
            {
                // First time you set the name of the QueryResult, it'll save
                // the value. Now, OldName will have the value of the first time
                // it was set.
                if (value != null && _oldName == null) { _oldName = value.ToLower(); }
                _name = value;
            }
        }

        public string OldName => _oldName;

        public Cmdline Query { get; set; } = Cmdline.Empty;

        /// <summary>
        /// Contains a thumbnail that can be displayed on the UI. Put <c>null</c> if you want
        /// to let <see cref="Icon"/> handle this.
        /// </summary>
        /// <remarks>
        /// The type is <see cref="object"/> to avoid UI technology leaking into Infrastructure
        /// </remarks>
        public object Thumbnail
        {
            get => _thumbnail;
            set
            {
                _thumbnail = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Thumbnail)));
            }
        }

        #endregion Properties
    }
}