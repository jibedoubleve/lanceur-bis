using AutoMapper;
using Lanceur.Core.Models;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace Lanceur.Models
{
    public static class SessionModelMixin
    {
        #region Fields

        private static readonly IMapper Mapper = Locator.Current.GetService<IMapper>();

        #endregion Fields

        #region Methods

        public static Session ToEntity(this SessionModel @this)
        {
            var item = Mapper.Map<SessionModel, Session>(@this);
            return item;
        }

        public static SessionModel ToModel(this Session @this)
        {
            var item = Mapper.Map<Session, SessionModel>(@this);
            return item;
        }

        #endregion Methods
    }

    public sealed class SessionModel : Session
    {
        #region Properties

        [Reactive] public new long Id { get => base.Id; set => base.Id = value; }
        [Reactive] public new string Name { get => base.Name; set => base.Name = value; }
        [Reactive] public new string Notes { get => base.Notes; set => base.Notes = value; }

        #endregion Properties
    }
}