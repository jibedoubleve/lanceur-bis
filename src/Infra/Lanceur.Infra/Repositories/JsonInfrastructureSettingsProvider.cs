using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Constants;
using Lanceur.Core.Repositories.Config;
using Lanceur.SharedKernel.Extensions;
using Newtonsoft.Json;

namespace Lanceur.Infra.Repositories;

public class JsonInfrastructureSettingsProvider : IInfrastructureSettingsProvider
{
    #region Fields

    private static readonly object Locker = new();
    private readonly string _filePath;
    private InfrastructureSettings _current;

    #endregion Fields

    #region Constructors

    public JsonInfrastructureSettingsProvider(string path = null)
    {
        path ??= Paths.Settings;
        _filePath = path.ExpandPath();
    }

    #endregion Constructors

    #region Properties

    public InfrastructureSettings Current
    {
        get
        {
            if (_current is null) Load();
            return _current;
        }
    }

    #endregion Properties

    #region Methods

    private FileStream OpenFile()
    {
        var dir = _filePath.GetDirectoryName();
        if (dir.IsNullOrEmpty()) throw new DirectoryNotFoundException($"Directory '{dir}' does not exist.");

        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir!);

        if (File.Exists(_filePath)) File.Delete(_filePath);

        return File.Create(_filePath);
    }

    public void Load()
    {
        lock (Locker)
        {
            InfrastructureSettings jsonConfiguration = null;
            if (File.Exists(_filePath))
            {
                var output = File.ReadAllText(_filePath);
                jsonConfiguration = JsonConvert.DeserializeObject<InfrastructureSettings>(output);
            }

            _current = jsonConfiguration ?? new InfrastructureSettings();
        }
    }

    public void Save()
    {
        lock (Locker)
        {
            // If _current is null, it was never loaded, then
            // not modified...
            if (_current is null) return;

            var json = JsonConvert.SerializeObject(_current);

            using var stream = OpenFile();
            using var file = new StreamWriter(stream);
            file.Write(json);
        }
    }

    #endregion Methods
}