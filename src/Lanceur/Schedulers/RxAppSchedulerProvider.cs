using ReactiveUI;
using System.Reactive.Concurrency;

namespace Lanceur.Schedulers;

public class RxAppSchedulerProvider : ISchedulerProvider
{
    #region Constructors

    public RxAppSchedulerProvider()
    {
        MainThreadScheduler = RxApp.MainThreadScheduler;
        TaskpoolScheduler = RxApp.TaskpoolScheduler;
    }

    #endregion Constructors

    #region Properties

    public IScheduler MainThreadScheduler { get; }
    public IScheduler TaskpoolScheduler { get; }

    #endregion Properties
}