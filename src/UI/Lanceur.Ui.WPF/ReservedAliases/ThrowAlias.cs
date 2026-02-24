#if DEBUG
using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;

namespace Lanceur.Ui.WPF.ReservedKeywords;

/// <summary>
///     This keyword is only visible in debug mode. The sole prurpose of this keyword is to throw an exception
///     for debugging purpose
/// </summary>
[ReservedAlias("throw")]
[Description("Throws an unexpected exception")]
public class ThrowAlias : SelfExecutableQueryResult
{
    #region Constructors

    public ThrowAlias(IServiceProvider serviceProvider) { }

    #endregion

    #region Properties

    public override string Icon => "Bug24";

    #endregion

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null) => throw new NotImplementedException("Some unexpected exception");

    #endregion
}
#endif