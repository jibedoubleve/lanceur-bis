using System.Diagnostics;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Infra.Mappers;

public static class ProcessStartInfoMapperExtensions
{
    #region Methods

    public static ProcessStartInfo ToProcessStartInfo(this ProcessContext context) => new()
    {
        FileName = context.FileName,
        Arguments = context.Arguments,
        WorkingDirectory = context.WorkingDirectory,
        UseShellExecute = context.UseShellExecute,
        Verb = context.Verb,
        WindowStyle = context.WindowStyle.AsWindowsStyle()
    };

    #endregion
}