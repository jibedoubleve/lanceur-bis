namespace Lanceur.SharedKernel.Utils
{
    public static class SingleInstance
    {
        #region Fields

        private static Mutex _mutex = new Mutex(true, @"Global\Lanceur2");

        #endregion Fields

        #region Properties

        public static Mutex Mutex => _mutex;

        #endregion Properties

        #region Methods

        public static void ReleaseMutex() => _mutex.ReleaseMutex();

        public static bool WaitOne()
        {
            try { return _mutex.WaitOne(100); }
            catch (AbandonedMutexException) { return false; }
        }

        #endregion Methods
    }
}