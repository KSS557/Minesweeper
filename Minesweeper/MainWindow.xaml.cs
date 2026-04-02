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
            ResizeMode = ResizeMode.NoResize;

            if (Application.Current.MainWindow is Window w)
            {
                w.ResizeMode = ResizeMode.NoResize;
                ResizeFromCenter(250, 400);
            }
        }

        public int Width => _difficulty.Width;
        public int Height => _difficulty.Height;
        public int MineCount => _difficulty.MineCount;

        public void ResizeFromCenter(double newWidth, double newHeight)
        {
            double currentCenterX = (this.Left + this.Width) / 2;
            double currentCenterY = (this.Top + this.Height) / 2;

            this.Left = (currentCenterX - this.Width) / 2;
            this.Top = (currentCenterY - this.Height) / 2;

            if (Application.Current.MainWindow is Window w)
            {
                w.Width = newWidth;
                w.Height = newHeight;
            }
        }
    }
}