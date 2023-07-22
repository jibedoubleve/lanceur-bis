using AutoMapper;
using Lanceur.Core.Models;
using System.Collections.Generic;

namespace Lanceur.Views.Mixins
{
    public static class PluginManifestViewModelMixin
    {
        #region Fields

        private static readonly Mapper _mapper;

        private static readonly MapperConfiguration _mapperConfig = new(cfg =>
        {
            cfg.CreateMap<PluginManifest, PluginManifestViewModel>()
               .ForMember(x => x.IsVisible, opt => opt.Ignore());
        });

        #endregion Fields

        #region Constructors

        static PluginManifestViewModelMixin()
        {
            _mapper = new Mapper(_mapperConfig);
        }

        #endregion Constructors

        #region Methods

        public static PluginManifestViewModel[] ToViewModel(this IEnumerable<IPluginManifest> pluginConfigurations)
        {
            return _mapper.Map<IEnumerable<IPluginManifest>, PluginManifestViewModel[]>(pluginConfigurations);
        }

        public static PluginManifestViewModel ToViewModel(this IPluginManifest pluginConfiguration)
        {
            return _mapper.Map<IPluginManifest, PluginManifestViewModel>(pluginConfiguration);
        }

        #endregion Methods
    }
}