using Lanceur.Ui;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using Microsoft.Win32;
using System.Windows;

namespace Lanceur.Views
{
    /// <summary>
    /// Interaction logic for PluginsView.xaml
    /// </summary>
    public partial class PluginsView : IViewFor<PluginsViewModel>
    {
        #region Constructors

        public PluginsView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                ViewModel.AskFile.RegisterHandler(interaction =>
                {
                    var ofd = new OpenFileDialog()
                    {
                        Title = "Select the plugin package",
                        Filter = "Plugin package|*.lpk|All files|*.*"
                    };
                    var result = ofd.ShowDialog();
                    var file = (result.HasValue && result.Value) ? ofd.FileName : string.Empty;
                    interaction.SetOutput(file);
                });
                ViewModel.AskWebFile.RegisterHandler(interaction =>
                {
                    var view = new PluginFromWebView
                    {
                        Owner = Window.GetWindow(this),
                        Interaction = interaction
                    };
                    view.ShowDialog();
                });


                this.OneWayBind(ViewModel, vm => vm.PluginManifests, v => v.PluginManifests.ItemsSource).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.Restart, v => v.BtnRestart).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.InstallPlugin, v => v.BtnInstallPlugin).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.InstallPluginFromWeb, v => v.BtnInstallPluginFromWeb).DisposeWith(d);

                ViewModel.Activate.Execute().Subscribe(x =>
                {
                    foreach (var plugin in x.PluginManifests)
                    {
                        plugin.ConfirmRemove.RegisterHandler(async interaction =>
                        {
                            var result = await Dialogs.YesNoQuestion($"Do you want to uninstall the plugin '{interaction.Input}'?");
                            interaction.SetOutput(result.AsBool());
                        });
                    }
                });
            });

        }

        #endregion Constructors
    }
}