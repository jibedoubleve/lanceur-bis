using Lanceur.Core.Models;

namespace Lanceur.Core.Managers
{
    public interface ICmdlineProcessor
    {
        Cmdline Process(string commandline);
    }
}