namespace Lanceur.SharedKernel.Utils;

public static class SingleInstance
{
    #region Properties

    public static Mutex Mutex { get; } = new(true, @"Global\Lanceur2");

    #endregion
}