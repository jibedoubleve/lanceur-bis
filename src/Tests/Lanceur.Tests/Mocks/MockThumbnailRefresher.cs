using Lanceur.Core.Decorators;
using Lanceur.Core.Models;
using Lanceur.Infra.Win32.Thumbnails;

namespace Lanceur.Tests.Mocks;

public class MockThumbnailRefresher : IThumbnailRefresher
{
    #region Methods

    public void RefreshCurrentThumbnail(EntityDecorator<QueryResult> query)
    {
        query.Entity.Thumbnail = Guid.NewGuid().ToString();
        query.Soil();
    }

    #endregion Methods
}