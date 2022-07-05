namespace Lanceur.Core.Plugins.Models
{
    public class ResultItem
    {
        #region Constructors

        public ResultItem(IPlugin plugin)
        {
            if (plugin is null) { throw new ArgumentNullException(nameof(plugin)); }
            else
            {
                Plugin = plugin;
                Description = Plugin?.Description ?? string.Empty;
                Name = Plugin?.Name ?? string.Empty;
            }
        }

        public ResultItem(string name, string description, string icon = null)
        {
            Name = name ?? string.Empty;
            Description = description ?? string.Empty;
            Icon = icon ?? "CellFunction";
        }

        #endregion Constructors

        #region Properties

        protected static IEnumerable<ResultItem> NoResult => new List<ResultItem>();
        protected static Task<IEnumerable<ResultItem>> NoResultAsync => Task.FromResult(NoResult);
        protected IPlugin Plugin { get; }

        public string Description { get; }

        /// <summary>
        /// Fall back for <see cref="Thumbnail"/>. This property is expected to
        /// contain information to display an icon (or whatever) in the UI
        /// when <see cref="Thumbnail"/> is <c>null</c>.
        /// </summary>
        /// <remarks>
        /// The fallback behaviour is expected to be handled in the UI
        /// </remarks>
        public string Icon { get; set; } = "RocketLaunchOutline";

        /// <summary>
        /// Indicates whether this item is the result of a search (<c>true</c>) or if it is an
        /// item used to provide information to the user (<c>false</c>).
        /// </summary>
        public virtual bool IsResult => true;

        public string Name { get; set; }

        public string Query { get; set; } = string.Empty;

        /// <summary>
        /// Contains a thumbnail that can be displayed on the UI. Put <c>null</c> if you want
        /// to let <see cref="Icon"/> handle this.
        /// </summary>
        /// <remarks>
        /// The type is <see cref="object"/> to avoid UI technology leaking into Infrastructure
        /// </remarks>
        public object Thumbnail { get; set; }

        #endregion Properties
    }
}