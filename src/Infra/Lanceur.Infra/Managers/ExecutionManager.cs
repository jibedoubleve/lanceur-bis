using System.Diagnostics;
using Lanceur.Core;
using Lanceur.Core.BusinessLogic;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Requests;
using Lanceur.Core.Responses;
using Lanceur.Infra.Logging;
using Lanceur.Infra.LuaScripting;
using Lanceur.Infra.Utils;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Managers;

public class ExecutionManager : IExecutionManager
{
    #region Fields

    private readonly IDbRepository _dataService;
    private readonly ILogger<ExecutionManager> _logger;
    private readonly IWildcardManager _wildcardManager;

    #endregion

    #region Constructors

    public ExecutionManager(
        ILoggerFactory logFactory,
        IWildcardManager wildcardManager,
        IDbRepository dataService
    )
    {
        _logger = logFactory.GetLogger<ExecutionManager>();
        _wildcardManager = wildcardManager;
        _dataService = dataService;
    }

    #endregion

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

            return query.IsUwp()
                ? ExecuteUwp(query)
                : ExecuteProcess(query);
        }
        catch (Exception ex) { throw new ApplicationException($"Cannot execute alias '{query?.Name ?? "NULL"}'. Check the path of the executable or the URL.", ex); }
    }

    private async Task<ExecutionResponse> ExecuteAsync(ExecutionRequest request, int delay)
    {
        await Task.Delay(delay);
        return await ExecuteAsync(request);
    }

    private void ExecuteLuaScript(ref AliasQueryResult query)
    {
        using var _ = _logger.BeginSingleScope("Query", query);

        var result = LuaManager.ExecuteScript(new()
        {
            Code = query.LuaScript ?? string.Empty, Context = new()
            {
                FileName = query.FileName, Parameters = query.Parameters
            }
        });
        using var __ = _logger.BeginSingleScope("ScriptResult", result);
        if (result.Exception is not null) _logger.LogWarning(result.Exception, "The Lua script is on error");

        query.Parameters = result.Context.Parameters;
        query.FileName = result.Context.FileName;

        _logger.LogInformation("Lua script executed on {AlisName}", query.Name);
    }

    private IEnumerable<QueryResult> ExecuteProcess(AliasQueryResult query)
    {
        if (query is null) return QueryResult.NoResult;

        using var _ = _logger.MeasureExecutionTime(this);

        query.Parameters = _wildcardManager.ReplaceOrReplacementOnNull(query.Parameters, query.Query.Parameters);
        ExecuteLuaScript(ref query);

        _logger.LogInformation("Executing {FileName} with args {Parameters}", query.FileName, query.Parameters);
        var psi = new ProcessStartInfo
        {
            FileName = _wildcardManager.Replace(query.FileName, query.Parameters),
            Verb = "open",
            Arguments = query.Parameters,
            UseShellExecute = true, // https://stackoverflow.com/a/5255335/389529
            WorkingDirectory = query.WorkingDirectory,
            WindowStyle = query.StartMode.AsWindowsStyle()
        };
        using var __ = _logger.ScopeProcessStartInfo(psi);
        if (query.IsElevated || query.RunAs == SharedKernel.Constants.RunAs.Admin)
        {
            psi.Verb = "runas";
            _logger.LogInformation("Run {FileName} as ADMIN", query.FileName);
        }

        using (Process.Start(psi))
        {
            _logger.LogInformation("Executing process for alias {AliasName}", query.Name);
            return QueryResult.NoResult;
        }
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
            return new() { Results = DisplayQueryResult.SingleFromResult("This alias does not exist"), HasResult = true };
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
                    await exec.ExecuteAsync(CmdlineManager.BuildFromText(request.Query))
                );

            default: throw new NotSupportedException($"Cannot execute query result '{request.QueryResult?.Name ?? "<EMPTY>"}'");
        }
    }

    public ExecutionResponse ExecuteMultiple(IEnumerable<QueryResult> queryResults, int delay = 0)
    {
        var currentDelay = 0;
        ;
        foreach (var queryResult in queryResults)
        {
            currentDelay += delay;
            _ = ExecuteAsync(new() { QueryResult = queryResult }, currentDelay);
        }

        return ExecutionResponse.NoResult;
    }

    #endregion
}