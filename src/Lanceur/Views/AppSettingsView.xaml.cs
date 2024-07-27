using Lanceur.Converters.Reactive;
using Microsoft.Win32;
using ReactiveUI;
using System;
using System.Reactive.Disposables;

namespace Lanceur.Views
{
    /// <summary>
    /// Interaction logic for AppSettingsView.xaml
    /// </summary>
    public partial class AppSettingsView : IViewFor<AppSettingsViewModel>
    {
        #region Constructors

        public AppSettingsView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                ViewModel.AskFile.RegisterHandler(interaction =>
                {
                    var ofd = new OpenFileDialog()
                    {
                        Title = "Select the database",
                        Filter = "Databases|*.sqlite;*.db|All files|*.*"
                    };
                    var result = ofd.ShowDialog();
                    var file = (result.HasValue && result.Value) ? ofd.FileName : string.Empty;
                    interaction.SetOutput(file);
                });

                this.OneWayBind(ViewModel, vm => vm.Sessions, v => v.CbSessions.ItemsSource).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.CurrentSession, v => v.CbSessions.SelectedItem).DisposeWith(d);
                //Settings
                this.Bind(ViewModel, vm => vm.DbPath, v => v.TbDatabasePath.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.RestartDelay, v => v.SlRestartDelay.Value).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.HotKeySection, v => v.BoxHotKey.HotKey, v => v.ToMahAppHotKey(), vm => vm.ToHotKeySection()).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ShowResult, v => v.ShowResultToggle.IsOn).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.SaveSettings, v => v.BtnSaveSettings).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SelectDatabase, v => v.BtnSelectDatabase).DisposeWith(d);

                ViewModel.Activate.Execute().Subscribe();
            });
        }

        #endregion Constructors
    }
}