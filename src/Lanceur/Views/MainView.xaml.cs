using Lanceur.Converters.Reactive;
using Lanceur.Core.Models;
using Lanceur.Core.Requests;
using Lanceur.Core.Services;
using Lanceur.Core.Services.Config;
using Lanceur.Infra.Utils;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Utils;
using NHotkey;
using NHotkey.Wpf;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Lanceur.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : IViewFor<MainViewModel>
    {
        #region Fields

        private readonly IAppConfigService _settings;
        private bool _isStoryBoardsFree = true;
        public readonly IAppLogger _log;

        #endregion Fields

        #region Constructors

        public MainView() : this(null, null)
        {
        }

        public MainView(IAppLoggerFactory factory, IAppConfigService settings)
        {
            InitializeComponent();

            _log = Locator.Current.GetLogger<MainView>(factory);

            ViewModel = Locator.Current.GetService<MainViewModel>();
            _settings = settings ?? Locator.Current.GetService<IAppConfigService>();
            DataContext = ViewModel;

            Loaded += OnWindowLoaded;
            PreviewKeyDown += OnPreviewKeyDown;
            MouseDown += OnWindowMouseDown;

            QueryTextBox.TextChanged += OnTextChanged;

            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.Query.Value, v => v.QueryTextBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.KeepAlive, v => v.KeepAlive).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.IsBusy, v => v.ProgressBar.Visibility, x => x ? Visibility.Visible : Visibility.Collapsed).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CurrentSessionName, v => v.CurrentSessionName.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CurrentAlias, v => v.QueryResults.SelectedItem).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.Results, v => v.QueryResults.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Results.Count, v => v.ResultPanel.Visibility, x => x.ToVisibility()).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Results.Count, v => v.ResultCounter.Text);
                this.OneWayBind(ViewModel, vm => vm.Results.Count, v => v.StatusPanel.Visibility, x => x.ToVisibility()).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.Suggestion, v => v.AutoCompleteBox.Text).DisposeWith(d);

                ViewModel.WhenAnyValue(vm => vm.KeepAlive)
                         .Where(v => v == false)
                         .Log(ViewModel, $"Hiding control.", v => $"KeepAlive = {v}")
                         .Subscribe(_ => HideControl())
                         .DisposeWith(d);

                #region QueryTextBox

                //Don't forget 'using System.Windows.Controls'
                QueryTextBox
                    .Events().PreviewKeyDown
                    .Where(x => x.Key == Key.Enter)
                    .Select(x =>
                    {
                        var vm = x.OriginalSource
                                  .GetParentDataSource<MainViewModel>();
                        x.Handled = true;
                        return new AliasExecutionRequest
                        {
                            Query = x.OriginalSource.GetTextFromTextbox(),
                            AliasToExecute = vm.CurrentAlias,
                            RunAsAdmin = Keyboard.Modifiers == ModifierKeys.Control
                        };
                    })
                    .InvokeCommand(ViewModel, vm => vm.ExecuteAlias)
                    .DisposeWith(d);

                QueryTextBox
                    .Events().PreviewKeyDown
                    .Where(x => x.Key == Key.Tab)
                    .Select(x =>
                    {
                        x.Handled = true;
                        return Unit.Default;
                    })
                    .InvokeCommand(ViewModel, vm => vm.AutoComplete)
                    .DisposeWith(d);

                #endregion QueryTextBox

                #region Navigation

                QueryTextBox
                    .Events().PreviewKeyDown
                    .Where(x => x.Key == Key.Down)
                    .Select(x =>
                    {
                        x.Handled = true;
                        return Unit.Default;
                    })
                    .InvokeCommand(ViewModel, vm => vm.SelectNextResult)
                    .DisposeWith(d);

                QueryTextBox
                    .Events().PreviewKeyDown
                    .Where(x => x.Key == Key.Up)
                    .Select(x =>
                    {
                        x.Handled = true;
                        return Unit.Default;
                    })
                    .InvokeCommand(ViewModel, vm => vm.SelectPreviousResult)
                    .DisposeWith(d);

                #endregion Navigation

                QueryResults
                    .Events().PreviewMouseLeftButtonUp
                    .Select(x =>
                    {
                        var context = x.OriginalSource as FrameworkElement;
                        var alias = context?.DataContext as QueryResult;
                        var source = (x.Source as ListView).ItemsSource as IEnumerable<QueryResult>;

                        var currentAlias = (from s in source
                                            where s.GetHashCode() == alias.GetHashCode()
                                            select s).FirstOrDefault();

                        return new AliasExecutionRequest
                        {
                            Query = alias?.Name ?? string.Empty,
                            AliasToExecute = currentAlias,
                            RunAsAdmin = false,
                        };
                    })
                    .InvokeCommand(ViewModel, vm => vm.ExecuteAlias)
                    .DisposeWith(d);
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
            _log.Trace("Fade in Storyboard completed...");
            Visibility = Visibility.Visible;
        }

        private void OnFadeOutStoryBoardCompleted(object sender, EventArgs e)
        {
            _isStoryBoardsFree = true;
            _log.Trace("Fade out Storyboard completed...");
            Visibility = Visibility.Collapsed;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
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

                    QueryResults.SelectedItem = selectedItem;
                    QueryResults.ScrollIntoView(selectedItem);
                }
            }
            else if (e.Key == Key.Escape) { HideControl(); }
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
            var stg = _settings.Current;
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
            _log?.Info("Window deactivated");
            HideControl();
        }

        public void HideControl()
        {
            QueryTextBox.Text = string.Empty;

            if (_isStoryBoardsFree)
            {
                _isStoryBoardsFree = false;
                var storyBoard = FindResource("fadeOutStoryBoard") as Storyboard;
                storyBoard.Begin(this);
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