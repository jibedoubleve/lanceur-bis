using Lanceur.Ui;
using ReactiveUI;
using System;
using System.Reactive.Disposables;

namespace Lanceur.Views
{
    /// <summary>
    /// Interaction logic for SessionsView.xaml
    /// </summary>
    public partial class SessionsView : IViewFor<SessionsViewModel>
    {
        #region Constructors

        public SessionsView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                ViewModel.ConfirmRemove.RegisterHandler(async interaction =>
                {
                    var result = await Dialogs.YesNoQuestion($"Do you want to remove alias '{interaction.Input}'?");
                    interaction.SetOutput(result.AsBool());
                });

                this.OneWayBind(ViewModel, vm => vm.Sessions, v => v.CbSessions.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Aliases, v => v.Aliases.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.CurrentSession, v => v.CbSessions.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.UpdatedCurrentSession.Name, v => v.TbSessionName.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.UpdatedCurrentSession.Notes, v => v.TbSessionNotes.Text).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.SaveSession, v => v.BtnSaveSession, vm => vm.UpdatedCurrentSession).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.RemoveSession, v => v.BtnRemoveSession, vm => vm.UpdatedCurrentSession).DisposeWith(d);

                ViewModel.Activate.Execute().Subscribe();
            });
        }

        #endregion Constructors
    }
}