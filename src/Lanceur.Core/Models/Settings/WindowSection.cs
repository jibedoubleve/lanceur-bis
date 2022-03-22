namespace Lanceur.Core.Models.Settings
{
    public class WindowSection
    {
        #region Properties

        public PositionSection Position { get; set; } = new PositionSection();
        public int ScoreLimit { get; set; }

        #endregion Properties
    }
}