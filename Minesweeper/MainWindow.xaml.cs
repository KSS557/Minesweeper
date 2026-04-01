using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Minesweeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DifficultySettings _difficulty;
        public readonly GameBoardController _game;

        public MainWindow()
        {
            InitializeComponent();
            _difficulty = new DifficultySettings(this);
            _game = new GameBoardController(this);
        }

        public void ShowSettings()
        {
            PanelSettings.Visibility = Visibility.Visible;
            PanelGame.Visibility = Visibility.Collapsed;
        }

        public int Width => _difficulty.Width;
        public int Height => _difficulty.Height;
        public int MineCount => _difficulty.MineCount;
    }
}