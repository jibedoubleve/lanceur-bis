using Lanceur.Core;
using Lanceur.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace Lanceur.ReservedKeywords
{
    [ReservedAlias("version"), Description("Indicates the version of the application")]
    public class VersionAlias : SelfExecutableQueryResult
    {
        #region Methods

        public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
        {
            var asm = Assembly.GetExecutingAssembly();
            var version = asm.GetName().Version.ToString();
            var fileVersion = FileVersionInfo.GetVersionInfo(asm.Location).FileVersion.ToString();

            var semver = FileVersionInfo.GetVersionInfo(asm.Location).ProductVersion.ToString();
            var semverSplit = semver.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
            semver = semverSplit.Length > 0 ? semverSplit[0] : semver;

            var nl = Environment.NewLine;
            var msg = $"Version: {version}{nl}File Version: {fileVersion}{nl}SemVer: {semver}{nl}Author: JB Wautier";
            MessageBox.Show(msg, "Lanceur", MessageBoxButton.OK, MessageBoxImage.Information);

            return NoResultAsync;
        }

        #endregion Methods
    }
}