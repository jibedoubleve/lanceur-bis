namespace Lanceur.Core.Plugins;

public interface IMaintenanceLogBook
{
    #region Methods

    Task<IEnumerable<MaintenanceCandidate>> GetInstallCandidatesAsync();

    Task<IEnumerable<MaintenanceCandidate>> GetUninstallCandidatesAsync();

    Task SaveAsync(MaintenanceCandidate[] candidates);
    
    Task SaveAsync(MaintenanceCandidate candidate);

    #endregion Methods
}