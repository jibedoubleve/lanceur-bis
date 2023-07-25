namespace Lanceur.Core.Plugins
{
    public class UninstallCandidate
    {
        #region Constructors

        public UninstallCandidate(string path)
        { Directory = path; }

        public UninstallCandidate()
        { }

        #endregion Constructors

        #region Properties

        public string Directory { get; set; }

        #endregion Properties
    }
}