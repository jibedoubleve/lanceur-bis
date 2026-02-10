using System.Text;
using Lanceur.Core.LuaScripting;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Extensions;
using Lanceur.SharedKernel.Utils;
using Microsoft.Extensions.Logging;
using NLua;

namespace Lanceur.Infra.LuaScripting;

public class LuaManager : ILuaManager
{
    #region Fields

    private readonly ILogger<LuaManager> _logger;

    private readonly IUserGlobalNotificationService _notificationService;

    #endregion

    #region Constructors

    public LuaManager(
        IUserGlobalNotificationService notificationService,
        ILogger<LuaManager> logger
    )
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    #endregion

    #region Methods

    public ScriptResult ExecuteScript(Script script)
    {
        if (script.Code.IsNullOrWhiteSpace())
            return new()
            {
                Code = script.Code ?? string.Empty,
                Context = new()
                {
                    FileName = script.Context?.FileName ?? string.Empty,
                    Parameters = script.Context?.Parameters ?? string.Empty
                }
            };

        var output = new TimestampedLogBuffer();
        using var lua = new Lua();
        try
        {
            lua.SetEncoding(Encoding.UTF8)
               .AddClrPackage()
               .AddContext(script)
               .AddOutput(output)
               .AddNotifications(_notificationService);

            var result = lua.DoString(script.Code);

            if (result.Length == 0) return script.ToScriptResult();

            return result[0] is not ScriptContext scriptContext
                ? script.ToScriptResult()
                : new()
                {
                    Code = script.Code,
                    Context = new() { FileName = scriptContext.FileName, Parameters = scriptContext.Parameters },
                    OutputContent = output.ToString()
                };
        }
        catch (Exception e) 
        {
            _logger.LogTrace("Scripts: {Logger}", output.ToString());
            return new() { Code = script.Code, Exception = e, OutputContent = output.ToString() };
        }        
    }

    #endregion
}