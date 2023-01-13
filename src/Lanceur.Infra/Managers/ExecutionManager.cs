using Lanceur.Core;
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
        private readonly IDataService _dataService;

        #endregion Fields

        #region Constructors

        public ExecutionManager(ILogService log, IWildcardManager wildcardManager, IDataService dataService)
        {
            _log = log;
            _wildcardManager = wildcardManager;
            _dataService = dataService;
        }

        #endregion Constructors

        #region Methods

        private async Task ExecuteAliasAsync(AliasQueryResult query)
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
                else
                {
                    _log.Trace("Executing process...");
                    ExecuteProcess(query);
                }
            }
            catch (Exception ex) { throw new ApplicationException($"Cannot execute alias '{(query?.Name ?? "NULL")}'", ex); }
        }

        private void ExecuteProcess(AliasQueryResult query)
        {
            _log.Debug($"Executing '{query.FileName}' with args '{query?.Query?.Parameters ?? string.Empty}'");
            var psi = new ProcessStartInfo
            {
                FileName = _wildcardManager.Replace(query.FileName, query.Query.Parameters),
                Verb = "open",
                Arguments = _wildcardManager.HandleArgument(query.Arguments, query.Query.Parameters),
                UseShellExecute = true,
                WorkingDirectory = query.WorkingDirectory,
                WindowStyle = query.StartMode.AsWindowsStyle(),
            };

            if (query.IsPrivilegeOverriden || query.RunAs == Constants.RunAs.Admin)
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
            if (query.IsPrivilegeOverriden)
            {
                psi.Verb = "runas";
                _log.Info($"Runs '{query.FileName}' as ADMIN");
            }

            _log.Debug($"Executing packaged application'{file}'");
            Process.Start(psi);
        }

        public async Task<ExecutionResponse> ExecuteAsync(ExecutionRequest request)
        {
            _dataService.SetUsage(request.QueryResult);

            if (request.QueryResult is null)
            {
                return new ExecutionResponse
                {
                    Results = DisplayQueryResult.SingleFromResult($"This alias does not exist"),
                    HasResult = true,
                };
            }
            else if (request.QueryResult is AliasQueryResult alias)
            {
                await ExecuteAliasAsync(alias);
                return ExecutionResponse.NoResult;
            }
            else if (request.QueryResult is IExecutable exec)
            {
                if (request.QueryResult is IExecutableWithPrivilege exp)
                {
                    exp.IsPrivilegeOverriden = request.ExecuteWithPrivilege;
                }
                var results = await exec.ExecuteAsync(request.Cmdline);
                return new ExecutionResponse
                {
                    Results = results,
                    HasResult = results.Any()
                };
            }
            else
            {
                _log.Info($"Alias '{request.QueryResult.Name}', is not executable. Add as a query");
                return ExecutionResponse.EmptyResult;
            }
        }

        #endregion Methods
    }
}