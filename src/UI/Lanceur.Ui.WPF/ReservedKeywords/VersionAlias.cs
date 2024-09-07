﻿using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using Lanceur.Core;
using Lanceur.Core.Models;

namespace Lanceur.Ui.WPF.ReservedKeywords;

[ReservedAlias("version"), Description("Indicates the version of the application")]
public class VersionAlias : SelfExecutableQueryResult
{
    #region Constructors

    public VersionAlias(IServiceProvider serviceProvider) { }

    #endregion

    #region Properties

    public override string Icon => "InformationOutline";

    #endregion

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        var asm = Assembly.GetExecutingAssembly();
        var semver = FileVersionInfo.GetVersionInfo(asm.Location).ProductVersion;
        var semverSplit = semver?.Split(["+"], StringSplitOptions.RemoveEmptyEntries);
        semver = semverSplit?.Length > 0 ? semverSplit[0] : semver;

        var nl = Environment.NewLine;
        var msg = $"Lanceur {semver}{nl}{nl}Written by Jean-Baptiste Wautier";
        MessageBox.Show(msg, "Lanceur", MessageBoxButton.OK, MessageBoxImage.Information);

        return NoResultAsync;
    }

    #endregion
}