using AutoMapper.Configuration.Annotations;
using FluentAssertions;
using Lanceur.Utils.PackagedApps;
using Xunit;

namespace Lanceur.Tests.Functional
{
    public class PackagedAppManagerShould
    {
        #region Methods

        [Theory(Skip = "This test depends on the state of the machine where the tests run")]
        [InlineData(@"C:\Program Files\WindowsApps\Microsoft.Todos_2.85.53361.0_x64__8wekyb3d8bbwe\Todo.exe", "Microsoft.Todos_8wekyb3d8bbwe!App")]
        [InlineData(@"C:\ProgramData\chocolatey\bin\ZoomIt64a.exe", "")]
        public async Task ReturnAppUniqueId(string path, string expected)
        {
            var mgr = new PackagedAppManager();

            (await mgr.GetPackageUniqueIdAsync(path))
               .Should().Be(expected);
        }

        [Theory(Skip = "This test depends on the state of the machine where the tests run")]
        [InlineData(@"C:\Program Files\WindowsApps\Microsoft.Todos_2.85.53361.0_x64__8wekyb3d8bbwe\Todo.exe", true)]
        [InlineData(@"C:\ProgramData\chocolatey\bin\ZoomIt64a.exe", false)]
        public async Task CheckIsPackage(string path, bool expected)
        {
            var mgr = new PackagedAppManager();

            (await mgr.IsPackageAsync(path))
               .Should().Be(expected);
        }

        #endregion Methods
    }
}