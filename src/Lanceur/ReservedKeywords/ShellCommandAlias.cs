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
    internal class ShellCommandAlias : ExecutableQueryResult
    {
        #region Methods

        public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "Powershell.exe",
                Arguments = cmdline.Parameters,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            using var process = Process.Start(psi);
            using var outputStream = process.StandardOutput;
            using var errorStream = process.StandardError;
            var output = outputStream.ReadToEnd();
            var error = errorStream.ReadToEnd();

            var resultOutput = error.IsNullOrWhiteSpace()
                ? output
                : error;
            var r = new List<DisplayQueryResult> {
                new DisplayQueryResult(
                    name: null,
                    description: resultOutput
                )
            };
            return Task.FromResult<IEnumerable<QueryResult>>(r);
        }

        #endregion Methods
    }
}