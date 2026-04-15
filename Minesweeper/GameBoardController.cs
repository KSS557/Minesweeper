using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Minesweeper
{
    public class GameBoardController
    {
        public readonly MainWindow _window;
        private readonly System.Timers.Timer _timer;
        private GameHeaderPanel _headerPanel;

        private int _width, _height, _mineCount;
        private Cell[,] _cells;
        private long _elapsedMilliseconds = 0;
        private int _lastDisplayedSeconds;

        private bool _gameStarted;
        private bool _isGameOver;
        private bool _firstOpen;
        private bool _isWin;
        private bool _timerIsActive;

        public bool IsGameOver => _isGameOver;
        public bool IsWin => _isWin;
        public int DisplaySeconds => (int)(_elapsedMilliseconds / 1000);

        public TimeOnly Time => TimeOnly.FromTimeSpan(TimeSpan.FromMilliseconds(_elapsedMilliseconds));

        public GameBoardController(MainWindow window)
        {
            _window = window;

            InitializeHeader();

            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += Timer_Tick;
            _lastDisplayedSeconds = -1;
        }

        public void StartGame(int width, int height, int mineCount)
        {
            const int tileSize = 32;
            const int borderWidth = 2;
            

            _width = width;
            _height = height;
            _mineCount = mineCount;
            _gameStarted = false;
            _isGameOver = false;
            _firstOpen = true;
            _timerIsActive = false;

            _window.PanelSettings.Visibility = Visibility.Collapsed;
            _window.PanelGame.Visibility = Visibility.Visible;
            _window.BoardCanvas.Children.Clear();

            _elapsedMilliseconds = 0;

            ResetTimer();

            _cells = new Cell[height, width];

            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    int index = r * width + c;

                    double leftPx = c * tileSize + borderWidth;
                    double topPx = r * tileSize + borderWidth;

                    var img = new Image
                    {
                        Name = "cell" + index,
                        Width = tileSize,
                        Height = tileSize,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        SnapsToDevicePixels = true,
                    };

                    RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);

                    Canvas.SetLeft(img, leftPx);
                    Canvas.SetTop(img, topPx);

                    var cell = new Cell { Img = img, Row = r, Col = c, IsMine = false };
                    img.Tag = cell;
                    _cells[r, c] = cell;

                    SetupCellEvents(img, cell, _cells);

                    _window.BoardCanvas.Children.Add(img);
                }
            }

            foreach (var cell in _cells)
            {
                cell.FlagChanged += OnCellFlagChanged;
            }

            double canvasWidth = width * tileSize + 2 * borderWidth;
            double canvasHeight = height * tileSize + 2 * borderWidth;

            _window.BoardCanvas.Width = canvasWidth;
            _window.BoardCanvas.Height = canvasHeight;

            _headerPanel.SetFlags(_mineCount);

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

        private void SetupCellEvents(Image img, Cell cell, Cell[,] cells)
        {
            if (_isGameOver) return;
            img.MouseMove += (s, e) =>
            {
                if (_isGameOver) return;
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    PressedLeftButtonCell(cell);
                }
            };

            img.MouseLeftButtonDown += (s, e) =>
            {
                if (_isGameOver) return;
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    PressedLeftButtonCell(cell);
                }
            };

            img.MouseLeftButtonUp += (s, e) =>
            {
                if (_isGameOver) return;
                PressedUpCell(cell);
            };

            img.MouseLeave += (s, e) =>
            {
                if (_isGameOver) return;
                PressedLeavCell(cell);
            };

            img.MouseRightButtonDown += (s, e) =>
            {
                if (_isGameOver) return;
                ToggleFlag(cell);
            };
        }

        private void PressedLeftButtonCell(Cell cell)
        {
            if (cell.IsOpened)
            {
                foreach (var neighbor in cell.GetNeighbors(this, _height, _width))
                {
                    if (!neighbor.IsOpened && !neighbor.IsFlagged && !neighbor.IsUnknown)
                    {
                        neighbor.IsPressed = true;
                    }
                }
                return;
            }
            if (!cell.IsOpened)
            {
                cell.IsPressed = true;
                return;
            }
        }

        private void PressedUpCell(Cell cell)
        {
            if (cell.IsOpened)
            {
                int flagCount = cell.GetNeighbors(this, _height, _width).Count(n => n.IsFlagged);

                if (flagCount == cell.AdjacentMines)
                {
                    foreach (var neighbor in cell.GetNeighbors(this, _height, _width))
                    {
                        if (!neighbor.IsOpened && !neighbor.IsFlagged && neighbor.IsPressed)
                        {
                            FinalizeCellOpen(neighbor);
                        }
                    }
                }
                else
                {
                    foreach (var neighbor in cell.GetNeighbors(this, _height, _width))
                    {
                        if (!neighbor.IsOpened && !neighbor.IsFlagged && !neighbor.IsUnknown)
                        {
                            neighbor.IsPressed = false;
                        }
                    }
                }
                return;
            }
            if (!cell.IsOpened)
            {
                cell.IsPressed = false;
                FinalizeCellOpen(cell);
                return;
            }
        }

        private void PressedLeavCell(Cell cell)
        {
            if (cell.IsOpened)
            {
                foreach (var neighbor in cell.GetNeighbors(this, _height, _width))
                {
                    if (!neighbor.IsOpened && !neighbor.IsFlagged && !neighbor.IsUnknown)
                    {
                        neighbor.IsPressed = false;
                    }
                }
                return;
            }
            if (!cell.IsOpened)
            {
                cell.IsPressed = false;
                return;
            }
        }

        private void PlaceMines(Cell safeCell)
        {
            Random rnd = new Random();
            int placed = 0;
            while (placed < _mineCount)
            {
                int r = rnd.Next(_height), c = rnd.Next(_width);
                Cell candidate = _cells[r, c];

                if (candidate == safeCell || candidate.IsMine)
                    continue;

                candidate.IsMine = true;
                placed++;

            }

            foreach (var neighbor in safeCell.GetNeighbors(this, _height, _width))
            {
                RelocateAdjacentMines(neighbor, safeCell, rnd);
            }
        }

        private void RelocateAdjacentMines(Cell neighbor, Cell safeCell, Random rnd)
        {
            // Пропускаем, если сосед НЕ мина
            if (!neighbor.IsMine)
                return;

            // Получаем соседей ТЕКУЩЕГО соседа (neighbor)
            var neighborNeighbors = neighbor.GetNeighbors(this, _height, _width);

            // Пропускаем, если соседи neighbor пересекаются с safeCell или ее соседями
            var safeCellNeighbors = safeCell.GetNeighbors(this, _height, _width);
            bool skipRelocation = neighborNeighbors.Any(n => n == safeCell || safeCellNeighbors.Contains(n));


            foreach (var nn in neighborNeighbors)
            {
                if (nn.IsMine) continue;
                if (nn != safeCell && !safeCellNeighbors.Contains(nn))
                {
                    
                    neighbor.IsMine = false;
                    nn.IsMine = true;
                    return; 
                }
            }
        }

        private void CalculateNumbers()
        {
            for (int r = 0; r < _height; r++)
            {
                for (int c = 0; c < _width; c++)
                {
                    if (!_cells[r, c].IsMine)
                    {
                        _cells[r, c].AdjacentMines = _cells[r, c].GetNeighbors(this, _height, _width).Count(n => n.IsMine);
                    }
                }
            }
        }

        private void OpenNeighborZero(Cell cell)
        {
            if (cell.AdjacentMines == 0)
            {
                foreach (var neighbor in cell.GetNeighbors(this, _height, _width))
                {
                    if (!neighbor.IsOpened && !neighbor.IsFlagged && !neighbor.IsUnknown)
                    {
                        FinalizeCellOpen(neighbor);
                    }
                }
            }
        }

        private void FinalizeCellOpen(Cell cell)
        {
            if (cell.IsFlagged || cell.IsOpened || cell.IsUnknown) return;

            if (!_gameStarted && _firstOpen)
            {
                PlaceMines(cell);
                CalculateNumbers();
                _gameStarted = true;
                _firstOpen = false;
                StartTimer();
                _timerIsActive = true;

            }

            cell.IsOpened = true;
            cell.IsPressed = false;

            if (cell.IsMine)
            {
                cell.IsPressedBomb = true;
                SetGameOverState(false);
                return;
            }
            
            if (cell.AdjacentMines == 0)
            {
                FloodFillOpen(cell);
            }

            OpenNeighborZero(cell);

            CheckWin();
            
        }

        private void FloodFillOpen(Cell startCell)
        {
            var stack = new Stack<Cell>();
            stack.Push(startCell);

            while (stack.Count > 0)
            {
                var cell = stack.Pop();
                if (cell.IsOpened || cell.IsFlagged) continue;

                cell.IsOpened = true;

                foreach (var neighbor in cell.GetNeighbors(this, _height, _width))
                {
                    if (!neighbor.IsFlagged && !neighbor.IsOpened)
                    {
                        if (neighbor.AdjacentMines == 0)
                            stack.Push(neighbor);
                        else
                            neighbor.IsOpened = true;
                    }
                }
            }
        }

        private void ToggleFlag(Cell cell)
        {
            if (cell.IsOpened) return;

            if (cell.IsFlagged)
            {
                cell.IsFlagged = false;
                cell.IsUnknown = true;
            }
            else if (cell.IsUnknown)
            {
                cell.IsUnknown = false;
            }
            else
            {
                cell.IsFlagged = true;
            }

        }

        private void OnCellFlagChanged(Cell cell)
        {
            _headerPanel.SetFlags(_mineCount - CountFlags());
        }

        private int CountFlags()
        {
            int count = 0;
            foreach (Image img in _window.BoardCanvas.Children)
            {
                if (img.Tag is Cell cell && cell.IsFlagged)
                    count++;
            }
            return count;
        }

        private void RevealAllMines(bool win)
        {
            foreach (Image img in _window.BoardCanvas.Children.OfType<Image>())
            {
                if (img.Tag is Cell cell)
                {
                    if (cell.IsMine || (cell.IsFlagged && !cell.IsMine) || (cell.IsUnknown && !cell.IsMine))
                    {
                        if (win)
                        {
                            cell.IsFlagged = true;
                        }
                        else
                        {
                            cell.IsOpened = true;
                        }
                    }
                }
            }
        }

        private void CheckWin()
        {
            int opened = 0;
            foreach (Image img in _window.BoardCanvas.Children.OfType<Image>())
            {
                if (img.Tag is Cell cell && cell.IsOpened && !cell.IsMine) opened++;
            }
            int totalSafe = _width * _height - _mineCount;
            if (opened == totalSafe) SetGameOverState(true);
        }

        private void InitializeHeader()
        {
            bool _isFacePressed = false;

            if (_headerPanel != null)
                return;

            _headerPanel = new GameHeaderPanel();

            if (_window.HeaderContainer != null)
            {
                _window.HeaderContainer.Content = _headerPanel.Panel;
            }

            _headerPanel.FaceImage.MouseLeftButtonDown += (s, e) =>
            {
                _headerPanel.SetFace(MinesweeperTextures.FaceSmileClick);
                _isFacePressed = true;
            };

            _headerPanel.FaceImage.MouseLeftButtonUp += (s, e) =>
            {
                if (_isFacePressed)
                {
                    _headerPanel.SetFace(MinesweeperTextures.FaceSmile);
                    ResetGame();
                    _isFacePressed = false;
                }
            };

            _headerPanel.FaceImage.MouseLeave += (s, e) =>
            {
                if (_isFacePressed)
                {
                    _headerPanel.SetFace(MinesweeperTextures.FaceSmile);
                    _isFacePressed = false;
                }
                
            };
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (!_timerIsActive || _isGameOver)
            {
                StopTimer();
                return;
            }

            // Функция 1: ПОДСЧЕТ миллисекунд (каждые 1мс)
            _elapsedMilliseconds+= 1000;

            // Функция 2: UI каждую секунду (когда %1000 == 0)
            if (_elapsedMilliseconds % 1000 == 0)
            {
                UpdateTimeDisplay();
            }
        }

        private void UpdateTimeDisplay()
        {
            if (_headerPanel == null) return;
            
                if (DisplaySeconds != _lastDisplayedSeconds)
                {
                    _lastDisplayedSeconds = DisplaySeconds;
                    Application.Current.Dispatcher?.Invoke(() =>
                    {
                        _headerPanel.SetTime(DisplaySeconds);
                    });
                    Debug.WriteLine(DisplaySeconds);
                }
        }

        private void StartTimer()
        {
            _elapsedMilliseconds = 0;
            Application.Current.Dispatcher.Invoke(() =>
            {
                _headerPanel.SetTime(0);
                _headerPanel.SetFace(MinesweeperTextures.FaceSmile);
            });

            _timer.Start();
        }

        public void StopTimer()
        {
            _timer.Stop();
        }

        public void ContinueTimer()
        {
            _timer.Start();
        }

        private void ResetTimer()
        {
            StartTimer();
            StopTimer();
            _gameStarted = false;
        }

        public void ResetGame()
        {
            _isGameOver = false;
            ResetTimer();
            StartGame(_width, _height, _mineCount);
        }

        public void SetGameOverState(bool win)
        {
            if (_isGameOver) return;
            _isGameOver = true;
            _isWin = win;
            StopTimer();
            _timerIsActive = false;

            _gameStarted = false;
            if (win)
            {
                _headerPanel.SetFace(MinesweeperTextures.FaceWin);
                _window.ShowWinOverlay();
                RevealAllMines(win);
            }
            else
            {
                RevealAllMines(win);
                _headerPanel.SetFace(MinesweeperTextures.FaceLose);
                _window.ShowLoseOverlay();
            }
        }


    }
}