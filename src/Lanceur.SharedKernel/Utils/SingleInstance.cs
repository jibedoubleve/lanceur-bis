namespace Lanceur.SharedKernel.Utils;

public static class SingleInstance
{
    #region Fields

    private static readonly Mutex Mutex = new(true, @"Global\Lanceur2");

    #endregion Fields

    #region Methods

    public static void ReleaseMutex() => Mutex.ReleaseMutex();

    public static bool WaitOne()
    {
        try { return Mutex.WaitOne(100); }
        catch (AbandonedMutexException) { return false; }
    }

    #endregion Methods
}