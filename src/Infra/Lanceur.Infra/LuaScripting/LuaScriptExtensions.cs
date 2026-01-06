using System.Collections;
using Lanceur.Core.LuaScripting;
using Lanceur.Core.Services;
using NLua;

namespace Lanceur.Infra.LuaScripting;

internal static class LuaScriptExtensions
{
    extension(Lua lua)
    {
        #region Methods

        public Lua AddApi()
        {
            var lfsInstance = new LuaFileSystem();
            lua.LoadCLRPackage();
            lua["lfs"] = new Dictionary<string, object>
            {
                ["attributes"] = new Func<string, object>(lfsInstance.Attributes),
                ["exists"] = new Func<string, bool>(lfsInstance.Exists),
                ["chdir"] = new Func<string, bool>(lfsInstance.Chdir),
                ["currentdir"] = new Func<string>(lfsInstance.Currentdir),
                ["dir"] = new Func<string, IEnumerable>(lfsInstance.Dir),
                ["mkdir"] = new Func<string, bool>(lfsInstance.Mkdir),
                ["rmdir"] = new Func<string, bool>(lfsInstance.Rmdir)
            };
            lua["get_env"] = new Func<string, string>(Environment.GetEnvironmentVariable);
            lua["path_combine"] = new Func<string, string, string>(Path.Combine);
            return lua;
        }

        public Lua AddContext(Script script)
        {
            lua["context"] = script.Context;
            return lua;
        }

        public Lua AddOutput(LuaScriptOutput logger)
        {
            lua["output"] = new Dictionary<string, object>
            {
                ["appendLine"] = new Action<string>(logger.AppendLine)
            };
            return lua;
        }

        public Lua AddNotifications(IUserGlobalNotificationService notificationService)
        {
            lua["notification"] = new NotificationScriptAdapter(notificationService);
            return lua;
        }

        public void AssertInjections()
        {
            lua.DoString(
                """
                    assert(get_env, 'get_env not injected')
                    assert(path_combine, 'path_combine not injected')
                    assert(lfs, 'lfs not injected')
                """
            );
        }

        #endregion
    }
}