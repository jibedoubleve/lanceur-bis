using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Utils;
using Lanceur.Models;
using Lanceur.Ui;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Lanceur.Views
{
    public class SessionsViewModel : RoutableViewModel
    {
        #region Fields

        private readonly IDataService _aliasService;
        private readonly Interaction<string, bool> _confirmRemove;
        private readonly IAppLogger _log;
        private readonly IThumbnailManager _thumbnailManager;

        #endregion Fields

        #region Constructors

        public SessionsViewModel(
            IScheduler uiThread = null,
            IScheduler poolThread = null,
            IAppLoggerFactory logFactory = null,
            IDataService aliasService = null,
            IUserNotification notify = null,
            IThumbnailManager thumbnailManager = null)
        {
            var l = Locator.Current;
            notify ??= l.GetService<IUserNotification>();
            _log = l.GetLogger<SessionsViewModel>(logFactory);
            _aliasService = aliasService ?? l.GetService<IDataService>();
            _thumbnailManager = thumbnailManager ?? l.GetService<IThumbnailManager>();

            uiThread ??= RxApp.MainThreadScheduler;
            poolThread ??= RxApp.TaskpoolScheduler;

            _confirmRemove = Interactions.YesNoQuestion(uiThread);

            Activate = ReactiveCommand.Create<Unit, ActivationContext>(OnActivate, outputScheduler: uiThread);
            Activate.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            LoadAliases = ReactiveCommand.Create<long, IEnumerable<QueryResult>>(OnLoadAliases, outputScheduler: uiThread);
            LoadAliases.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            var canUseSession = this.WhenAnyValue(x => x.CurrentSession).Select(x => x is not null).ObserveOn(uiThread);
            SaveSession = ReactiveCommand.Create<SessionModel, SessionModel>(OnSaveSession, canUseSession, outputScheduler: uiThread);
            SaveSession.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            RemoveSession = ReactiveCommand.CreateFromTask<SessionModel, SessionModel>(OnRemoveSessionAsync, canUseSession, outputScheduler: uiThread);
            RemoveSession.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            this.WhenAnyObservable(vm => vm.Activate)
                .ObserveOn(uiThread)
                .Subscribe(ctx =>
                {
                    Sessions = ctx.Sessions ?? new();
                    CurrentSession = (from s in Sessions
                                      where s.Id == ctx.IdSession
                                      select s).FirstOrDefault();
                });

            this.WhenAnyObservable(vm => vm.LoadAliases)
                .ObserveOn(uiThread)
                .BindTo(this, vm => vm.Aliases);

            this.WhenAnyObservable(vm => vm.RemoveSession)
                .ObserveOn(uiThread)
                .Where(ctx => ctx is not null)
                .Subscribe(session =>
                {
                    CurrentSession = null;
                    var del = (from s in Sessions
                               where s.Id == session.Id
                               select s).FirstOrDefault();
                    var deleted = Sessions.Remove(del);
                });

            this.WhenAnyValue(vm => vm.CurrentSession)
                .Where(x => x is not null)
                .DistinctUntilChanged()
                .Select(x => x.Id)
                .InvokeCommand(LoadAliases);

            this.WhenAnyValue(vm => vm.CurrentSession)
                .DistinctUntilChanged()
                .Select(e => e.ToModel())
                .BindTo(this, vm => vm.UpdatedCurrentSession);
        }

        #endregion Constructors

        #region Properties

        public ReactiveCommand<Unit, ActivationContext> Activate { get; }

        [Reactive] public IEnumerable<QueryResult> Aliases { get; set; }

        public Interaction<string, bool> ConfirmRemove => _confirmRemove;
        [Reactive] public Session CurrentSession { get; set; }

        public ReactiveCommand<long, IEnumerable<QueryResult>> LoadAliases { get; }

        public ReactiveCommand<SessionModel, SessionModel> RemoveSession { get; }

        public ReactiveCommand<SessionModel, SessionModel> SaveSession { get; }

        [Reactive] public ObservableCollection<Session> Sessions { get; set; }

        [Reactive] public SessionModel UpdatedCurrentSession { get; set; }

        #endregion Properties

        #region Methods

        private ActivationContext OnActivate(Unit _)
        {
            var sessions = new List<Session>(_aliasService.GetSessions())
            {
                new Session { Name = "<Create new session>", Id = 0 }
            };
            var idSession = _aliasService.GetDefaultSessionId();
            return new()
            {
                Sessions = new ObservableCollection<Session>(sessions),
                IdSession = idSession,
            };
        }

        private IEnumerable<QueryResult> OnLoadAliases(long idSession)
        {
            var aliases = _aliasService.GetAll(idSession);
            _thumbnailManager.RefreshThumbnails(aliases);
            return aliases;
        }

        private async Task<SessionModel> OnRemoveSessionAsync(SessionModel session)
        {
            var delete = await _confirmRemove.Handle(session.Name);
            if (delete)
            {
                _aliasService.Remove(session.ToEntity());
                _log.Trace($"Removed session with id '{session?.Id ?? -1}'");
                Toast.Information($"Session '{session.Name}' removed.");
                return session;
            }
            else { return null; }
        }

        private SessionModel OnSaveSession(SessionModel session)
        {
            if (session is not null)
            {
                _log.Trace($"Save session {session.Id}");
                var entity = session.ToEntity();

                _aliasService.Update(ref entity);

                session.Id = entity.Id;
                Toast.Information($"Session '{session.Name}' updated.");
                return session;
            }
            else
            {
                _log.Warning($"Trying to change the default session to a null.");
                return null;
            }
        }

        #endregion Methods

        #region Classes

        public class ActivationContext
        {
            #region Properties

            public long IdSession { get; internal set; }
            public ObservableCollection<Session> Sessions { get; internal set; }

            #endregion Properties
        }

        #endregion Classes
    }
}