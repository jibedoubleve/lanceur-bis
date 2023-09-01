﻿using Lanceur.Core.Plugins;
using Lanceur.Core.Services;
using Newtonsoft.Json;
using System.IO.Compression;

namespace Lanceur.Infra.Plugins
{
    public class PluginInstaller : IPluginInstaller
    {
        #region Fields

        private readonly IAppLogger _log;

        #endregion Fields

        #region Constructors

        public PluginInstaller(IAppLoggerFactory appLoggerFactory)
        {
            ArgumentNullException.ThrowIfNull(appLoggerFactory, nameof(appLoggerFactory));

            _log = appLoggerFactory.GetLogger<PluginInstaller>();
        }

        #endregion Constructors

        #region Methods

        private static void InstallFiles(string destination, ZipArchive zip)
        {
            foreach (var entry in zip.Entries)
            {
                // Gets the full path to ensure that relative segments are removed.
                string destinationPath = Path.GetFullPath(Path.Combine(destination, entry.FullName));

                var directory = Path.GetDirectoryName(destinationPath);
                if (!Directory.Exists(directory)) { Directory.CreateDirectory(directory); }

                entry.ExtractToFile(destinationPath);
            }
        }

        public IPluginManifest Install(string packagePath)
        {
            using var zip = ZipFile.OpenRead(packagePath);
            var config = (from entry in zip.Entries
                          where entry.Name == PluginLocation.ManifestName
                          select entry).SingleOrDefault();

            if (config == null)
            {
                _log.Warning($"No plugin manifest in package '{packagePath}'");
                return null;
            }

            using var stream = config.Open();
            using var reader = new StreamReader(stream);

            var json = reader.ReadToEnd();
            var manifest = JsonConvert.DeserializeObject<PluginManifest>(json);

            InstallFiles(
                new PluginLocation(manifest).DirectoryPath,
                zip);

            return manifest;
        }

        public async Task<IPluginManifest> InstallFromWebAsync(string url)
        {
            url = PluginWebManifestMetadata.ExpandUrl(url);
            var path = Path.GetTempFileName();

            // Download & copy to temp directory
            using (var client = new HttpClient())
            using (var response = await client.GetAsync(url))
            using (var stream = new FileStream(path, FileMode.OpenOrCreate))
            {
                await response.Content.CopyToAsync(stream);
            }

            return Install(path);

        }
        #endregion Methods
    }
}