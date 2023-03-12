using Lanceur.Core;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Utils;
using Lanceur.SharedKernel;
using Lanceur.SharedKernel.Mixins;
using System;
using System.Diagnostics;

namespace Lanceur.Infra.Managers
{
    public class ExecutionManager : IExecutionManager
    {
        #region Fields

        private readonly IDataService _dataService;
        private readonly IAppLogger _log;
        private readonly IWildcardManager _wildcardManager;

        #endregion Fields

        #region Constructors

        public ExecutionManager(IAppLoggerFactory logFactory, IWildcardManager wildcardManager, IDataService dataService)
        {
            _log = logFactory.GetLogger<ExecutionManager>();
            _wildcardManager = wildcardManager;
            _dataService = dataService;
        }

        #endregion Constructors

        #region Methods

        private async Task<IEnumerable<QueryResult>> ExecuteAliasAsync(AliasQueryResult query)
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
                    return ExecuteUwp(query);
                }
                else
                {
                    _log.Trace("Executing process...");
                    return ExecuteProcess(query);
                }
            }
            catch (Exception ex) { throw new ApplicationException($"Cannot execute alias '{(query?.Name ?? "NULL")}'", ex); }
        }

        private IEnumerable<QueryResult> ExecuteProcess(AliasQueryResult query)
        {
            _log.Debug($"Executing '{query.FileName}' with args '{query?.Query?.Parameters ?? string.Empty}'");
            var psi = new ProcessStartInfo
            {
                FileName = _wildcardManager.Replace(query.FileName, query.Query.Parameters),
                Verb = "open",
                Arguments = _wildcardManager.ReplaceOrReplacementOnNull(query.Arguments, query.Query.Parameters),
                UseShellExecute = true,
                WorkingDirectory = query.WorkingDirectory,
                WindowStyle = query.StartMode.AsWindowsStyle(),
            };

            if (query.IsPrivilegeOverriden || query.RunAs == Constants.RunAs.Admin)
            {
                psi.Verb = "runas";
                _log.Info($"Runs '{query.FileName}' as ADMIN");
            }

            using var _ = Process.Start(psi);
            return QueryResult.NoResult;
        }

        private IEnumerable<QueryResult> ExecuteUwp(AliasQueryResult query)
        {
            // https://stackoverflow.com/questions/42521332/launching-a-windows-10-store-app-from-c-sharp-executable
            var file = query.FileName.Replace("package:", @"shell:AppsFolder\");
            var psi = new ProcessStartInfo()
            {
                FileName = file,
            };
            if (query.IsPrivilegeOverriden)
            {
                //https://stackoverflow.com/a/23199505/389529
                psi.UseShellExecute = true;
                psi.Verb = "runas";
                _log.Info($"Runs '{query.FileName}' as ADMIN");
            }

            _log.Debug($"Executing packaged application'{file}'");
            Process.Start(psi);

            return QueryResult.NoResult;
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
            else if (request.QueryResult is IExecutable exec)
            {
                if (request.QueryResult is IExecutableWithPrivilege exp)
                {
                    exp.IsPrivilegeOverriden = request.ExecuteWithPrivilege;
                }

                var result = (request.QueryResult is AliasQueryResult alias)
                    ? await ExecuteAliasAsync(alias)
                    : await exec.ExecuteAsync();

                return ExecutionResponse.FromResults(result);
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