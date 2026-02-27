using Lanceur.Core.Models;

namespace Lanceur.Infra.Win32.Thumbnails;

public interface IThumbnailStrategy
{
    #region Methods

    Task UpdateThumbnailAsync(AliasQueryResult alias);

    #endregion
}