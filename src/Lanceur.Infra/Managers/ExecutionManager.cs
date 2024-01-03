using Lanceur.Core;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Requests;
using Lanceur.Core.Utils;
using Lanceur.Infra.Logging;
using Lanceur.Infra.LuaScripting;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Lanceur.Infra.Managers
{
    public class ExecutionManager : IExecutionManager
    {
        #region Fields

        private readonly ICmdlineManager _cmdlineManager;
        private readonly IDbRepository _dataService;
        private readonly ILogger<ExecutionManager> _logger;
        private readonly IWildcardManager _wildcardManager;

        #endregion Fields

        #region Constructors

        public ExecutionManager(
            ILoggerFactory logFactory,
            IWildcardManager wildcardManager,
            IDbRepository dataService,
            ICmdlineManager cmdlineManager
        )
        {
            _logger = logFactory.GetLogger<ExecutionManager>();
            _wildcardManager = wildcardManager;
            _dataService = dataService;
            _cmdlineManager = cmdlineManager;
        }

        #endregion Constructors

        #region Methods

        private async Task<IEnumerable<QueryResult>> ExecuteAliasAsync(AliasQueryResult query)
        {
            try
            {
                if (query.Delay > 0)
                {
                    _logger.LogDebug("Delay of {Delay} second(s) before executing the alias", query.Delay);
                    await Task.Delay(query.Delay * 1000);
                }
                if (query.IsUwp())
                {
                    return ExecuteUwp(query);
                }

                return ExecuteProcess(query);
            }
            catch (Exception ex) { throw new ApplicationException($"Cannot execute alias '{query?.Name ?? "NULL"}'. Check the path of the executable or the URL.", ex); }
        }

        private void ExecuteLuaScript(ref AliasQueryResult query)
        {
            using var _ = _logger.BeginSingleScope("Query", query);

            var result = LuaManager.ExecuteScript(new()
            {
                Code = query.LuaScript,
                Context = new()
                {
                    FileName = query.FileName,
                    Parameters = query.Parameters
                }
            });
            using var __ = _logger.BeginSingleScope("ScriptResult", result);
            if (!result.Error.IsNullOrEmpty()) _logger.LogWarning("The Lua script is on error");

            if (result?.Context?.FileName is not null) query.FileName = result.Context.FileName;
            if (result?.Context?.Parameters is not null) query.Parameters = result.Context.Parameters;

            _logger.LogInformation("Lua script executed on {AlisName}", query.Name);
        }

        private IEnumerable<QueryResult> ExecuteProcess(AliasQueryResult query)
        {
            if (query is null) return QueryResult.NoResult;

            query.Parameters = _wildcardManager.ReplaceOrReplacementOnNull(query.Parameters, query.Query.Parameters);
            ExecuteLuaScript(ref query);

            _logger.LogInformation("Executing {FileName} with args {Parameters}", query.FileName, query.Parameters);
            var psi = new ProcessStartInfo
            {
                FileName = _wildcardManager.Replace(query.FileName, query.Parameters),
                Verb = "open",
                Arguments = query.Parameters,
                UseShellExecute = true,
                WorkingDirectory = query.WorkingDirectory,
                WindowStyle = query.StartMode.AsWindowsStyle(),
            };

            if (query.IsElevated || query.RunAs == SharedKernel.Constants.RunAs.Admin)
            {
                psi.Verb = "runas";
                _logger.LogInformation("Run {FileName} as ADMIN", query.FileName);
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
                _logger.LogInformation("Run {FileName} as {Elevated}", query.FileName, "ADMIN");
            }
            else
            {
                psi.FileName = "explorer.exe";
                psi.Arguments = file;
                psi.UseShellExecute = false;
                _logger.LogInformation("Run {FileName} as {Elevated}", query.FileName, "regular user");
            }

            _logger.LogInformation("Run packaged application {File}", file);
            Process.Start(psi);

            return QueryResult.NoResult;
        }

        public async Task<ExecutionResponse> ExecuteAsync(ExecutionRequest request)
        {
            if (request is null)
            {
                _logger.LogInformation("The execution request is null");
                return new()
                {
                    Results = DisplayQueryResult.SingleFromResult("This alias does not exist"),
                    HasResult = true,
                };
            }
            
            var name = request.QueryResult?.Name ?? "<EMPTY>";
            if (request.QueryResult is not IExecutable)
            {
                const string noResult = nameof(ExecutionResponse.NoResult);
                _logger.LogInformation("Alias {Name}, is not executable. Return {NoResult}", name, noResult);
                return ExecutionResponse.NoResult;
            }

            _dataService.SetUsage(request.QueryResult);
            switch (request.QueryResult)
            {
                case AliasQueryResult alias:
                    alias.IsElevated = request.ExecuteWithPrivilege;
                    return ExecutionResponse.FromResults(
                          await ExecuteAliasAsync(alias)
                    );

                case ISelfExecutable exec:
                    _logger.LogInformation("Executing self executable {Name}", name);
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