using Lanceur.Core.Managers;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Services;
using Lanceur.Tests.Logging;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit.Abstractions;

namespace Lanceur.Tests.Utils.Builders;

public class SearchServiceBuilder
{
    #region Fields

    private readonly IMacroManager _macroManager = Substitute.For<IMacroManager>();
    private readonly IThumbnailManager _thumbnailManager = Substitute.For<IThumbnailManager>();
    private ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
    private IStoreLoader _storeLoader = Substitute.For<IStoreLoader>();

    #endregion Fields

    #region Methods

    public IAsyncSearchService Build()
    {
        return new SearchService(
            _storeLoader,
            _macroManager,
            _thumbnailManager,
            _loggerFactory);
    }

    public SearchServiceBuilder With(IStoreLoader storeLoader)
    {
        _storeLoader = storeLoader;
        return this;
    }

    public SearchServiceBuilder With(ITestOutputHelper output)
    {
        _loggerFactory = new MicrosoftLoggingLoggerFactory(output);
        return this;
    }

    #endregion Methods
}