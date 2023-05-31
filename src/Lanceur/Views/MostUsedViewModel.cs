using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Ui;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Reactive;

namespace Lanceur.Views
{
    public class MostUsedViewModel : RoutableViewModel
    {
        #region Fields

        private readonly IDbRepository _service;

        #endregion Fields

        #region Constructors

        public MostUsedViewModel(IDbRepository service = null, IUserNotification notify = null)
        {
            var l = Locator.Current;
            _service = service ?? l.GetService<IDbRepository>();
            notify ??= l.GetService<IUserNotification>();

            Activate = ReactiveCommand.Create(OnActivate);
            Activate.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            this.WhenAnyObservable(vm => vm.Activate)
                .BindTo(this, vm => vm.Aliases);
        }

        #endregion Constructors

        #region Properties

        public ReactiveCommand<Unit, IEnumerable<QueryResult>> Activate { get; }

        [Reactive] public IEnumerable<QueryResult> Aliases { get; set; }

        #endregion Properties

        #region Methods

        private IEnumerable<QueryResult> OnActivate()
        {
            var result = _service.GetMostUsedAliases();
            return result;
        }

        #endregion Methods
    }
}