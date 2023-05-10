namespace Lanceur.Core.Models
{
    public abstract class ExecutableQueryResult : QueryResult, IExecutable
    {

        public bool IsElevated { get; set; }

        public string Parameters { get; set; }

    }
}