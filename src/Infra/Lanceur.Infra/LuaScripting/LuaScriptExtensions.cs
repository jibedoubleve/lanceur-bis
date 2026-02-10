using System.Text;
using Lanceur.Core.LuaScripting;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Utils;
using NLua;

namespace Lanceur.Infra.LuaScripting;

internal static class LuaScriptExtensions
{
    extension(Lua lua)
    {
        #region Methods

        public Lua AddClrPackage()
        {
            lua.LoadCLRPackage();
            return lua;
        }

        public Lua AddContext(Script script)
        {
            lua["context"] = script.Context;
            return lua;
        }

        public Lua AddNotifications(IUserGlobalNotificationService notificationService)
        {
            lua["notification"] = new NotificationScriptAdapter(notificationService);
            return lua;
        }

        public Lua AddOutput(TimestampedLogBuffer logger)
        {
            lua["output"] = new Dictionary<string, object> { ["appendLine"] = new Action<string>(logger.AppendLine) };
            return lua;
        }

        public Lua SetEncoding(Encoding encoding)
        {
            lua.State.Encoding = encoding;
            return lua;
        }

        #endregion
    }
}