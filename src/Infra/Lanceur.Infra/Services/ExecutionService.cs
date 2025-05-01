using System.Diagnostics;
using Lanceur.Core;
using Lanceur.Core.BusinessLogic;
using Lanceur.Core.LuaScripting;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Requests;
using Lanceur.Core.Responses;
using Lanceur.Core.Services;
using Lanceur.Infra.Utils;
using Lanceur.SharedKernel;
using Lanceur.SharedKernel.Extensions;
using Lanceur.SharedKernel.Logging;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Services;

public class ExecutionService : IExecutionService
{
    #region Fields

    private readonly IAliasRepository _aliasRepository;
    private readonly ILogger<ExecutionService> _logger;
    private readonly ILuaManager _luaManager;
    private readonly IWildcardService _wildcardService;

    #endregion

    #region Constructors

    public ExecutionService(
        ILoggerFactory logFactory,
        IWildcardService wildcardService,
        IAliasRepository aliasRepository,
        ILuaManager luaManager
    )
    {
        _logger = logFactory.GetLogger<ExecutionService>();
        _wildcardService = wildcardService;
        _aliasRepository = aliasRepository;
        _luaManager = luaManager;
    }

    #endregion

    #region Methods

    private async Task<IEnumerable<QueryResult>> ExecuteAliasAsync(AliasQueryResult query)
    {
        try
        {
            if (query.Delay > 0)
            {
                _logger.LogTrace("Delay of {Delay} second(s) before executing the alias", query.Delay);
                await Task.Delay(query.Delay * 1000);
            }

            return query.IsUwp()
                ? ExecuteUwp(query)
                : ExecuteProcess(query);
        }
        catch (Exception ex)
        {
            throw new InvalidDataException(
                $"Cannot execute alias {query?.Name ?? "NULL"}. Check the path of the executable or the URL.",
                ex
            );
        }
    }

    private async Task<ExecutionResponse> ExecuteAsync(ExecutionRequest request, int delay)
    {
        await Task.Delay(delay);
        return await ExecuteAsync(request);
    }

    private ScriptResult ExecuteLuaScript(ref AliasQueryResult query)
    {
        using var _ = _logger.BeginSingleScope("Query", query);

        var result = _luaManager.ExecuteScript(
            new()
            {
                Code = query.LuaScript ?? string.Empty,
                Context = new() { FileName = query.FileName, Parameters = query.Parameters }
            }
        );
        using var __ = _logger.BeginSingleScope("ScriptResult", result);
        if (result.Exception is not null) _logger.LogWarning(result.Exception, "The Lua script is on error");

        query.Parameters = result.Context.Parameters;
        query.FileName = result.Context.FileName;

        _logger.LogInformation("Lua script executed on {AlisName}", query.Name);
        return result;
    }

    private IEnumerable<QueryResult> ExecuteProcess(AliasQueryResult query)
    {
        if (query is null) return QueryResult.NoResult;

        using var _ = _logger.WarnIfSlow(this);

        query.Parameters = _wildcardService.ReplaceOrReplacementOnNull(query.Parameters, query.Query.Parameters);
        var result = ExecuteLuaScript(ref query);
        if (result.IsCancelled)
        {
            _logger.LogInformation("The Lua script has been cancelled. No execution of the alias will be done.");
            return [];
        }

        _logger.LogInformation("Executing {FileName} with args {Parameters}", query.FileName, query.Parameters);
        var psi = new ProcessStartInfo
        {
            FileName = _wildcardService.Replace(query.FileName, query.Parameters),
            Verb = "open",
            Arguments = query.Parameters,
            UseShellExecute = true, // https://stackoverflow.com/a/5255335/389529
            WorkingDirectory = query.WorkingDirectory,
            WindowStyle = query.StartMode.AsWindowsStyle()
        };
        using var __ = _logger.ScopeProcessStartInfo(psi);
        if (query.IsElevated || query.RunAs == Constants.RunAs.Admin)
        {
            psi.Verb = "runas";
            _logger.LogGrantedLevel(query.FileName, "ADMIN");
        }

        using (Process.Start(psi)) { return QueryResult.NoResult; }
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
            _logger.LogGrantedLevel(query.FileName, "ADMIN");
        }
        else
        {
            psi.FileName = "explorer.exe";
            psi.Arguments = file;
            psi.UseShellExecute = false;
            _logger.LogGrantedLevel(query.FileName, "regular user");
        }

        _logger.LogInformation("Run packaged application {File}", file);
        Process.Start(psi);

        return QueryResult.NoResult;
    }

    /// <inheritdoc />
    public async Task<ExecutionResponse> ExecuteAsync(ExecutionRequest request)
    {
        if (request is null)
        {
            _logger.LogInformation("The execution request is null");
            return new()
            {
                Results = DisplayQueryResult.SingleFromResult("This alias does not exist"), HasResult = true
            };
        }

        _logger.LogInformation("Executing alias {AliasName}", request.QueryResult.Name);
        var name = request.QueryResult?.Name ?? "<EMPTY>";
        if (request.QueryResult is not IExecutable)
        {
            const string noResult = nameof(ExecutionResponse.NoResult);
            _logger.LogInformation("Alias {Name}, is not executable. Return {NoResult}", name, noResult);
            return ExecutionResponse.NoResult;
        }

        _aliasRepository.SetUsage(request.QueryResult);
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
                    await exec.ExecuteAsync(Cmdline.Parse(request.Query))
                );

            default:
                throw new NotSupportedException(
                    $"Cannot execute query result '{request.QueryResult?.Name ?? "<EMPTY>"}'"
                );
        }
    }

    /// <inheritdoc />
    public ExecutionResponse ExecuteMultiple(IEnumerable<QueryResult> queryResults, int delay = 0)
    {
        var currentDelay = 0;

        foreach (var queryResult in queryResults)
        {
            currentDelay += delay;
            _ = ExecuteAsync(new() { QueryResult = queryResult }, currentDelay);
        }

        return ExecutionResponse.NoResult;
    }

    /// <inheritdoc />
    public void OpenDirectoryAsync(QueryResult queryResult)
    {
        if (queryResult is not AliasQueryResult alias) return;

        if (!File.Exists(alias.FileName) && !Directory.Exists(alias.FileName)) return;

        var directory = Path.GetDirectoryName(alias.FileName);
        if (directory is not null) Process.Start("explorer.exe", directory);
        if (Directory.Exists(alias.FileName)) Process.Start("explorer.exe", alias.FileName);
    }

    #endregion
}

internal static class LoggerExtensions
{
    #region Methods

    public static void LogGrantedLevel(this ILogger logger, string fileName, string grantedLevel)
        => logger.LogInformation("Run {FileName} as {GrantedLevel}", fileName, grantedLevel.ToUpper());

    #endregion
}