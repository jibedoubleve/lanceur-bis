namespace Lanceur.Core.Models.Settings
{
    public class RepositorySection
    {
        #region Properties

        public static RepositorySection Default => new();
        public int ScoreLimit { get; set; }

        #endregion Properties
    }
}