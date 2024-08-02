using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Splat;

namespace Lanceur.Infra.Services;

public abstract class SearchServiceCache
{
    #region Fields

    private readonly IStoreLoader _storeLoader;
    private IEnumerable<ISearchService> _stores;

    #endregion Fields

    #region Constructors

    internal SearchServiceCache(IStoreLoader storeLoader) => _storeLoader = storeLoader ?? Locator.Current.GetService<IStoreLoader>();

    #endregion Constructors

    #region Properties

    public IEnumerable<ISearchService> Stores => _stores ??= _storeLoader.Load();

    #endregion Properties
}