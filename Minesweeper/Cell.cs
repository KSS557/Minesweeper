using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Minesweeper
{
    public class Cell : INotifyPropertyChanged
    {
        private Image _img;
        private bool _isOpened;
        private bool _isFlagged;
        private bool _isUnknown;
        private bool _isMine;
        private int _adjacentMines;

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
            set { _isOpened = value; OnPropertyChanged(); UpdateTexture(); }
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

        public bool HasNumber => IsOpened && AdjacentMines > 0 && AdjacentMines <= 8;
        public bool IsClosed => !IsOpened;
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

            if (IsOpened)
            {
                if (IsMine)
                    _img.Source = MinesweeperTextures.CellBomb;
                else
                    _img.Source = GetNumberCell(AdjacentMines);
            }
            else if (IsFlagged)
                _img.Source = MinesweeperTextures.CellFlag;
            else if (IsUnknown)
                _img.Source = MinesweeperTextures.CellUnknown;
            else
                _img.Source = MinesweeperTextures.CellClose;
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