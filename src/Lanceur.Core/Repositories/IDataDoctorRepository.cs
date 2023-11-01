namespace Lanceur.Core.Repositories;

public interface IDataDoctorRepository
{
    #region Methods

    Task FixIconsForHyperlinksAsync();

    #endregion Methods
}