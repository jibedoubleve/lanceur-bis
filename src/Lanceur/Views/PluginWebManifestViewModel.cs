using Lanceur.Core.Plugins;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;

namespace Lanceur.Views
{
    public class PluginWebManifestViewModel : ReactiveObject, IPluginWebManifest
    {
        #region Constructors

        public PluginWebManifestViewModel()
        {
        }

        #endregion Constructors

        #region Properties

        public Action Close { get; internal set; }
        [Reactive] public string Description { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public string Url { get; set; }
        [Reactive] public Version Version { get; set; }

        #endregion Properties
    }
}