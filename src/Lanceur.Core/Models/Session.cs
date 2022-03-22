using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Core.Models
{
    public class Session
    {
        #region Properties

        public string FullName => $"{Name}{(Notes.IsNullOrWhiteSpace() ? string.Empty : $" ({Notes})")}";
        public long Id { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }

        #endregion Properties
    }
}