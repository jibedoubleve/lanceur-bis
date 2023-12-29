namespace Lanceur.Core.Models
{
    public class PackagedApp
    {
        #region Properties

        public string AppUserModelId { get; init; }
        public string Description { get; init; }
        public string DisplayName { get; init; }
        public string InstalledLocation { get; init; }
        public Uri Logo { get; init; }

        #endregion Properties
    }
}