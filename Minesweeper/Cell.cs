using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Minesweeper
{
    public class Cell : INotifyPropertyChanged
    {
        private bool _isMine;
        private bool _isRevealed;
        private bool _isFlagged;
        private int _adjacentMines;

        public int Row { get; set; }
        public int Col { get; set; }

        public bool IsMine
        {
            get => _isMine;
            set
            {
                _isMine = value;
                OnPropertyChanged();
            }
        }

        public bool IsRevealed
        {
            get => _isRevealed;
            set
            {
                _isRevealed = value;
                OnPropertyChanged();
            }
        }

        public bool IsFlagged
        {
            get => _isFlagged;
            set
            {
                _isFlagged = value;
                OnPropertyChanged();
            }
        }

        public int AdjacentMines
        {
            get => _adjacentMines;
            set
            {
                _adjacentMines = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}