using Lanceur.Views;
using System;
using System.Windows.Forms;
using WpfApplication = System.Windows.Application;

namespace Lanceur.Controls
{
    public sealed class NotifyIconAdapter : IDisposable
    {
        #region Fields

        private readonly NotifyIcon _notifyIcon;

        #endregion Fields

        #region Constructors

        public NotifyIconAdapter()
        {
            var stream = WpfApplication.GetResourceStream(new Uri("pack://application:,,,/Lanceur;component/Assets/appIcon.ico")).Stream;
            _notifyIcon = new NotifyIcon
            {
                Icon = new(stream),
                Visible = true,
                ContextMenuStrip = new(),
            };
            _notifyIcon.ContextMenuStrip.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem( "Show...", null,OnShowImpl),
                new ToolStripMenuItem( "Open settings...", null,OnShowSettingsImpl),
                new ToolStripMenuItem( "Quit",null, OnQuitImpl),
            });
            _notifyIcon.DoubleClick += OnDoubleClick;
        }

        #endregion Constructors

        #region Properties

        public bool Visible
        {
            get => _notifyIcon.Visible; set => _notifyIcon.Visible = value;
        }

        #endregion Properties

        #region Methods

        private void OnDoubleClick(object sender, EventArgs e) => OnShowImpl(sender, e);

        private void OnQuitImpl(object sender, EventArgs e) => WpfApplication.Current.Shutdown();

        private void OnShowImpl(object sender, EventArgs e)
        {
            if (WpfApplication.Current.MainWindow is MainView view)
            {
                view.ShowWindow();
            }
        }

        private void OnShowSettingsImpl(object sender, EventArgs e)
        {
            var view = new SettingsView();
            view.Show();
        }

        public void Dispose()
        {
            _notifyIcon.Dispose();
        }

        #endregion Methods
    }
}