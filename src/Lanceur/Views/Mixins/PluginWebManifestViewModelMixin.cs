using AutoMapper;
using Lanceur.Core.Plugins;
using System.Collections.Generic;

namespace Lanceur.Views.Mixins;

public static class PluginWebManifestViewModelMixin
{
    #region Fields

    private static readonly Mapper _mapper;

    private static readonly MapperConfiguration _mapperConfig = new(cfg => { cfg.CreateMap<PluginWebManifest, PluginWebManifestViewModel>(); });

    #endregion Fields

    #region Constructors

    static PluginWebManifestViewModelMixin() => _mapper = new(_mapperConfig);

    #endregion Constructors

    #region Methods

    public static PluginWebManifestViewModel[] ToViewModel(this IEnumerable<IPluginWebManifest> pluginConfigurations) => _mapper.Map<IEnumerable<IPluginWebManifest>, PluginWebManifestViewModel[]>(pluginConfigurations);

    public static PluginWebManifestViewModel ToViewModel(this IPluginWebManifest pluginConfiguration) => _mapper.Map<IPluginWebManifest, PluginWebManifestViewModel>(pluginConfiguration);

    #endregion Methods
}