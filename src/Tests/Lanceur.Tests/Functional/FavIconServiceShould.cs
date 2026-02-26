using Shouldly;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Services;
using Lanceur.Infra.Win32.Helpers;
using Lanceur.SharedKernel.Extensions;
using NSubstitute;
using Xunit;

namespace Lanceur.Tests.Functional;

public class FavIconServiceShould
{
    #region Methods

    [Theory]
    [InlineData("http://www.google.com", "http://www.google.com/favicon.ico")]
    [InlineData("http://www.google.com:4001", "http://www.google.com:4001/favicon.ico")]
    [InlineData("http://www.google.com:80", "http://www.google.com:80/favicon.ico")]
    [InlineData("https://www.google.com", "https://www.google.com/favicon.ico")]
    [InlineData("https://www.google.com:4001", "https://www.google.com:4001/favicon.ico")]
    [InlineData("https://www.google.com:80", "https://www.google.com:80/favicon.ico")]
    public void CreateFaviconUri(string url, string expected)
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
    public async Task RetrieveExpectedUrl(string url, string asExpected)
    {
        // ARRANGE
        var favIconDownloader = Substitute.For<IFavIconDownloader>();
        var repository = Path.GetTempPath();
        

        // ACT
        var manager = new FavIconService(
            favIconDownloader,
            repository
        );
        var alias = new AliasQueryResult { FileName = url };
        await manager.UpdateFaviconAsync(alias);

        // ASSERT
        await favIconDownloader.Received()
                               .RetrieveAndSaveFavicon(new(asExpected), Arg.Any<string>());
    }

    #endregion
}