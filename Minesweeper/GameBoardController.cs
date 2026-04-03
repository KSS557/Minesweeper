using System.Drawing.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        /*public void StartGame(int width, int height)
        {
            StopTimer();
            ResetTimer();

            _window.PanelSettings.Visibility = Visibility.Collapsed;
            _window.PanelGame.Visibility = Visibility.Visible;

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
                w.ResizeMode = ResizeMode.CanResize;
                w.Width = _window.MaxWidth;
                w.Height = _window.MaxHeight;
            }

            StartTimer();
        }*/

        public void StartGame(int width, int height)
        {
            const int tileSize = 32;   // как в tile0, tile1 и т.п.
            const int borderWidth = 2; // внутренний отступ поля, как в #board.left/top
            const int fieldLeft = 20; // аналог left: 0px у tile0
            const int fieldTop = 108; // аналог top: 108px у #board

            StopTimer();
            ResetTimer();

            _width = width;
            _height = height;

            _window.PanelSettings.Visibility = Visibility.Collapsed;
            _window.PanelGame.Visibility = Visibility.Visible;

            // Очистить поле
            _window.BoardCanvas.Children.Clear();

            // Генерируем клетки как в HTML (#tile0, #tile1, ...)
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    int index = r * width + c;

                    // Позиция в стиле old HTML
                    double leftPx = fieldLeft + c * tileSize + borderWidth;
                    double topPx = fieldTop + r * tileSize + borderWidth;

                    var img = new Image
                    {
                        Name = "tile" + index,
                        Width = tileSize,
                        Height = tileSize,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                    };

                    // Установить Canvas.Left/Top
                    Canvas.SetLeft(img, leftPx);
                    Canvas.SetTop(img, topPx);

                    // Сначала — картинка закрытой клетки
                    // img.Source = closedCellImage;  // твой BitmapImage
                    img.Source = MinesweeperTextures.CellClose;

                    // Обработчик клика (если надо)
                    img.MouseDown += (sender, e) =>
                    {
                        if (e.LeftButton == MouseButtonState.Pressed)
                        {
                            // логика клика по клетке r, c
                        }
                        else if (e.RightButton == MouseButtonState.Pressed)
                        {
                            // логика правого клика
                        }
                    };

                    _window.BoardCanvas.Children.Add(img);
                }
            }

            // Оптимизация: задать размеры Canvas, чтобы ScrollViewer не путался
            double canvasWidth = fieldLeft + width * tileSize + 2 * borderWidth;
            double canvasHeight = fieldTop + height * tileSize + 2 * borderWidth;

            _window.BoardCanvas.Width = canvasWidth;
            _window.BoardCanvas.Height = canvasHeight;

            // Если нужно, подогнать окно под поле (с учётом скролл‑бара, ~17 px)
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                const int scrollbarSize = 17;

                double desiredWidth = canvasWidth + scrollbarSize;
                double desiredHeight = canvasHeight + 80; // плюс верхняя панель + отступы

                _window.MaxWidth = desiredWidth;
                _window.MaxHeight = desiredHeight;

                if (Application.Current.MainWindow is Window w)
                {
                    w.ResizeMode = ResizeMode.CanResize;
                    _window.ResizeFromCenter(_window.MaxWidth, _window.MaxHeight);
                }
            });

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