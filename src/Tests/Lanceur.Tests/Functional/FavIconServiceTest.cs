using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Services;
using Lanceur.SharedKernel.Extensions;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Functional;

public class FavIconServiceTest
{
    #region Fields

    private readonly ITestOutputHelper _output;

    #endregion

    #region Constructors

    public FavIconServiceTest(ITestOutputHelper output) => _output = output;

    #endregion

    #region Methods

    [Theory]
    [InlineData("http://www.google.com", "http://www.google.com/favicon.ico")]
    [InlineData("http://www.google.com:4001", "http://www.google.com:4001/favicon.ico")]
    [InlineData("http://www.google.com:80", "http://www.google.com:80/favicon.ico")]
    [InlineData("https://www.google.com", "https://www.google.com/favicon.ico")]
    [InlineData("https://www.google.com:4001", "https://www.google.com:4001/favicon.ico")]
    [InlineData("https://www.google.com:80", "https://www.google.com:80/favicon.ico")]
    public void When_fetching_favicon_Then_expected_favicons_returned(string url, string expected)
    {
        var thisUri = new Uri(expected);
        new Uri(url).GetFavicons()
                    .ShouldContain(thisUri);
    }

    [Theory]
    [InlineData("http://www.google.com", "http://www.google.com")]
    [InlineData("http://www.google.com:4001", "http://www.google.com:4001")]
    [InlineData("http://www.google.com:80", "http://www.google.com:80")]
    [InlineData("https://www.google.com", "https://www.google.com")]
    [InlineData("https://www.google.com:4001", "https://www.google.com:4001")]
    [InlineData("https://www.google.com:80", "https://www.google.com:80")]
    [InlineData("https://www.google.com/some/index.html", "https://www.google.com")]
    [InlineData("https://www.google.com:4001/some/index.html", "https://www.google.com:4001")]
    [InlineData("https://www.google.com:80/some/index.html", "https://www.google.com:80")]
    public async Task When_retrieving_origin_from_url_Then_expected_value_returned(string url, string asExpected)
    {
        // ARRANGE
        var favIconDownloader = Substitute.For<IFavIconDownloader>();


        // ACT
        var manager = new FavIconService(
            favIconDownloader
        );
        var alias = new AliasQueryResult { FileName = url };

        await manager.UpdateFaviconAsync(
            alias,
            _ => Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())
        );

        // ASSERT
        await favIconDownloader.Received()
                               .RetrieveAndSaveFavicon(new(asExpected), Arg.Any<string>());
    }

    #endregion
}