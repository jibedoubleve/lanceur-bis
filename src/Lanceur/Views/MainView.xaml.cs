using Lanceur.Converters.Reactive;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Utils;
using NHotkey;
using NHotkey.Wpf;
using ReactiveUI;
using Splat;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Lanceur.Views
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : IViewFor<MainViewModel>
    {
        #region Fields

        private readonly IAppSettingsService _settings;
        private bool _isStoryBoardsFree = true;
        public readonly ILogService _log = Locator.Current.GetService<ILogService>();

        #endregion Fields

        #region Constructors

        public MainView() : this(null, null)
        {
        }

        public MainView(ILogService log = null, IAppSettingsService settings = null)
        {
            InitializeComponent();

            _log ??= log;

            ViewModel = Locator.Current.GetService<MainViewModel>();
            _settings = settings ?? Locator.Current.GetService<IAppSettingsService>();
            DataContext = ViewModel;

            Loaded += OnWindowLoaded;
            PreviewKeyDown += OnPreviewKeyDown;
            MouseDown += OnWindowMouseDown;

            QueryTextBox.TextChanged += OnTextChanged;

            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.Query, v => v.QueryTextBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.KeepAlive, v => v.KeepAlive).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.IsBusy, v => v.QueryTextBox.IsReadOnly).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.CurrentAliasSuggestion, v => v.AutoCompleteBox.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CurrentSessionName, v => v.RunSession.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Results, v => v.QueryResults.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Results.Count, v => v.ResultPanel.Visibility, x => x.ToVisibility()).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Results.Count, v => v.ResultCounter.Text);
                this.OneWayBind(ViewModel, vm => vm.Results.Count, v => v.StatusPanel.Visibility, x => x.ToVisibility()).DisposeWith(d);

                //Don't forget 'using System.Windows.Controls'
                QueryTextBox
                    .Events().PreviewKeyDown
                    .Where(x => x.Key == Key.Enter)
                    .Select(x =>
                    {
                        x.Handled = true;
                        return new MainViewModel.ExecutionContext
                        {
                            Query = x.OriginalSource.GetTextFromTextbox(),
                            RunAsAdmin = Keyboard.Modifiers == ModifierKeys.Control
                        };
                    })
                    .InvokeCommand(ViewModel, vm => vm.ExecuteAlias);

                QueryTextBox
                    .Events().PreviewKeyDown
                    .Where(x => x.Key == Key.Up)
                    .Select(x => Unit.Default)
                    .InvokeCommand(ViewModel, vm => vm.SelectPreviousResult);

                QueryTextBox
                    .Events().PreviewKeyDown
                    .Where(x => x.Key == Key.Down)
                    .Select(x => Unit.Default)
                    .InvokeCommand(ViewModel, vm => vm.SelectNextResult);

                QueryTextBox
                    .Events().PreviewKeyDown
                    .Where(x => x.Key == Key.Tab)
                    .Select(x =>
                    {
                        x.Handled = true;
                        return Unit.Default;
                    })
                    .InvokeCommand(ViewModel, vm => vm.AutoCompleteQuery);

                QueryResults
                    .Events().PreviewMouseLeftButtonUp
                    .Select(x => x.OriginalSource.GetQueryFromDataContext())
                    .InvokeCommand(ViewModel, vm => vm.ExecuteAlias);
            });
        }

        #endregion Constructors

        #region Properties

        public bool KeepAlive { get; set; }

        #endregion Properties

        #region Methods

        private void OnFadeInStoryBoardCompleted(object sender, EventArgs e)
        {
            _isStoryBoardsFree = true;
            LogService.Current.Trace("Fade in Storyboard completed...");
            Visibility = Visibility.Visible;
        }

        private void OnFadeOutStoryBoardCompleted(object sender, EventArgs e)
        {
            _isStoryBoardsFree = true;
            LogService.Current.Trace("Fade out Storyboard completed...");
            Visibility = Visibility.Collapsed;
        }

        private async void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            await Task.Delay(10);
            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                var collection = QueryResults.ItemsSource.Cast<object>();

                if (collection.CanNavigate() == false) { e.Handled = false; }
                else
                {
                    var idx = e.Key == Key.Down
                        ? collection.GetNextIndex(QueryResults.SelectedIndex)
                        : collection.GetPreviousIndex(QueryResults.SelectedIndex);

                    var selectedItem = collection.ElementAt(idx);
                    QueryResults.ScrollIntoView(selectedItem);
                }
            }
            else if (e.Key == Key.Escape) { HideControl(force: true); }
        }

        private void OnShowWindow(object sender, HotkeyEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.IsOnError = false;
                ShowWindow();
                if (e != null) { e.Handled = true; }
            }
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var length = e.Changes.ElementAt(0).AddedLength;
            if (sender is TextBox t && length > 1)
            {
                t.CaretIndex = t.Text.Length;
            }
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            //TODO value from settings
            var stg = _settings.Load();
            var hk = stg.HotKey;
            SetShortcut((Key)hk.Key, (ModifierKeys)hk.ModifierKey);

            //var coordinate = ScreenRuler.GetCenterCoordinate(ScreenRuler.DefaultTopOffset);
            var coordinate = new Coordinate(stg.Window.Position.Left, stg.Window.Position.Top);
            ScreenRuler.SetWindowPosition(coordinate);

            ShowWindow();
        }

        private void OnWindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) { DragMove(); }
        }

        private void SetShortcut(Key key, ModifierKeys modifyer)
        {
            try
            {
                HotkeyManager.Current.AddOrReplace("OnShowWindow", key, modifyer, OnShowWindow);
            }
            catch (HotkeyAlreadyRegisteredException ex)
            {
                //Default values
                _log?.Warning(ex.Message);
                SetShortcut(Key.R, ModifierKeys.Shift | ModifierKeys.Windows);
            }
        }

        protected override void OnDeactivated(EventArgs e)
        {
            _log?.Info("Windows deactivated");
            HideControl();
        }

        public void HideControl(bool force = false)
        {
            if (KeepAlive && !force) { return; }
            else if (_isStoryBoardsFree)
            {
                _isStoryBoardsFree = false;
                var storyBoard = FindResource("fadeOutStoryBoard") as Storyboard;
                storyBoard.Begin(this);
                QueryTextBox.Text = string.Empty;
            }
        }

        public void ShowWindow()
        {
            _log?.Info("Window showing");
            ViewModel.Activate.Execute().Subscribe();
            if (_isStoryBoardsFree)
            {
                _isStoryBoardsFree = false;
                var storyBoard = FindResource("fadeInStoryBoard") as Storyboard;
                storyBoard.Begin(this);

                Visibility = Visibility.Visible;
                QueryTextBox.Focus();

                //https://stackoverflow.com/questions/3109080/focus-on-textbox-when-usercontrol-change-visibility
                Dispatcher.BeginInvoke((Action)delegate { Keyboard.Focus(QueryTextBox); });

                Activate();
                Topmost = true;
                Topmost = false;
                Focus();
            }
        }

        #endregion Methods
    }
}