namespace Lanceur.Core.Models
{
    /// <summary>
    /// Represents the response when checks whether the names specified is alias names
    /// </summary>
    public record ExistingNameResponse
    {
        public ExistingNameResponse(string[] existingNames)
        {
            ExistingNames = existingNames ?? Array.Empty<string>();
        }

        /// <summary>
        /// List of existing names. Empty if no names exists
        /// </summary>
        public string[] ExistingNames { get; }

        /// <summary>
        /// <c>True</c> if at least one of the names to check already exist; otherwise <c>False</c>
        /// </summary>
        public bool Exists => ExistingNames?.Any() ?? false;
    }
}