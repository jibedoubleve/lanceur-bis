using AutoMapper;
using Lanceur.Core.Models;

namespace Lanceur.Views.Helpers
{
    public static class PluginConfigurationViewModelMixin
    {
        #region Fields

        private static readonly Mapper _mapper;

        private static readonly MapperConfiguration _mapperConfig = new(cfg =>
        {
            cfg.CreateMap<PluginConfiguration, PluginConfigurationViewModel>()
               .ForMember(x => x.IsVisible, opt => opt.Ignore());
        });

        #endregion Fields

        #region Constructors

        static PluginConfigurationViewModelMixin()
        {
            _mapper = new Mapper(_mapperConfig);
        }

        #endregion Constructors

        #region Methods

        public static PluginConfigurationViewModel[] ToViewModel(this IPluginConfiguration[] pluginConfigurations)
        {
            return _mapper.Map<IPluginConfiguration[], PluginConfigurationViewModel[]>(pluginConfigurations);
        }

        public static PluginConfigurationViewModel ToViewModel(this IPluginConfiguration pluginConfiguration)
        {
            return _mapper.Map<IPluginConfiguration, PluginConfigurationViewModel>(pluginConfiguration);
        }

        #endregion Methods
    }
}