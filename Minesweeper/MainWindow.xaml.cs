using System.Diagnostics;
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
            if (Application.Current.MainWindow is Window w)
            {
                w.Left = Size(w.Left, w.Width, newWidth);
                w.Top = Size(w.Top, w.Height, newHeight);

                w.Width = newWidth;
                w.Height = newHeight;
            }
        }

        private double Size(double indent, double oldSize, double newSize)
        {
            double indentToTheCenter = Math.Abs((newSize - oldSize) / 2 - indent);
            if (indentToTheCenter <= 0) return 0;
            return indentToTheCenter;
        }

        private void GameScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;

            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                double delta = 3.0 * e.Delta;
                double newOffset = scrollViewer.HorizontalOffset - delta;

                newOffset = Math.Max(0, Math.Min(newOffset, scrollViewer.ScrollableWidth));
                scrollViewer.ScrollToHorizontalOffset(newOffset);

                e.Handled = true;
            }
        }
    }
}