using Lanceur.Core.Services;
using Lanceur.Core.Stores;

namespace Lanceur.Infra.Services;

public abstract class SearchServiceCache
{
    #region Fields

    private readonly IStoreLoader _storeLoader;
    private IEnumerable<IStoreService> _stores;

    #endregion Fields

    #region Constructors

    internal SearchServiceCache(IStoreLoader storeLoader) => _storeLoader = storeLoader;

    #endregion Constructors

    #region Properties

    public IEnumerable<IStoreService> Stores => _stores ??= _storeLoader.Load();

    #endregion Properties
}