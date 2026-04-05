using System.Diagnostics;
using System.Drawing.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Minesweeper
{
    public class GameBoardController
    {
        private readonly MainWindow _window;
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private GameHeaderPanel _headerPanel;

        private int _elapsedSeconds = 0;
        private int _width;
        private int _height;

        private int _mineCount;
        private int _flagCount = 0;

        private bool _gameStarted = false;

        public GameBoardController(MainWindow window)
        {
            _window = window;

            InitializeHeader();

            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            
        }

        public void StartGame(int width, int height)
        {
            const int tileSize = 32;
            const int borderWidth = 2;


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
                    double leftPx = c * tileSize + borderWidth;
                    double topPx = r * tileSize + borderWidth;

                    var img = new Image
                    {
                        Name = "tile" + index,
                        Width = tileSize,
                        Height = tileSize,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        SnapsToDevicePixels = true,
                    };

                    RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);

                    // Установить Canvas.Left/Top
                    Canvas.SetLeft(img, leftPx);
                    Canvas.SetRight(img, leftPx);
                    Canvas.SetTop(img, topPx);
                    Canvas.SetBottom(img, topPx);

                    img.Source = MinesweeperTextures.CellClose;

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

            double canvasWidth = width * tileSize + 2 * borderWidth;
            double canvasHeight = height * tileSize + 2 * borderWidth;

            _window.BoardCanvas.Width = canvasWidth;
            _window.BoardCanvas.Height = canvasHeight;

            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                const int scrollbarSize = 17;

                double desiredWidth = canvasWidth + scrollbarSize;
                double desiredHeight = canvasHeight + 82 + scrollbarSize;

                _window.MinWidth = 250;
                _window.MinHeight = 400;

                _window.MaxWidth = desiredWidth;
                _window.MaxHeight = desiredHeight;

                if (Application.Current.MainWindow is Window w)
                {
                    w.ResizeMode = ResizeMode.CanResize;
                    _window.ResizeFromCenter(_window.MaxWidth, _window.MaxHeight);
                }
            });

        }

        private void InitializeHeader()
        {
            if (_headerPanel != null)
                return;

            _headerPanel = new GameHeaderPanel();

            // Подключаем панель в верхний контейнер
            if (_window.HeaderContainer != null)
            {
                _window.HeaderContainer.Content = _headerPanel.Panel;
            }


            _headerPanel.FaceImage.MouseLeftButtonDown+= (s, e) =>
            {
                _headerPanel.FaceImage.Source = MinesweeperTextures.FaceSmileClick;
                e.Handled = true;

            };

            _headerPanel.FaceImage.MouseLeftButtonUp += (s, e) =>
            {
                _headerPanel.FaceImage.Source = MinesweeperTextures.FaceSmile;
                e.Handled = true;
                _window.ShowSettings();

            };

            _headerPanel.FaceImage.MouseLeave += (s, e) =>
            {
                _headerPanel.FaceImage.Source = MinesweeperTextures.FaceSmile;
                e.Handled = true;

            };

            _headerPanel.FaceImage.MouseLeave += (s, e) =>
            {
                if (_gameStarted)
                    _headerPanel.FaceImage.Source = IsGameOver() ?
                        (IsWin() ? MinesweeperTextures.FaceWin : MinesweeperTextures.FaceLose)
                        : MinesweeperTextures.FaceSmile;
            };
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _elapsedSeconds++;
            _headerPanel.SetTime(_elapsedSeconds);
        }

        public void HandleRightClick(int r, int c)
        {
            // После любого изменения флага:
            _flagCount = CountFlags(); // подсчёт флагов

            int remaining = _mineCount - _flagCount;
            _headerPanel.SetFlags(remaining);
        }

        private int CountFlags()
        {
            int count = 0;
            foreach (Image img in _window.BoardCanvas.Children)
            {
                if (img.Source == MinesweeperTextures.CellFlag)
                    count++;
            }
            return count;
        }

        private void StartTimer()
        {
            _elapsedSeconds = 0;
            _headerPanel.SetTime(0);
            _headerPanel.SetFace(MinesweeperTextures.FaceSmile);

            _timer.Start();
        }

        private void StopTimer()
        {
            _timer.Stop();
        }

        private void ResetTimer()
        {
            StopTimer();
            _elapsedSeconds = 0;
            _headerPanel.SetTime(0);
        }

        private bool IsGameOver() => false; // твой код
        private bool IsWin() => false; // твой код

        public void SetGameOverState(bool win)
        {
            StopTimer();

            _headerPanel.SetFace(win ? MinesweeperTextures.FaceWin : MinesweeperTextures.FaceLose);
        }

        private void FaceImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _headerPanel.FaceImage.Source = MinesweeperTextures.FaceSmileClick;
            }
        }

    }
}