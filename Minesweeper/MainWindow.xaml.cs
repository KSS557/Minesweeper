using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Minesweeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DifficultySettings _difficulty;
        public readonly GameBoardController _game;
        public readonly Leaderboard _leaderboard;

        public int Width => _difficulty.Width;
        public int Height => _difficulty.Height;
        public int MineCount => _difficulty.MineCount;

        public MainWindow()
        {
            InitializeComponent();
            _difficulty = new DifficultySettings(this);
            _game = new GameBoardController(this);
            _leaderboard = new Leaderboard(this);

            AssistDialogOverlay();
            AssistWinOvwrlay();
            AssistLoseOvwrlay();

            BtnLeaderboard.Click += (s, e) => ShowLeaderbord();
            BtnBackToMenu.Click += (s, e) => ShowSettings();


        }

        public void ShowSettings()
        {
            PanelGame.Visibility = Visibility.Collapsed;
            Leaderboard.Visibility = Visibility.Collapsed;
            PanelSettings.Visibility = Visibility.Visible;
            ResizeMode = ResizeMode.NoResize;

            if (Application.Current.MainWindow is Window w)
            {
                w.ResizeMode = ResizeMode.NoResize;
                ResizeFromCenter(250, 400);
                w.MaxWidth = double.PositiveInfinity;
                w.MaxHeight = double.PositiveInfinity;
            }
        }

        public void ShowLeaderbord()
        {
            PanelSettings.Visibility = Visibility.Collapsed;
            PanelGame.Visibility = Visibility.Collapsed;
            Leaderboard.Visibility = Visibility.Visible;
            ResizeMode = ResizeMode.NoResize;

            if (Application.Current.MainWindow is Window w)
            {
                w.ResizeMode = ResizeMode.NoResize;
                ResizeFromCenter(550, 400);
                w.MaxWidth = double.PositiveInfinity;
                w.MaxHeight = double.PositiveInfinity;
            }
        }

        public void ResizeFromCenter(double newWidth, double newHeight)
        {
            if (Application.Current.MainWindow is Window w)
            {
                w.Left = Size(w.Left, w.Width, newWidth);
                w.Top = Size(w.Top, w.Height, newHeight);

                w.Width = newWidth;
                w.Height = newHeight;
            }
        }

        private double Size(double indent, double oldSize, double newSize)
        {
            double indentToTheCenter = Math.Abs((newSize - oldSize) / 2 - indent);
            if (indentToTheCenter <= 0) return 0;
            return indentToTheCenter;
        }

        private void GameScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;

            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                double delta = 3.0 * e.Delta;
                double newOffset = scrollViewer.HorizontalOffset - delta;

                newOffset = Math.Max(0, Math.Min(newOffset, scrollViewer.ScrollableWidth));
                scrollViewer.ScrollToHorizontalOffset(newOffset);

                e.Handled = true;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape && WinOverlay.Visibility == Visibility.Collapsed && LoseOverlay.Visibility == Visibility.Collapsed)
            {
                if (DialogOverlay.Visibility == Visibility.Collapsed)
                    ShowDialogOverlay();
                else
                    HideDialogOverlay();

                e.Handled = true;
            }

            base.OnKeyDown(e);
        }

        private void AssistDialogOverlay()
        {
            BtnResume.Click += (s, e) => HideDialogOverlay();
            BtnQuit.Click += (s, e) =>
            {
                HideDialogOverlay();
                ShowSettings();
            };
        }

        private void AssistWinOvwrlay()
        {
            BtnWinLeaderBoard.Click += (s, e) =>
            {
                //TODO лидер борд
            };
            BtnWinQuit.Click += (s, e) =>
            {
                HideWinOverlay();
                ShowSettings();
            };
            BtnWinExit.Click += (s, e) => HideWinOverlay();
        }

        private void AssistLoseOvwrlay()
        {
            BtnLoseRestart.Click += (s, e) =>
            {
                HideLoseOverlay();
                _game.ResetGame();
            };
            BtnLouseQuit.Click += (s, e) =>
            {
                HideLoseOverlay();
                ShowSettings();
            };
            BtnLoseExit.Click += (s, e) => HideLoseOverlay();
        }

        public void ShowDialogOverlay()
        {
            DialogOverlay.Visibility = Visibility.Visible;
            _game.StopTimer();
        }

        public void HideDialogOverlay()
        {
            DialogOverlay.Visibility = Visibility.Collapsed;
            _game.ContinueTimer();
        }

        public void ShowWinOverlay()
        {
            WinOverlay.Visibility = Visibility.Visible;
        }

        public void HideWinOverlay()
        {
            WinOverlay.Visibility = Visibility.Collapsed;
        }

        public void ShowLoseOverlay()
        {
            LoseOverlay.Visibility = Visibility.Visible;
        }

        public void HideLoseOverlay()
        {
            LoseOverlay.Visibility = Visibility.Collapsed;
        }

        private void LeaderboardTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}