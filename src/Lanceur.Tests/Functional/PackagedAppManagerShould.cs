using FluentAssertions;
using Lanceur.Utils.PackagedApps;
using Xunit;

namespace Lanceur.Tests.Functional
{
    public class PackagedAppManagerShould
    {
        #region Methods

        [Theory]
        [InlineData(@"C:\Program Files\WindowsApps\Microsoft.Todos_2.73.51701.0_x64__8wekyb3d8bbwe\Todo.exe", "Microsoft.Todos_8wekyb3d8bbwe!App")]
        [InlineData(@"C:\ProgramData\chocolatey\bin\ZoomIt64a.exe", "")]
        public void ReturnAppUniqueId(string path, string expected)
        {
            var mgr = new PackagedAppManager();

            mgr.GetPackageUniqueIdAsync(path)
               .Should().Be(expected);
        }
        [Theory]
        [InlineData(@"C:\Program Files\WindowsApps\Microsoft.Todos_2.73.51701.0_x64__8wekyb3d8bbwe\Todo.exe", true)]
        [InlineData(@"C:\ProgramData\chocolatey\bin\ZoomIt64a.exe", false)]
        public void CheckIsPackage(string path, bool expected)
        {
            var mgr = new PackagedAppManager();

            mgr.IsPackageAsync(path)
               .Should().Be(expected);
        }

        #endregion Methods
    }
}