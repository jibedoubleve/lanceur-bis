namespace Lanceur.Core.Models;

public record SteamGame(int Id, string Name)
{
    #region Fields

    public const string UriTemplate = "steam://run/{0}";

    #endregion

    #region Methods

    public string ToSteamUrl() => string.Format(UriTemplate, Id);

    #endregion
}