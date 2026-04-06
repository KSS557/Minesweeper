using System.Windows;
using System.Windows.Controls;
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
            InitializeBoardCanvasEvents();

            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            
        }

        private void InitializeBoardCanvasEvents()
        {
            _window.BoardCanvas.PreviewMouseUp += (s, e) =>
            {
                if (_pressedImage != null)
                {
                    // Возвращаем нормальную текстуру, если она не открыта буквой/флагом
                    if (_pressedImage.Source == MinesweeperTextures.CellIsEmpty)
                    {
                        _pressedImage.Source = MinesweeperTextures.CellClose;
                    }

                    _pressedImage = null;
                }
            };
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

                    img.Source = MinesweeperTextures.CellClose;

                    RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);

                    // Установить Canvas.Left/Top
                    Canvas.SetLeft(img, leftPx);
                    Canvas.SetTop(img, topPx);

                    var cell = new Cell { Img = img, Row = r, Col = c };
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

        private void SetupCellEvents(Image img, Cell cell)
        {
            bool isPressed = false;
            List<Cell> tempOpenedNeighbors = new();  // Для отката

            img.MouseLeftButtonDown += (s, e) =>
            {
                isPressed = true;
                _pressedImage = img;  // Глобальный pressed

                if (!cell.IsOpened)
                {
                    img.Source = MinesweeperTextures.CellIsEmpty;
                    OpenNeighborsTemp(cell, tempOpenedNeighbors);  // Временно открываем соседей если число
                }
                else if (cell.HasNumber)
                {
                    img.Source = MinesweeperTextures.CellIsEmpty;  // "Нажатие" на число
                    OpenNeighborsTemp(cell, tempOpenedNeighbors);
                }
            };

            img.MouseLeftButtonUp += (s, e) =>
            {
                if (isPressed)
                {
                    isPressed = false;
                    _pressedImage = null;

                    if (!_gameStarted)
                    {
                        _gameStarted = true;
                        StartTimer();
                    }

                    FinalizeCellOpen(cell);  // Финализируем открытие
                    ClearTempNeighbors(tempOpenedNeighbors);
                }
            };

            img.MouseLeave += (s, e) =>
            {
                if (isPressed)
                {
                    img.Source = cell.IsOpened ? GetNumberCell(cell.AdjacentMines) : MinesweeperTextures.CellClose;
                    ClearTempNeighbors(tempOpenedNeighbors);
                    _pressedImage = null;
                    isPressed = false;
                }
            };

            // Правый клик для флага (тоже здесь)
            img.MouseRightButtonUp += (s, e) =>
            {
                ToggleFlag(cell);
            };
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
            if (!cell.IsOpened)
            {
                cell.IsOpened = true;
                if (cell.IsMine)
                {
                    cell.Img.Source = MinesweeperTextures.CellBomb;
                    SetGameOverState(false);  // Проигрыш
                    RevealAllMines();  // Показать все мины
                    return;
                }
                cell.Img.Source = cell.IsMine ? MinesweeperTextures.CellBomb : GetNumberCell(cell.AdjacentMines);
                CheckWin();
                // TODO: Если бомба — game over
            }
            // Для чисел: ничего, соседи уже открыты навсегда? По задаче — только пока зажато
        }

        private void ToggleFlag(Cell cell)
        {
            if (!cell.IsOpened)
            {
                cell.IsFlagged = !cell.IsFlagged;
                cell.Img.Source = cell.IsFlagged ? MinesweeperTextures.CellFlag : MinesweeperTextures.CellClose;
                _flagCount = CountFlags();  // Или инкремент/декремент
                int remaining = _window.MineCount - _flagCount;
                _headerPanel.SetFlags(remaining);
            }
        }

        private void RevealAllMines()
        {
            foreach (Image img in _window.BoardCanvas.Children.OfType<Image>())
            {
                if (img.Tag is Cell cell && cell.IsMine && !cell.IsOpened)
                {
                    img.Source = MinesweeperTextures.CellBomb;
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

            // Подключаем панель в верхний контейнер
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
                    ResetTimer();
                    StartGame(_width, _height, _mineCount);
                    _isFacePressed = false;
                }
            };

            _headerPanel.FaceImage.MouseLeave += (s, e) =>
            {
                _headerPanel.SetFace(MinesweeperTextures.FaceSmile);
                _isFacePressed = false;
            };

            /*_headerPanel.FaceImage.MouseLeave += (s, e) =>
            {
                if (_gameStarted)
                    _headerPanel.FaceImage.Source = IsGameOver() ?
                        (IsWin() ? MinesweeperTextures.FaceWin : MinesweeperTextures.FaceLose)
                        : MinesweeperTextures.FaceSmile;
            };*/
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _elapsedSeconds++;
            _headerPanel.SetTime(_elapsedSeconds);
        }

        /*public void HandleRightClick(int r, int c)
        {
            // После любого изменения флага:
            _flagCount = CountFlags(); // подсчёт флагов

            int remaining = _window.MineCount - _flagCount;
            _headerPanel.SetFlags(remaining);
        }*/

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

        private bool IsGameOver() => false; // твой код
        private bool IsWin() => false; // твой код

        public void SetGameOverState(bool win)
        {
            StopTimer();

            _headerPanel.SetFace(win ? MinesweeperTextures.FaceWin : MinesweeperTextures.FaceLose);
        }

        public void HandleLeftClick(Image img)
        {
            bool _isCellPressed = false;

            img.MouseLeftButtonDown += (s, e) =>
            {
                if (img.Source.ToString() == MinesweeperTextures.CellClose.ToString())
                {
                    img.Source = MinesweeperTextures.CellIsEmpty;
                    _isCellPressed = true;
                }
            };

            img.MouseLeftButtonUp += (s, e) =>
            {
                if (_isCellPressed)
                {
                    if (!_gameStarted)
                    {
                        _gameStarted = true;
                        StartTimer();
                    }

                    if (img.Source.ToString() == MinesweeperTextures.CellClose.ToString())
                    {
                        Random rnd = new Random();
                        int test = rnd.Next(0, 10);
                        BitmapImage img2 = GetNumberCell(test);
                        img.Source = img2;
                        _isCellPressed = false;
                    }
                }
            };

            img.MouseLeave += (s, e) =>
            {
                img.Source = MinesweeperTextures.CellClose;
                _isCellPressed = false;
            };
        }

        public void HandleRightClick(Image img)
        {
            // Правило:
            // - если CellClose -> ставишь флаг
            // - если CellFlag -> убираешь флаг обратно в CellClose
            // - остальное (числа, бомбы и т.п.) не трогаем

            if (img.Source == MinesweeperTextures.CellClose)
            {
                img.Source = MinesweeperTextures.CellFlag;
                _flagCount++;
            }
            else if (img.Source == MinesweeperTextures.CellFlag)
            {
                img.Source = MinesweeperTextures.CellUnknown;
                _flagCount--;
            }
            else if (img.Source == MinesweeperTextures.CellUnknown)
            {
                img.Source = MinesweeperTextures.CellClose;
            }

            // Обновляем счётчик: min_count − flags
            int remaining = Math.Max(0, _window.MineCount - _flagCount);
            _headerPanel.SetFlags(remaining);
        }

        private BitmapImage GetNumberCell(int digit)
        {
            return digit switch
            {
                0 => MinesweeperTextures.CellIsEmpty,
                1 => MinesweeperTextures.Cell1,
                2 => MinesweeperTextures.Cell2,
                3 => MinesweeperTextures.Cell3,
                4 => MinesweeperTextures.Cell4,
                5 => MinesweeperTextures.Cell5,
                6 => MinesweeperTextures.Cell6,
                7 => MinesweeperTextures.Cell7,
                8 => MinesweeperTextures.Cell8,
                _ => MinesweeperTextures.CellBomb,
            };
        }
    }
}