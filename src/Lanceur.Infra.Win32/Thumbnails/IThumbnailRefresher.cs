using Lanceur.Core.Decorators;
using Lanceur.Core.Models;

namespace Lanceur.Infra.Win32.Thumbnails;

public interface IThumbnailRefresher
{
    #region Methods

    void RefreshCurrentThumbnail(EntityDecorator<QueryResult> query);

    #endregion Methods
}