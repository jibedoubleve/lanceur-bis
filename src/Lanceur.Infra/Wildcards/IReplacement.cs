namespace Lanceur.Infra.Wildcards
{
    public interface IReplacement
    {
        #region Properties

        string Wildcard { get; }

        #endregion Properties

        #region Methods

        string Replace(string text, string param);

        #endregion Methods
    }
}