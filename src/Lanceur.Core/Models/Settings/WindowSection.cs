﻿namespace Lanceur.Core.Models.Settings
{
    public class WindowSection
    {
        #region Properties

        public static WindowSection Default => new() { Position = PositionSection.Default};
        public PositionSection Position { get; set; } = new PositionSection();

        #endregion Properties
    }
}