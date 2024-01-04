using Lanceur.Core.Decorators;
using Lanceur.Core.Models;
using Lanceur.Infra.Win32.Thumbnails;

namespace Lanceur.Tests.Mocks;

public class MockThumbnailRefresher : IThumbnailRefresher
{
    #region Methods

    public Task RefreshCurrentThumbnailAsync(EntityDecorator<QueryResult> query)
    {
        query.Entity.Thumbnail = Guid.NewGuid().ToString();
        query.Soil();
        return Task.CompletedTask;
    }

    #endregion Methods
}