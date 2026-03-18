using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Configuration.Sections.Application;

namespace Lanceur.Ui.WPF.Services;

public sealed class MainViewSections
{
    #region Fields

    private readonly IWriteableSection<WindowSection> _windowSection;

    #endregion

    #region Constructors

    public MainViewSections(
        ISection<ResourceMonitorSection> resourceMonitorSection,
        IWriteableSection<WindowSection> windowSection,
        ISection<SearchBoxSection> searchBoxSection)
    {
        _windowSection = windowSection;
        SearchBox = searchBoxSection.Value;
        ResourceMonitor = resourceMonitorSection.Value;
    }

    #endregion

    #region Properties

    public ResourceMonitorSection ResourceMonitor { get; }
    public SearchBoxSection SearchBox { get; }
    public WindowSection Window => _windowSection.Value;

    #endregion

    #region Methods

    public void SaveWindowSection() => _windowSection.Save();

    #endregion
}