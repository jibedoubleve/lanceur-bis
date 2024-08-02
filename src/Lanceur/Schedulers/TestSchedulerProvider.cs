using System.Reactive.Concurrency;

namespace Lanceur.Schedulers;

public class TestSchedulerProvider : ISchedulerProvider
{
    #region Constructors

    public TestSchedulerProvider(IScheduler testScheduler)
    {
        MainThreadScheduler = testScheduler;
        TaskpoolScheduler = testScheduler;
    }

    #endregion Constructors

    #region Properties

    public IScheduler MainThreadScheduler { get; }

    public IScheduler TaskpoolScheduler { get; }

    #endregion Properties
}