using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Utils;
using Lanceur.SharedKernel;
using Lanceur.SharedKernel.Mixins;
using System.Diagnostics;

namespace Lanceur.Infra.Managers
{
    public class ExecutionManager : IExecutionManager
    {
        #region Fields

        private readonly ILogService _log;
        private readonly IWildcardManager _wildcardManager;

        #endregion Fields

        #region Constructors

        public ExecutionManager(ILogService log, IWildcardManager wildcardManager)
        {
            _log = log;
            _wildcardManager = wildcardManager;
        }

        #endregion Constructors

        #region Methods

        private void ExecuteProcess(AliasQueryResult query)
        {
            _log.Debug($"Executing '{query.FileName}' with args '{query.Arguments}'");
            var psi = new ProcessStartInfo
            {
                FileName = _wildcardManager.Replace(query.FileName, query.Query.Parameters),
                Verb = "open",
                Arguments = _wildcardManager.HandleArgument(query.Arguments, query.Query.Parameters),
                UseShellExecute = true,
                WorkingDirectory = query.WorkingDirectory,
                WindowStyle = query.StartMode.AsWindowsStyle(),
            };

            if (query.IsPrivileged || query.RunAs == Constants.RunAs.Admin)
            {
                psi.Verb = "runas";
                _log.Info($"Runs '{query.FileName}' as ADMIN");
            }

            using var ps = new Process { StartInfo = psi };
            ps.Start();
        }

        private void ExecuteUwp(AliasQueryResult query)
        {
            // https://stackoverflow.com/questions/42521332/launching-a-windows-10-store-app-from-c-sharp-executable
            var file = query.FileName.Replace("package:", @"shell:AppsFolder\");
            var psi = new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments = file,
            };
            if (query.IsPrivileged)
            {
                psi.Verb = "runas";
                _log.Info($"Runs '{query.FileName}' as ADMIN");
            }

            _log.Debug($"Executing packaged application'{file}'");
            Process.Start(psi);
        }

        public async Task ExecuteAsync(AliasQueryResult query)
        {
            try
            {
                if (query.Delay > 0)
                {
                    _log.Trace($"Delay of {query.Delay} second(s) before executing the alias");
                    await Task.Delay(query.Delay * 1000);
                }
                if (query.IsUwp())
                {
                    _log.Trace("Executing UWP application...");
                    ExecuteUwp(query);
                }
                //if (query.IsMacro())
                //{
                //    _log.Trace("Executing a Macro...");
                //    var cmdline = new Cmdline(query.GetMacro(), query.Arguments);
                //    await query.ExecuteAsync(cmdline);
                //}
                else
                {
                    _log.Trace("Executing process...");
                    ExecuteProcess(query);
                }
            }
            catch (Exception ex) { throw new ApplicationException($"Cannot execute alias '{(query?.Name ?? "NULL")}'", ex); }
        }

        #endregion Methods
    }
}