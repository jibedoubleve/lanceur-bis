using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

public interface IApplicationService
{
    #region Methods

    Coordinate GetCenterCoordinate();
    void Shutdown();

    #endregion
}