using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Minesweeper
{
    public class Cell : INotifyPropertyChanged
    {
        public event Action<Cell> CellOpened;

        private Image _img;
        private bool _isOpened;
        private bool _isFlagged;
        private bool _isUnknown;
        private bool _isMine;
        private int _adjacentMines;
        private bool _isPressed;
        private bool _isPressedBomb;

        public Image Img
        {
            get => _img;
            set
            {
                _img = value;
                UpdateTexture();
            }
        }

        public int Row { get; set; }
        public int Col { get; set; }

        public bool IsOpened
        {
            get => _isOpened;
            set 
            { 
                if (_isOpened == value) return;
                _isOpened = value; OnPropertyChanged(); UpdateTexture();
                if (_isOpened) CellOpened?.Invoke(this);
            }
        }

        public bool IsFlagged
        {
            get => _isFlagged;
            set { _isFlagged = value; OnPropertyChanged(); UpdateTexture(); }
        }

        public bool IsUnknown
        {
            get => _isUnknown;
            set { _isUnknown = value; OnPropertyChanged(); UpdateTexture(); }
        }

        public bool IsMine
        {
            get => _isMine;
            set { _isMine = value; OnPropertyChanged(); UpdateTexture(); }
        }

        public int AdjacentMines
        {
            get => _adjacentMines;
            set { _adjacentMines = value; OnPropertyChanged(); UpdateTexture(); }
        }

        public bool IsPressed 
        {
            get => _isPressed;
            set { _isPressed = value; OnPropertyChanged(); UpdateTexture(); }
        }

        public bool IsPressedBomb
        {
            get => _isPressedBomb;
            set { _isPressedBomb = value; OnPropertyChanged(); UpdateTexture(); }
        }

        public bool HasNumber => IsOpened && AdjacentMines > 0 && AdjacentMines <= 8;
        public bool IsEmpty => IsOpened && AdjacentMines == 0;
        public bool IsBomb => IsOpened && IsMine;

        public List<Cell> GetNeighbors(GameBoardController controller, int totalRows, int totalCols)
        {
            var neighbors = new List<Cell>();
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;
                    int nr = Row + dr, nc = Col + dc;
                    if (nr >= 0 && nr < totalRows && nc >= 0 && nc < totalCols)
                    {
                        int idx = nr * totalCols + nc;
                        var neighborImg = controller._window.BoardCanvas.Children.OfType<Image>()
                            .FirstOrDefault(i => i.Name == "cell" + idx);
                        if (neighborImg?.Tag is Cell n) neighbors.Add(n);
                    }
                }
            }
            return neighbors;
        }

        private void UpdateTexture()
        {
            if (_img == null) return;
            
            if (IsPressedBomb)
            {
                _img.Source = MinesweeperTextures.CellBombClick;
                return;
            }

            if (!IsOpened && IsPressed)
            {
                _img.Source = MinesweeperTextures.CellIsEmpty;
                return;
            }
            if (IsOpened && IsFlagged && IsMine)
            {
                _img.Source = MinesweeperTextures.CellFlag;
                return;
            }
            if (IsOpened && IsFlagged)
            {
                _img.Source = MinesweeperTextures.CellBombWrong;
                return;
            }
            if (IsOpened && IsMine)
            {
                _img.Source = MinesweeperTextures.CellBomb;
                return;
            }
            if (IsOpened && IsUnknown)
            {
                _img.Source = MinesweeperTextures.CellUnknownOpen;
                return;
            }
            

            if (IsOpened)
            {
                _img.Source = GetNumberCell(AdjacentMines);
                return;
            }
            if (IsFlagged)
            {
                _img.Source = MinesweeperTextures.CellFlag;
                return;
            }
            if (IsUnknown)
            {
                _img.Source = MinesweeperTextures.CellUnknown;
                return;
            }
            if (!IsOpened)
            {
                _img.Source = MinesweeperTextures.CellClose;
                return;
            }
            
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}