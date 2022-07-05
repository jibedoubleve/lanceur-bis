using ReactiveUI;
using System.Reactive;
using System.Reactive.Concurrency;

namespace Lanceur.Ui
{
    public static class Interactions
    {
        #region Methods

        public static Interaction<string, bool> YesNoQuestion(IScheduler scheduler = null) => new(scheduler);

        public static Interaction<Unit, string> SelectFile(IScheduler scheduler = null) => new(scheduler);

        #endregion Methods
    }
}