using Lanceur.Core.Models;

namespace Lanceur.Ui.Core.Messages;

public record AddAliasMessage
{
    #region Constructors

    public AddAliasMessage(Cmdline? cmdline) => Cmdline = cmdline;

    #endregion

    #region Properties

    public Cmdline? Cmdline { get; }

    #endregion
}