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
        private readonly Random _rnd = new();

        private int _width, _height, _mineCount;
        private Cell[,] _cells;
        private long _elapsedMilliseconds = 0;
        private int _lastDisplayedSeconds;
        private int _openedSafeCells;
        private int _flagCount;

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

            _timer = new System.Timers.Timer(10);
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

            _openedSafeCells = 0;
            _flagCount = 0;
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

            InitializeNeighbors();

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

        private void InitializeNeighbors()
        {
            for (int r = 0; r < _height; r++)
            {
                for (int c = 0; c < _width; c++)
                {
                    var cell = _cells[r, c];

                    for (int dr = -1; dr <= 1; dr++)
                    {
                        for (int dc = -1; dc <= 1; dc++)
                        {
                            if (dr == 0 && dc == 0)
                                continue;

                            int nr = r + dr;
                            int nc = c + dc;

                            if (nr >= 0 && nr < _height &&
                                nc >= 0 && nc < _width)
                            {
                                cell.Neighbors.Add(_cells[nr, nc]);
                            }
                        }
                    }
                }
            }
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
                foreach (var neighbor in cell.Neighbors)
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
                int flagCount = 0;

                foreach (var n in cell.Neighbors)
                {
                    if (n.IsFlagged)
                        flagCount++;
                }

                if (flagCount == cell.AdjacentMines)
                {
                    foreach (var neighbor in cell.Neighbors)
                    {
                        if (!neighbor.IsOpened && !neighbor.IsFlagged && neighbor.IsPressed)
                        {
                            FinalizeCellOpen(neighbor);
                        }
                    }
                }
                else
                {
                    foreach (var neighbor in cell.Neighbors)
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
                foreach (var neighbor in cell.Neighbors)
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

            List<Cell> available = new();

            for (int r = 0; r < _height; r++)
            {
                for (int c = 0; c < _width; c++)
                {
                    var cell = _cells[r, c];

                    if (cell != safeCell)
                        available.Add(cell);
                }
            }

            for (int i = available.Count - 1; i > 0; i--)
            {
                int j = _rnd.Next(i + 1);
                (available[i], available[j]) = (available[j], available[i]);
            }

            for (int i = 0; i < _mineCount; i++)
            {
                available[i].IsMine = true;
            }
            RelocateAroundSafeCell(safeCell);
        }

        private void RelocateAroundSafeCell(Cell safeCell)
        {

            var safeNeighbors = safeCell.Neighbors;

            foreach (var neighbor in safeNeighbors)
            {
                RelocateAdjacentMines(neighbor, safeCell);
            }
        }
        private void RelocateAdjacentMines(Cell neighbor, Cell safeCell)
        {
            if (!neighbor.IsMine)
                return;

            var neighborNeighbors = neighbor.Neighbors;
            var safeNeighbors = safeCell.Neighbors;

            HashSet<Cell> safeSet = new(safeNeighbors)
            {
                safeCell
            };

            foreach (var nn in neighborNeighbors)
            {
                if (nn.IsMine)
                    continue;

                if (!safeSet.Contains(nn))
                {
                    neighbor.IsMine = false;
                    nn.IsMine = true;
                    return;
                }
            }
        }

        private void CalculateNumbers()
        {
            foreach (var cell in _cells)
            {
                if (cell.IsMine)
                    continue;

                int count = 0;

                foreach (var neighbor in cell.Neighbors)
                {
                    if (neighbor.IsMine)
                        count++;
                }

                cell.AdjacentMines = count;
            }
        }

        private void FinalizeCellOpen(Cell cell)
        {
            if (cell.IsFlagged ||
                cell.IsOpened ||
                cell.IsUnknown)
            {
                return;
            }

            if (!_gameStarted && _firstOpen)
            {
                PlaceMines(cell);

                CalculateNumbers();

                _gameStarted = true;
                _firstOpen = false;

                StartTimer();
                _timerIsActive = true;
            }

            
            if (cell.IsMine)
            {
                cell.IsPressedBomb = true;

                SetGameOverState(false);

                return;
            }
            FloodFillOpen(cell);

            CheckWin();
        }

        private void FloodFillOpen(Cell start)
        {
            Stack<Cell> stack = new();

            stack.Push(start);

            while (stack.Count > 0)
            {
                var cell = stack.Pop();

                if (cell.IsOpened ||
                    cell.IsFlagged)
                {
                    continue;
                }

                cell.IsOpened = true;

                if (!cell.IsMine)
                {
                    _openedSafeCells++;
                }

                if (cell.AdjacentMines > 0)
                    continue;

                foreach (var neighbor in cell.Neighbors)
                {
                    if (!neighbor.IsOpened &&
                        !neighbor.IsFlagged)
                    {
                        stack.Push(neighbor);
                    }
                }
            }
        }

        private void ToggleFlag(Cell cell)
        {
            if (cell.IsOpened)
                return;

            if (cell.IsFlagged)
            {
                cell.IsFlagged = false;

                cell.IsUnknown = true;

                _flagCount--;
            }
            else if (cell.IsUnknown)
            {
                cell.IsUnknown = false;
            }
            else
            {
                cell.IsFlagged = true;

                _flagCount++;
            }

            _headerPanel.SetFlags(_mineCount - _flagCount);
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
            int totalSafe = _width * _height - _mineCount;

            if (_openedSafeCells == totalSafe)
            {
                SetGameOverState(true);
                _headerPanel.SetFlags(0);
            }
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

            _elapsedMilliseconds+= 10;

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