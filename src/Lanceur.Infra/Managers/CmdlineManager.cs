using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Infra.Managers
{
    public class CmdlineManager : ICmdlineManager
    {
        #region Fields

        private static readonly string[] Specials = {
            "$", "&", "|", "@", "#", "(", ")", "§", "!", "{", "}", "-", "_", "\\", "+", "-", "*", "/", "=", "<", ">", ",", ";", ":", "%", "?"
        };

        #endregion Fields

        #region Methods

        private static string GetSpecialName(string cmdline)
        {
            if (cmdline.Length > 1 && cmdline[0] == cmdline[1])
            {
                return cmdline[..2];
            }
            else if (cmdline.Length > 0)
            {
                return cmdline[..1];
            }
            else { throw new ArgumentException($"Cmdline is too short (length {cmdline.Length})"); }
        }

        private static bool HasSpecialName(string cmdName)
        {
            cmdName = cmdName.Trim();
            var res1 = false;
            var res2 = false;

            if (cmdName.Length > 0)
            {
                res1 = Specials.Contains(cmdName[..1]);
            }
            if (cmdName.Length > 1)
            {
                res2 = Specials.Contains(cmdName[..2]);
            }

            return res1 || res2;
        }

        public Cmdline Build(string cmdline, ExecutableQueryResult query)
        {
            var cmd = BuildFromText(cmdline);
            if (cmd.Parameters.IsNullOrWhiteSpace())
            {
                cmd = BuildFromText(query.ToQuery());
            }
            return cmd;
        }

        public Cmdline BuildFromText(string cmdline)
        {
            var name = string.Empty;
            var args = string.Empty;
            cmdline ??= string.Empty;

            if (HasSpecialName(cmdline))
            {
                name = GetSpecialName(cmdline);
                args = cmdline[name.Length..];
            }
            else
            {
                var elements = cmdline.Split(" ");
                if (elements.Length > 0)
                {
                    name = elements[0];
                    args = cmdline[name.Length..];
                }
            }
            return new Cmdline(name, args.Trim());
        }

        public Cmdline CloneWithNewParameters(string newParameters, Cmdline cmd) => BuildFromText($"{cmd?.Name} {newParameters}");

        #endregion Methods
    }
}