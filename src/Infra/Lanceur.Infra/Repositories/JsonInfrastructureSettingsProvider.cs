using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Constants;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lanceur.Infra.Repositories;

public sealed class JsonInfrastructureSettingsProvider : ISettingsProvider<InfrastructureSettings>
{
    #region Fields

    private InfrastructureSettings? _current;
    private readonly string _filePath;

    private static readonly Lock Locker = new();

    #endregion

    #region Constructors

    public JsonInfrastructureSettingsProvider(string? path = null)
    {
        path ??= Paths.Settings;
        _filePath = path.ExpandPath();
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    object ISettingsProvider.Value => Value;

    /// <inheritdoc />
    public InfrastructureSettings Value
    {
        get
        {
            if (_current is null) { Load(); }

            return _current!;
        }
    }

    #endregion

    #region Methods

    private FileStream OpenFile()
    {
        var dir = _filePath.GetDirectoryName();
        if (dir.IsNullOrEmpty()) { throw new DirectoryNotFoundException($"Directory '{dir}' does not exist."); }

        if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir!); }

        if (File.Exists(_filePath)) { File.Delete(_filePath); }

        return File.Create(_filePath);
    }

    /// <inheritdoc />
    public void Load()
    {
        lock (Locker)
        {
            InfrastructureSettings? jsonConfiguration = null;
            if (File.Exists(_filePath))
            {
                var output = File.ReadAllText(_filePath);
                jsonConfiguration = JsonConvert.DeserializeObject<InfrastructureSettings>(
                    output,
                    new JsonSerializerSettings { Converters = { new StringEnumConverter() } }
                );
            }

            _current = jsonConfiguration ?? new InfrastructureSettings();
        }
    }

    /// <inheritdoc />
    public void Save()
    {
        lock (Locker)
        {
            // If _current is null, it was never loaded, then
            // not modified...
            if (_current is null) { return; }

            var json = JsonConvert.SerializeObject(_current);

            using var stream = OpenFile();
            using var file = new StreamWriter(stream);
            file.Write(json);
        }
    }

    #endregion
}