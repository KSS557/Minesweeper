using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Minesweeper
{
    public class GameBoardController
    {
        public readonly MainWindow _window;
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private GameHeaderPanel _headerPanel;

        private int _elapsedSeconds = 0;

        private int _width, _height, _mineCount;
        private int _flagCount;

        private bool _gameStarted = false;
        private Image _pressedImage = null;

        public GameBoardController(MainWindow window)
        {
            _window = window;

            InitializeHeader();

            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            
        }

       

        public void StartGame(int width, int height, int mineCount)
        {
            const int tileSize = 32;
            const int borderWidth = 2;

            _width = width;
            _height = height;
            _mineCount = mineCount;
            _gameStarted = false;
            _flagCount = 0;

            _window.PanelSettings.Visibility = Visibility.Collapsed;
            _window.PanelGame.Visibility = Visibility.Visible;
            _window.BoardCanvas.Children.Clear();

            Cell[,] cells = new Cell[height, width];

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
                        Name = "cell" + index,
                        Width = tileSize,
                        Height = tileSize,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        SnapsToDevicePixels = true,
                    };

                    RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);

                    // Установить Canvas.Left/Top
                    Canvas.SetLeft(img, leftPx);
                    Canvas.SetTop(img, topPx);

                    var cell = new Cell { Img = img, Row = r, Col = c, IsMine = false };
                    img.Tag = cell;  // Связываем Image с Cell
                    cells[r, c] = cell;

                    SetupCellEvents(img, cell);

                    _window.BoardCanvas.Children.Add(img);
                }
            }

            PlaceMines(cells);
            CalculateNumbers(cells);


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

        private void SetupCellEvents2(Image img, Cell cell)
        {
            bool isPressed = false;
            List<Cell> tempOpenedNeighbors = new();

            img.MouseLeftButtonDown += (s, e) =>
            {
                if (cell.IsFlagged || cell.IsUnknown) return;
                isPressed = true;
                _pressedImage = img;
                // ВРЕМЕННЫЙ визуал нажатия:
                var oldSource = img.Source;
                img.Source = MinesweeperTextures.CellIsEmpty;
                tempOpenedNeighbors.Clear();
                if (cell.IsOpened && cell.HasNumber)
                    OpenNeighborsTemp(cell, tempOpenedNeighbors);
            };

            img.MouseLeftButtonUp += (s, e) =>
            {
                if (isPressed)
                {
                    isPressed = false;
                    _pressedImage = null;
                    ClearTempNeighbors(tempOpenedNeighbors);
                    if (!_gameStarted) { _gameStarted = true; StartTimer(); }
                    FinalizeCellOpen(cell);  // ← Изменяет свойства → текстура авто!
                }
            };

            img.MouseLeave += (s, e) =>
            {
                if (isPressed)
                {
                    isPressed = false;
                    _pressedImage = null;
                    ClearTempNeighbors(tempOpenedNeighbors);
                }
            };

            img.MouseRightButtonDown += (s, e) => ToggleFlag(cell);
        }


        private void SetupCellEvents(Image img, Cell cell)
        {

            img.MouseMove += (s, e) =>
            {
                if(e.LeftButton == MouseButtonState.Pressed)
                {
                    PressedMoveCell(cell);
                }
            };

            img.MouseLeftButtonDown += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    PressedMoveCell(cell);
                }
            };

            img.MouseLeftButtonUp += (s, e) =>
            {
                FinalizeCellOpen(cell);
                PressedUpCell(cell);
            };

            img.MouseLeave += (s, e) =>
            {
                PressedLeavCell(cell);
            };

            img.MouseRightButtonDown += (s, e) => ToggleFlag(cell);
        }

        private void PressedMoveCell(Cell cell)
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
                    // Открываем все безопасные зажатые соседи
                    foreach (var neighbor in cell.GetNeighbors(this, _height, _width))
                    {
                        if (!neighbor.IsOpened && !neighbor.IsFlagged && neighbor.IsPressed)
                        {
                            FinalizeCellOpen(neighbor);  // Полное открытие!
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

        private void PlaceMines(Cell[,] cells)
        {
            Random rnd = new Random();
            int placed = 0;
            while (placed < _mineCount)
            {
                int r = rnd.Next(_height), c = rnd.Next(_width);
                if (!cells[r, c].IsMine)
                {
                    cells[r, c].IsMine = true;
                    placed++;
                }
            }
        }

        private void CalculateNumbers(Cell[,] cells)
        {
            for (int r = 0; r < _height; r++)
            {
                for (int c = 0; c < _width; c++)
                {
                    if (!cells[r, c].IsMine)
                    {
                        cells[r, c].AdjacentMines = cells[r, c].GetNeighbors(this, _height, _width).Count(n => n.IsMine);
                    }
                }
            }
        }

        private void OpenNeighborsTemp(Cell cell, List<Cell> tempList)
        {
            tempList.Clear();
            foreach (var n in cell.GetNeighbors(this, _height, _width))
            {
                if (!n.IsOpened && !n.IsFlagged)  // IsClosed = !IsOpened
                {
                    n.Img.Source = MinesweeperTextures.CellIsEmpty;
                    tempList.Add(n);
                }
            }
        }

        private void ClearTempNeighbors(List<Cell> tempList)
        {
            foreach (var n in tempList)
            {
                if (!n.IsOpened) n.Img.Source = MinesweeperTextures.CellClose;
            }
        }

        private void FinalizeCellOpen(Cell cell)
        {
            if (cell.IsFlagged || cell.IsOpened) return;

            cell.IsOpened = true;
            cell.IsPressed = false;

            if (cell.IsMine)
            {
                cell.IsPressedBomb = true;
                SetGameOverState(false);
                RevealAllMines();
            }
            else
            {
                if (cell.AdjacentMines == 0)
                {
                    FloodFillOpen(cell);
                }
                CheckWin();
            }
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
                            neighbor.IsOpened = true;  // Граница
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
                cell.IsUnknown = true;  // ← Текстура авто!
            }
            else if (cell.IsUnknown)
            {
                cell.IsUnknown = false;  // ← Текстура авто!
            }
            else
            {
                cell.IsFlagged = true;   // ← Текстура авто!
            }

            _headerPanel.SetFlags(_mineCount - CountFlags());
        }


        private void RevealAllMines()
        {
            foreach (Image img in _window.BoardCanvas.Children.OfType<Image>())
            {
                if (img.Tag is Cell cell)
                {
                    if (cell.IsMine || (cell.IsFlagged && !cell.IsMine) || (cell.IsUnknown && !cell.IsMine))
                    {
                        cell.IsOpened = true;
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
                _headerPanel.SetFace(MinesweeperTextures.FaceSmile);
                _isFacePressed = false;
            };
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _elapsedSeconds++;
            _headerPanel.SetTime(_elapsedSeconds);
        }

        private int CountFlags()
        {
            int count = 0;
            foreach (Image img in _window.BoardCanvas.Children)
            {
                if (img.Source.ToString() == MinesweeperTextures.CellFlag.ToString())
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

        public void StopTimer()
        {
            _timer.Stop();
        }

        public void ContyTimer()
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
            ResetTimer();
            StartGame(_width, _height, _mineCount);
        }

        public void SetGameOverState(bool win)
        {
            StopTimer();
            //_headerPanel.SetFace(win ? MinesweeperTextures.FaceWin : MinesweeperTextures.FaceLose);
            if (win)
            {
                _headerPanel.SetFace(MinesweeperTextures.FaceWin);
                _window.ShowWinOverlay();
            }
            else
            {
                _headerPanel.SetFace(MinesweeperTextures.FaceLose);
                _window.ShowLoseOverlay();
            }
        }
        
    }
}