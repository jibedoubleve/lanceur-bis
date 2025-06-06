﻿using Lanceur.Core.Constants;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;
using Lanceur.Infra.Services;
using Lanceur.SharedKernel.Extensions;
using Newtonsoft.Json;

namespace Lanceur.Infra.Repositories;

public class JsonApplicationConfigurationService : IApplicationConfigurationService
{
    #region Fields

    private static readonly object Locker = new();
    private readonly string _filePath;
    private ApplicationSettings _current;

    #endregion Fields

    #region Constructors

    public JsonApplicationConfigurationService(string path = null)
    {
        path ??= Paths.Settings;
        _filePath = path.ExpandPath();
    }

    #endregion Constructors

    #region Properties

    public ApplicationSettings Current
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
            ApplicationSettings jsonSettings = null;
            if (File.Exists(_filePath))
            {
                var output = File.ReadAllText(_filePath);
                jsonSettings = JsonConvert.DeserializeObject<ApplicationSettings>(output);
            }

            _current = jsonSettings ?? new ApplicationSettings();
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