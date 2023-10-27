using Lanceur.Core;
using Lanceur.Core.LuaScripting;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Requests;
using Lanceur.Core.Services;
using Lanceur.Core.Utils;
using Lanceur.Infra.LuaScripting;
using Lanceur.SharedKernel;
using Lanceur.SharedKernel.Mixins;
using System.Diagnostics;

namespace Lanceur.Infra.Managers
{
    public class ExecutionManager : IExecutionManager
    {
        #region Fields

        private readonly ICmdlineManager _cmdlineManager;
        private readonly IDbRepository _dataService;
        private readonly IAppLogger _log;
        private readonly IWildcardManager _wildcardManager;

        #endregion Fields

        #region Constructors

        public ExecutionManager(
            IAppLoggerFactory logFactory,
            IWildcardManager wildcardManager,
            IDbRepository dataService,
            ICmdlineManager cmdlineManager
        )
        {
            _log = logFactory.GetLogger<ExecutionManager>();
            _wildcardManager = wildcardManager;
            _dataService = dataService;
            _cmdlineManager = cmdlineManager;
        }

        #endregion Constructors

        #region Methods

        private static void ExecuteLuaScript(ref AliasQueryResult query)
        {
            var result = LuaManager.ExecuteScript(new()
            {
                Code = query.LuaScript,
                Context = new()
                {
                    FileName = query.FileName,
                    Parameters = query.Parameters
                }

            });
            if(result?.Context?.FileName is not null) query.FileName = result.Context.FileName;
            if(result?.Context?.Parameters is not null) query.Parameters = result.Context.Parameters;
        }

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
            catch (Exception ex) { throw new ApplicationException($"Cannot execute alias '{(query?.Name ?? "NULL")}: {ex.Message}'", ex); }
        }

        private IEnumerable<QueryResult> ExecuteProcess(AliasQueryResult query)
        {
            if (query is null) return QueryResult.NoResult;

            query.Parameters = _wildcardManager.ReplaceOrReplacementOnNull(query.Parameters, query.Query.Parameters);
            ExecuteLuaScript(ref query);

            _log.Debug($"Executing '{query.FileName}' with args '{query.Query.Parameters}'");
            var psi = new ProcessStartInfo
            {
                FileName = _wildcardManager.Replace(query.FileName, query.Query.Parameters),
                Verb = "open",
                Arguments = query.Parameters,
                UseShellExecute = true,
                WorkingDirectory = query.WorkingDirectory,
                WindowStyle = query.StartMode.AsWindowsStyle(),
            };

            if (query.IsElevated || query.RunAs == Constants.RunAs.Admin)
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
            var psi = new ProcessStartInfo();

            if (query.IsElevated)
            {
                psi.FileName = file;
                //https://stackoverflow.com/a/23199505/389529
                psi.UseShellExecute = true;
                psi.Verb = "runas";
                _log.Info($"Runs '{query.FileName}' as ADMIN");
            }
            else
            {
                psi.FileName = $"explorer.exe";
                psi.Arguments = file;
                psi.UseShellExecute = false;
                _log.Info($"Runs '{query.FileName}'");
            }

            _log.Debug($"Executing packaged application'{file}'");
            Process.Start(psi);

            return QueryResult.NoResult;
        }

        public async Task<ExecutionResponse> ExecuteAsync(ExecutionRequest request)
        {
            if (request is null)
            {
                _log.Trace("The execution request is null.");
                return new()
                {
                    Results = DisplayQueryResult.SingleFromResult("This alias does not exist"),
                    HasResult = true,
                };
            }
            if (request.QueryResult is not IExecutable)
            {
                _log.Trace($"Alias '{request.QueryResult?.Name ?? "<EMPTY>"}', is not executable. Return '{nameof(ExecutionResponse.NoResult)}'.");
                return ExecutionResponse.NoResult;
            }

            _log.Info($"Executing alias '{request.QueryResult.Name}'");
            _dataService.SetUsage(request.QueryResult);
            switch (request.QueryResult)
            {
                case AliasQueryResult alias:
                    alias.IsElevated = request.ExecuteWithPrivilege;
                    return ExecutionResponse.FromResults(
                          await ExecuteAliasAsync(alias)
                    );

                case ISelfExecutable exec:
                    exec.IsElevated = request.ExecuteWithPrivilege;
                    return ExecutionResponse.FromResults(
                        await exec.ExecuteAsync(_cmdlineManager.BuildFromText(request.Query))
                    );

                default: throw new NotSupportedException($"Cannot execute query result '{(request.QueryResult?.Name ?? "<EMPTY>")}'");
            }
        }

        #endregion Methods
    }
}