using Shouldly;
using Lanceur.SharedKernel.Utils;
using Xunit;

namespace Lanceur.Tests.SharedKernel;

public class CurrentVersionShould
{
    #region Methods

    public static IEnumerable<object[]> FeedCreateCurrentVersionFromFullVersion()
    {
        yield return [
            "3.4.0-alpha-issue.878.1+4.Branch.issue-878.Sha.584abe02d74b0cea5a50e15c83c0b46ef30f39ad.584abe02d74b0cea5a50e15c83c0b46ef30f39ad", 
            "3.4.0", 
            "alpha-issue.878.1", 
            "4.Branch.issue-878.Sha.584abe02d74b0cea5a50e15c83c0b46ef30f39ad.584abe02d74b0cea5a50e15c83c0b46ef30f39ad"
        ];
        yield return [
            "3.4.0+4.Branch.issue-878.Sha.584abe02d74b0cea5a50e15c83c0b46ef30f39ad.584abe02d74b0cea5a50e15c83c0b46ef30f39ad", 
            "3.4.0", 
            "", 
            "4.Branch.issue-878.Sha.584abe02d74b0cea5a50e15c83c0b46ef30f39ad.584abe02d74b0cea5a50e15c83c0b46ef30f39ad"
        ];
        yield return [
            "3.4.0", 
            "3.4.0", 
            "", 
            ""
        ];
        yield return [
            "", 
            "0.0.0", 
            "", 
            ""
        ];
    } 
    [Theory]
    [MemberData(nameof(FeedCreateCurrentVersionFromFullVersion))]
    public void CreateCurrentVersionFromFullVersion(string fullVersion, string version, string suffix, string commit)
    {
        var v = CurrentVersion.FromFullVersion(fullVersion);

        Assert.Multiple(
            () => v.Commit.ShouldBe(commit),
            () => v.Version.ShouldBe(new(version)),
            () => v.Suffix.ShouldBe(suffix)
        );
    }

    #endregion
}