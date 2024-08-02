using Lanceur.Core;
using Lanceur.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace Lanceur.ReservedKeywords;

[ReservedAlias("version"), Description("Indicates the version of the application")]
public class VersionAlias : SelfExecutableQueryResult
{
    #region Properties

    public override string Icon => "InformationOutline";

    #endregion Properties

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
    {
        var asm = Assembly.GetExecutingAssembly();
        var semver = FileVersionInfo.GetVersionInfo(asm.Location).ProductVersion;
        var semverSplit = semver?.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
        semver = semverSplit?.Length > 0 ? semverSplit[0] : semver;

        var nl = Environment.NewLine;
        var msg = $"Lanceur {semver}{nl}{nl}Written by Jean-Baptiste Wautier";
        MessageBox.Show(msg, "Lanceur", MessageBoxButton.OK, MessageBoxImage.Information);

        return NoResultAsync;
    }

    #endregion Methods
}