using Lanceur.Core.Plugins;
using Newtonsoft.Json;

namespace Lanceur.Infra.Plugins;

public class MaintenanceLogBook : IMaintenanceLogBook
{
    #region Methods

    private async Task<IEnumerable<MaintenanceCandidate>> GetCandidatesAsync(MaintenanceAction maintenanceAction)
    {
        if (!File.Exists(PluginLocation.MaintenanceLogBook))
        {
            return Array.Empty<MaintenanceCandidate>();
        }

        var json = await File.ReadAllTextAsync(PluginLocation.MaintenanceLogBook);
        return (from candidate in JsonConvert.DeserializeObject<IEnumerable<MaintenanceCandidate>>(json)
                where candidate.MaintenanceAction == maintenanceAction
                select candidate).ToArray();
    }

    public async Task<IEnumerable<MaintenanceCandidate>> GetInstallCandidatesAsync() =>
        await GetCandidatesAsync(MaintenanceAction.Install);

    public async Task<IEnumerable<MaintenanceCandidate>> GetUninstallCandidatesAsync() =>
        await GetCandidatesAsync(MaintenanceAction.Uninstall);

    private async Task<List<MaintenanceCandidate>> LoadAsync()
    {
        if (!File.Exists(PluginLocation.MaintenanceLogBook))
        {
            return new();
        }

        var json = await File.ReadAllTextAsync(PluginLocation.MaintenanceLogBook);
        return JsonConvert.DeserializeObject<List<MaintenanceCandidate>>(json);
    }

    public async Task SaveAsync(MaintenanceCandidate[] candidates)
    {
        if ((candidates?.Length ?? 0) == 0)
        {
            return;
        }

        var book = await LoadAsync();
        book.AddRange(candidates);

        var json = JsonConvert.SerializeObject(book);
        await File.WriteAllTextAsync(PluginLocation.MaintenanceLogBook, json);
    }

    public async Task SaveAsync(MaintenanceCandidate candidate)
    {
        await SaveAsync(new[]
        {
            candidate,
        });
    }

    #endregion Methods
}