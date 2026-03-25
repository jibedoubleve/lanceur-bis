using System.Diagnostics;

namespace Lanceur.Core.Configuration.Sections.Application;

[DebuggerDisplay("Top: {Top} - Left: {Left}")]
public sealed class PositionSection
{
    #region Properties

    public double Left { get; set; }
    public double Top { get; set; }

    #endregion
}