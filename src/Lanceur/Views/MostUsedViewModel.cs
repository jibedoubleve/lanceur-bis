using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Ui;
using ReactiveUI;
using System;
using Splat;
using System.Collections.Generic;
using System.Reactive;
using ReactiveUI.Fody.Helpers;

namespace Lanceur.Views
{
    public class MostUsedViewModel : RoutableViewModel
    {
        #region Fields

        private readonly IDataService _service;

        #endregion Fields

        #region Constructors

        public MostUsedViewModel(IDataService service = null, IUserNotification notify = null)
        {
            var l = Splat.Locator.Current;
            _service = service ?? l.GetService<IDataService>();
            notify ??= l.GetService<IUserNotification>();

            Activate = ReactiveCommand.Create(OnActivate);
            Activate.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            this.WhenAnyObservable(vm => vm.Activate)
                .BindTo(this, vm => vm.Aliases);
        }

        private IEnumerable<QueryResult> OnActivate()
        {
            var result = _service.GetMostUsedAliases();
            return result;
        }

        [Reactive]public IEnumerable<QueryResult> Aliases { get; set; }
        public ReactiveCommand<Unit, IEnumerable<QueryResult>> Activate { get; }
        #endregion Constructors
    }
}