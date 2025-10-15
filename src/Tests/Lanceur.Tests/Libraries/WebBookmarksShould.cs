using System.Web.Bookmarks;
using System.Web.Bookmarks.Repositories;
using System.Web.Bookmarks.RepositoryConfiguration;
using Shouldly;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Lanceur.Tests.Repositories;

public class WebBookmarksShould
{
    #region Fields

    private readonly ITestOutputHelper _outputHelper;

    #endregion

    #region Constructors

    public WebBookmarksShould(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    #endregion

    #region Methods

    [Fact]
    public void NotThrowWhenGeckoConfigurationIsWrong()
    {
        // ARRANGE
        var serviceProvider = new ServiceCollection().AddLogging(builder => builder.AddXUnit(_outputHelper))
                                                     .AddSingleton<IMemoryCache, MemoryCache>()
                                                     .AddSingleton<IGeckoBrowserConfiguration>(new DummyGeckoConfiguration("", "", ""))
                                                     .AddTransient<IBookmarkRepository, GeckoBrowserBookmarks>()
                                                     .BuildServiceProvider();
        var repository = serviceProvider.GetService<IBookmarkRepository>();

        // ACT & ASSERT
        repository.GetBookmarks().ShouldBeEmpty();
    }
    
    [Fact]
    public void NotThrowWhenChromeConfigurationIsWrong()
    {
        // ARRANGE
        var serviceProvider = new ServiceCollection().AddLogging(builder => builder.AddXUnit(_outputHelper))
                                                     .AddSingleton<IMemoryCache, MemoryCache>()
                                                     .AddSingleton<IBlinkBrowserConfiguration>(new DummyBlinkConfiguration())
                                                     .AddTransient<IBookmarkRepository, BlinkBrowserBookmarks>()
                                                     .BuildServiceProvider();
        var repository = serviceProvider.GetService<IBookmarkRepository>();

        // ACT & ASSERT
        repository.GetBookmarks().ShouldBeEmpty();
    }

    #endregion
}