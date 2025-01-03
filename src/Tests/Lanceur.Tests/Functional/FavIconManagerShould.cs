using FluentAssertions;
using Lanceur.Core.Services;
using Lanceur.Infra.Managers;
using Lanceur.SharedKernel.Mixins;
using Lanceur.SharedKernel.Web;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Lanceur.Tests.Functional;

public class FavIconManagerShould
{
    [Theory, InlineData("http://www.google.com", "http://www.google.com"), InlineData("http://www.google.com:4001", "http://www.google.com:4001"), InlineData("http://www.google.com:80", "http://www.google.com:80"), InlineData("https://www.google.com", "https://www.google.com"), InlineData("https://www.google.com:4001", "https://www.google.com:4001"), InlineData("https://www.google.com:80", "https://www.google.com:80"), InlineData("https://www.google.com/some/index.html", "https://www.google.com"), InlineData("https://www.google.com:4001/some/index.html", "https://www.google.com:4001"), InlineData("https://www.google.com:80/some/index.html", "https://www.google.com:80")]
    public async Task RetrieveExpectedUrl(string url, string asExpected)
    {
        // ARRANGE
        var searchService = Substitute.For<IPackagedAppSearchService>();
        var favIconDownloader = Substitute.For<IFavIconDownloader>();
        var logger = Substitute.For<ILoggerFactory>();
        var repository = Path.GetTempPath();

        // ACT
        var manager = new FavIconService(searchService, favIconDownloader, logger, repository);
        await manager.RetrieveFaviconAsync(url);

        // ASSERT
        await favIconDownloader.Received()
                               .CheckExistsAsync(new(asExpected));
    }

    [Theory, InlineData("http://www.google.com", "http://www.google.com/favicon.ico"), InlineData("http://www.google.com:4001", "http://www.google.com:4001/favicon.ico"), InlineData("http://www.google.com:80", "http://www.google.com:80/favicon.ico"), InlineData("https://www.google.com", "https://www.google.com/favicon.ico"), InlineData("https://www.google.com:4001", "https://www.google.com:4001/favicon.ico"), InlineData("https://www.google.com:80", "https://www.google.com:80/favicon.ico")]
    public void CreateFaviconUri(string url, string expected)
    {
        var thisUri = new Uri(expected);
        new Uri(url).GetFavicons()
                    .Should()
                    .Contain(thisUri);
    }
}