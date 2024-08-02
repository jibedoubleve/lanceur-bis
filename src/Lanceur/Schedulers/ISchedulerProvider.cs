using System.Reactive.Concurrency;

namespace Lanceur.Schedulers;

public interface ISchedulerProvider
{
    #region Properties

    IScheduler MainThreadScheduler { get; }
    IScheduler TaskpoolScheduler { get; }

    #endregion Properties
}