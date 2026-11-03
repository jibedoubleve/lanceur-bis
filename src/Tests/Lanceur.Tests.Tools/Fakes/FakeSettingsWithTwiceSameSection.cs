using Lanceur.Core.Configuration.Sections.Infrastructure;

namespace Lanceur.Tests.Tools.Fakes;

public sealed class FakeSettingsWithTwiceSameSection
{
    #region Properties

    public DatabaseSection Database { get; } = new();
    public DatabaseSection Database2 { get; } = new();

    #endregion
}