using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.SharedKernel.Mixins;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Lanceur.ReservedKeywords
{
    [ReservedAlias(">"), Description("execute the parameters in PowerShell")]
    internal class ShellCommandAlias : SelfExecutableQueryResult
    {
        #region Properties

        public override string Icon => "ConsoleLine";

        #endregion Properties

        #region Methods

        public override async Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
        {
            if (cmdline is null) return NoResult;
            
            var psi = new ProcessStartInfo
            {
                FileName               = "Powershell.exe",
                Arguments              = $"-noprofile {cmdline.Parameters}",
                CreateNoWindow         = true,
                UseShellExecute        = false,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
            };

            using var process      = Process.Start(psi);
            using var outputStream = process!.StandardOutput;
            using var errorStream  = process!.StandardError;
            
            var output = await outputStream.ReadToEndAsync();
            var error = await errorStream.ReadToEndAsync();

            var resultOutput = error.IsNullOrWhiteSpace()
                ? output
                : error;
            return new List<DisplayQueryResult>
            {
                new(
                    null,
                    description: resultOutput
                )
            };
        }

        #endregion Methods
    }
}