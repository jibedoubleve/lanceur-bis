﻿namespace Lanceur.Core.Plugins
{
    public class PluginWebManifest : PluginManifestBase, IPluginWebManifest
    {
        #region Properties

        public string Url { get; set; }
        public string Dll { get; set; }

        #endregion Properties
    }
}