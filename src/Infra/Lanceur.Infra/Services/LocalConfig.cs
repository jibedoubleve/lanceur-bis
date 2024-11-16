using Lanceur.Core.Models.Settings;
using Lanceur.Infra.Constants;

namespace Lanceur.Infra.Services;

public class LocalConfig : ILocalConfig
{
    #region Properties

    public string DbPath { get; set; } = Paths.DefaultDb;

    #endregion Properties
}