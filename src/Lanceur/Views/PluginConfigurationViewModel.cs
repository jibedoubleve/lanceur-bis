using AutoMapper;
using Lanceur.Core.Models;

namespace Lanceur.Views
{
    public static class PluginConfigurationViewModelMixin
    {
        #region Fields

        private static readonly Mapper _mapper;

        private static readonly MapperConfiguration _mapperConfig = new(cfg =>
        {
            cfg.CreateMap<PluginConfiguration, PluginConfigurationViewModel>();
        });

        #endregion Fields

        #region Constructors

        static PluginConfigurationViewModelMixin()
        {
            _mapper = new Mapper(_mapperConfig);
        }

        #endregion Constructors

        #region Methods

        public static PluginConfigurationViewModel[] ToViewModel(this PluginConfiguration[] pluginConfigurations)
        {
            return _mapper.Map<PluginConfiguration[], PluginConfigurationViewModel[]>(pluginConfigurations);
        }

        public static PluginConfigurationViewModel ToViewModel(this PluginConfiguration pluginConfiguration)
        {
            return _mapper.Map<PluginConfiguration, PluginConfigurationViewModel>(pluginConfiguration);
        }

        #endregion Methods
    }

    public class PluginConfigurationViewModel : PluginConfiguration
    {
    }
}