using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Infra.Managers
{
    public class CmdlineManager : ICmdlineManager
    {
        #region Fields

        private static readonly char[] Specials = {
            '$', '&', '|', '@', '#', '(', ')', '§', '!', '{', '}', '-', '_', '\\', '+', '-', '*', '/', '=', '<', '>', ',', ';', ':', '%', '?'
        };

        #endregion Fields

        #region Methods

        private static bool HasSpecialName(string cmdName)
        {
            if (cmdName?.Length > 0)
            {
                return Specials.Contains(cmdName[0]);
            }
            else { return false; }
        }

        public Cmdline Build(string cmdline, ExecutableQueryResult query)
        {
            var cmd = BuildFromText(cmdline);
            if (cmd.Parameters.IsNullOrWhiteSpace())
            {
                cmd = BuildFromText(query.ToString());
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
                name = cmdline[0].ToString();
                args = cmdline[1..];

            }
            else
            {

                var elements = cmdline.Split(' ');
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