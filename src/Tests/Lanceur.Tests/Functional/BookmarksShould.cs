using System.Web.Bookmarks;
using System.Web.Bookmarks.Repositories;
using System.Web.Bookmarks.RepositoryConfiiguration;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using ILoggerFactory = Castle.Core.Logging.ILoggerFactory;

namespace Lanceur.Tests.Functional;

public class BookmarksShould
{
    #region Fields

    private readonly ITestOutputHelper _outputHelper;

    #endregion

    #region Constructors

    public BookmarksShould(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    #endregion

    #region Methods

    [Fact]
    public void NotThrowWhenGeckoConfigurationIsWrong()
    {
        // ARRANGE
        var serviceProvider = new ServiceCollection().AddLogging(builder => builder.AddXUnit(_outputHelper))
                                                     .AddTransient<IMemoryCache, MemoryCache>()
                                                     .AddSingleton<IGeckoBrowserConfiguration>(new DummyGeckoConfiguration("", "", ""))
                                                     .AddTransient<IBookmarkRepository, GeckoBrowserBookmarks>()
                                                     .BuildServiceProvider();
        var repository = serviceProvider.GetService<IBookmarkRepository>();

        // ACT & ASSERT
        repository.GetBookmarks().Should().BeEmpty();
    }
    
    [Fact]
    public void NotThrowWhenChromeConfigurationIsWrong()
    {
        // ARRANGE
        var serviceProvider = new ServiceCollection().AddLogging(builder => builder.AddXUnit(_outputHelper))
                                                     .AddTransient<IMemoryCache, MemoryCache>()
                                                     .AddSingleton<IChromiumBrowserConfiguration>(new DummyChromiumConfiguration())
                                                     .AddTransient<IBookmarkRepository, ChromiumBrowserBookmarks>()
                                                     .BuildServiceProvider();
        var repository = serviceProvider.GetService<IBookmarkRepository>();

        // ACT & ASSERT
        repository.GetBookmarks().Should().BeEmpty();
    }

    #endregion
}