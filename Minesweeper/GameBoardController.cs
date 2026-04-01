using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Minesweeper
{
    public class GameBoardController
    {
        private readonly MainWindow _window;

        private readonly DispatcherTimer _timer = new DispatcherTimer();

        private int _elapsedSeconds = 0;
        private int _width;
        private int _height;

        public GameBoardController(MainWindow window)
        {
            _window = window;

            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) =>
            {
                _elapsedSeconds++;
                int mins = _elapsedSeconds / 60;
                int secs = _elapsedSeconds % 60;
                _window.TimerText.Text = $"{mins:D2}:{secs:D2}";
            };

            _window.BtnRestart.Click += (s, e) =>
            {
                StartGame(_width, _height);
            };

            _window.BtnBackToMenu.Click += (s, e) =>
            {
                StopTimer();
                _window.ShowSettings();
            };
        }

        public void StartGame(int width, int height)
        {
            StopTimer();
            ResetTimer();

            _window.PanelSettings.Visibility = Visibility.Collapsed;
            _window.PanelGame.Visibility     = Visibility.Visible;

            const int tileSize = 20;
            const int margin = 20;
            const int captionBar = 30;

            _window.BoardGrid.RowDefinitions.Clear();
            _window.BoardGrid.ColumnDefinitions.Clear();
            _window.BoardGrid.Children.Clear();

            for (int r = 0; r < height; r++)
            {
                _window.BoardGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(tileSize) });
            }

            for (int c = 0; c < width; c++)
            {
                _window.BoardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(tileSize) });
            }

            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    var btn = new Button
                    {
                        Margin = new Thickness(0.5),
                        Background = System.Windows.Media.Brushes.AliceBlue,
                        BorderBrush = System.Windows.Media.Brushes.LightGray,
                        Padding = new Thickness(0)
                    };

                    Grid.SetRow(btn, r);
                    Grid.SetColumn(btn, c);

                    _window.BoardGrid.Children.Add(btn);
                }
            }

            int boardWidth = width * tileSize;
            int boardHeight = height * tileSize;

            _window.MaxWidth = boardWidth + margin + 7;
            _window.MaxHeight = boardHeight + captionBar + margin + 7;


            if (Application.Current.MainWindow is Window w)
            {
                w.Width = _window.MaxWidth;
                w.Height = _window.MaxHeight;
            }

            StartTimer();
        }

        private void StartTimer()
        {
            _elapsedSeconds = 0;
            _window.TimerText.Text = "00:00";
            _timer.Start();
        }

        private void StopTimer()
        {
            _timer.Stop();
        }

        private void ResetTimer()
        {
            _timer.Stop();
            _elapsedSeconds = 0;
            _window.TimerText.Text = "00:00";
        }
    }
}