using Lanceur.Core.Configuration.Sections.Infrastructure;

namespace Lanceur.Tests.Tools.Stubs;

public class TestSettingsWithTwiceSameSection
{
    #region Properties

    public DatabaseSection Database { get; } = new();
    public DatabaseSection Database2 { get; } = new();

    #endregion
}