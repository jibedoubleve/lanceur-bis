using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Services;
using Lanceur.SharedKernel.Extensions;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Functional;

public sealed class FavIconServiceTest
{
    #region Fields

    private readonly ITestOutputHelper _output;

    #endregion

    #region Constructors

    public FavIconServiceTest(ITestOutputHelper output) => _output = output;

    #endregion

    #region Methods

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
        var source = new CancellationTokenSource();

        // ACT
        var manager = new FavIconService(
            favIconDownloader
        );
        var alias = new AliasQueryResult { FileName = url };

        await manager.UpdateFaviconAsync(
            alias,
            _ => Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()),
            source.Token
        );

        // ASSERT
        await favIconDownloader.Received()
                               .RetrieveAndSaveFavicon(new Uri(asExpected), Arg.Any<string>(), source.Token);
    }

    #endregion
}